using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SmokeSystem
{
    /// <summary>
    /// CS2-style volumetric smoke: a voxel grid is flood-filled outward from the detonation
    /// point using per-edge line-of-sight checks, so the cloud naturally stops at walls and
    /// pours through doorways/windows instead of clipping through geometry. The same voxel
    /// occupancy data is used both to spawn visual particles and to answer vision-blocking
    /// queries (GetObscuration / IsPositionInSmoke), matching the "everyone sees the same
    /// smoke" behaviour described for CS2.
    /// </summary>
    public class SmokeVolume : MonoBehaviour
    {
        [Header("Shape")]
        [SerializeField] float radius = 4.5f;
        [SerializeField] float cellSize = 0.4f;
        [SerializeField] LayerMask obstacleMask = ~0;
        [SerializeField] float wallCheckRadius = 0.12f;
        [Tooltip("Roughly how many milliseconds DiscoverCells is allowed to spend per frame before yielding. A time budget (rather than a fixed cell count) keeps the per-frame cost similar whether the space is cramped — few cells reachable at all — or wide open, where nearly every cell needs the pricier origin-visibility check.")]
        [SerializeField] float discoveryMillisecondsPerFrame = 2f;

        [Header("Floor Safety Net")]
        [Tooltip("Extra hard clamp: raycasts straight down from the seed once at detonation and never lets growth go below whatever floor it finds, regardless of what the per-edge checks decide. Disable if you need smoke to flow down stairs/ledges to a lower floor.")]
        [SerializeField] bool clampToFloorBelowSeed = true;
        [SerializeField] float floorSearchDistance = 3f;

        [Header("Timing")]
        [SerializeField] float growthDuration = 1.4f;
        [SerializeField] float lifeDuration = 15f;
        [SerializeField] float dissipateDuration = 2.5f;

        [Header("Visuals")]
        [SerializeField, Range(0f, 1f)] float particlesPerCell = 0.35f;
        [SerializeField] Color smokeTint = new Color(0.8f, 0.8f, 0.82f, 1f);

        [Header("Debug")]
        [SerializeField] bool debugDrawCells = true;
        [SerializeField] bool debugLogStats = true;

        public static readonly List<SmokeVolume> ActiveVolumes = new List<SmokeVolume>();

        // filledCells is "the cloud's shape" (every cell that's part of this detonation).
        // clearedCells is the subset of that shape currently punched out by Disturb().
        // healTimers counts down how long each cleared cell has left before it's eligible to regrow.
        readonly HashSet<Vector3Int> filledCells = new HashSet<Vector3Int>();
        readonly HashSet<Vector3Int> clearedCells = new HashSet<Vector3Int>();
        readonly Dictionary<Vector3Int, float> healTimers = new Dictionary<Vector3Int, float>();

        List<Vector3Int> revealOrder;
        int revealIndex;
        Vector3 origin;
        float floorClampY;
        int targetVoxelCount;
        float cellsPerSecond;
        float cellBudgetAccumulator;
        int blockedNeighborChecks;
        int totalNeighborChecks;
        bool discoveryComplete;

        ParticleSystem smokeParticles;
        ParticleSystem.Particle[] scratchParticles;

        enum State { Inactive, Growing, Idle, Dissipating, Done }
        State state = State.Inactive;
        float stateTimer;

        static readonly Vector3Int[] neighborOffsets =
        {
            new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
            new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1),
        };

        void Awake()
        {
            SetupParticleSystem();
        }

        // Registered via OnEnable/OnDisable rather than Awake/OnDestroy because
        // SmokeGrenadeProjectile keeps this component disabled until the grenade actually
        // detonates — flipping it on is what both starts growth and makes the volume visible
        // to queries like IsPositionInSmoke.
        void OnEnable() => ActiveVolumes.Add(this);
        void OnDisable() => ActiveVolumes.Remove(this);

        /// <summary>Starts the flood-fill simulation from the given world position.</summary>
        public void BeginGrowth(Vector3 worldOrigin)
        {
            origin = worldOrigin;

            // If the grenade detonated flush against geometry (e.g. resting exactly on a floor),
            // the seed cell can end up embedded in/grazing that collider, which makes the very
            // first blocking checks unreliable. Nudge it clear before flood-filling from it.
            if (CellObstructed(origin))
            {
                origin += Vector3.up * (cellSize * 0.5f + wallCheckRadius);
                if (debugLogStats)
                    Debug.Log($"[SmokeVolume] Seed was embedded in geometry, nudged to {origin}", this);
            }

            floorClampY = float.NegativeInfinity;
            if (clampToFloorBelowSeed &&
                Physics.Raycast(origin + Vector3.up * 0.05f, Vector3.down, out RaycastHit floorHit, floorSearchDistance, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                floorClampY = floorHit.point.y + wallCheckRadius;
                if (debugLogStats)
                    Debug.Log($"[SmokeVolume] Floor detected at y={floorHit.point.y:F3} ({floorHit.collider.name}), clamping growth to y >= {floorClampY:F3}", this);
            }
            else if (clampToFloorBelowSeed && debugLogStats)
            {
                Debug.Log("[SmokeVolume] No floor found below seed within floorSearchDistance — floor clamp inactive for this detonation.", this);
            }

            float cellVolume = cellSize * cellSize * cellSize;
            float sphereVolume = (4f / 3f) * Mathf.PI * radius * radius * radius;
            targetVoxelCount = Mathf.Max(1, Mathf.RoundToInt(sphereVolume / cellVolume));
            cellsPerSecond = targetVoxelCount / Mathf.Max(0.01f, growthDuration);
            cellBudgetAccumulator = 0f;

            filledCells.Clear();
            clearedCells.Clear();
            healTimers.Clear();

            blockedNeighborChecks = 0;
            totalNeighborChecks = 0;

            // Discovering which cells are even reachable can mean thousands of physics queries
            // for a large or wide-open volume — spread across frames (see DiscoverCells) rather
            // than paid all at once here, which used to be able to spike a single frame badly,
            // worst of all in open spaces where almost nothing prunes the search early.
            revealOrder = new List<Vector3Int>(targetVoxelCount);
            revealIndex = 0;
            discoveryComplete = false;
            StartCoroutine(DiscoverCells());

            var main = smokeParticles.main;
            main.maxParticles = targetVoxelCount + 64;

            state = State.Growing;
            stateTimer = 0f;
        }

        Vector3 CellToWorld(Vector3Int cell) => origin + new Vector3(cell.x, cell.y, cell.z) * cellSize;

        /// <summary>
        /// True if the straight path between two adjacent cell centers is obstructed. Uses a
        /// thick SphereCast rather than a zero-width Linecast: a plain Linecast can miss thin
        /// geometry (a floor slab, a Plane collider) when one of the endpoints sits almost
        /// exactly on the collider's surface, which is a floating-point razor's edge case a
        /// swept sphere doesn't suffer from.
        /// </summary>
        bool CellPathBlocked(Vector3 fromWorld, Vector3 toWorld)
        {
            Vector3 delta = toWorld - fromWorld;
            float dist = delta.magnitude;
            if (dist < 0.0001f)
                return false;

            return Physics.SphereCast(fromWorld, wallCheckRadius, delta / dist, out _, dist, obstacleMask, QueryTriggerInteraction.Ignore);
        }

        /// <summary>True if the given world position itself overlaps an obstacle (e.g. a cell that lands inside a floor/wall).</summary>
        bool CellObstructed(Vector3 worldPos)
        {
            return Physics.CheckSphere(worldPos, wallCheckRadius, obstacleMask, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// True if the origin has a clear direct line to worldPos. Deliberately a plain
        /// zero-width Linecast rather than CellPathBlocked's thick SphereCast: this only decides
        /// which distance metric a cell gets in DiscoverCells (a shape refinement), not
        /// whether smoke can actually flow there, so it doesn't need the thin-geometry robustness
        /// — and unlike the adjacent-cell checks (cellSize apart), this one runs up to `radius`
        /// long for every newly discovered cell, so the cheaper query type matters a lot here.
        /// </summary>
        bool HasLineOfSight(Vector3 fromWorld, Vector3 toWorld)
        {
            return !Physics.Linecast(fromWorld, toWorld, obstacleMask, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Flood-fills outward from the origin, spread across frames instead of resolved in one
        /// synchronous burst (see BeginGrowth), appending straight into revealOrder as cells are
        /// confirmed reachable so TickGrowth can start revealing the nearest ones before the
        /// furthest ones have even been worked out.
        ///
        /// Each newly discovered cell gets a "distance" used both to order the reveal
        /// (Dijkstra-style, so cells don't billow out faster than they should) and to cap growth
        /// at `radius`. That distance is the straight-line distance from the origin whenever the
        /// origin can actually see the cell directly — which keeps the cloud spherical through
        /// open space, since most cells in a room have a clear view of the origin — and only
        /// falls back to the accumulated hop-path distance for cells that are genuinely shadowed
        /// behind an obstacle. That way it's specifically detours that get penalized, not "not
        /// being on an axis": a plain 6-directional grid alone makes diagonal directions cost up
        /// to ~1.73x their real distance (three hops to cover what a straight corner-to-corner
        /// line does in one), which would flatten the whole cloud toward a rounded cube even with
        /// nothing in the way.
        /// </summary>
        IEnumerator DiscoverCells()
        {
            var visited = new HashSet<Vector3Int> { Vector3Int.zero };
            var pathDistance = new Dictionary<Vector3Int, float> { [Vector3Int.zero] = 0f };
            var frontier = new MinHeap();
            frontier.Push(Vector3Int.zero, 0f);

            var frameBudget = Stopwatch.StartNew();

            while (frontier.Count > 0 && revealOrder.Count < targetVoxelCount)
            {
                Vector3Int cell = frontier.Pop();
                revealOrder.Add(cell);

                Vector3 cellWorld = CellToWorld(cell);
                float cellDist = pathDistance[cell];

                foreach (var offset in neighborOffsets)
                {
                    Vector3Int neighbor = cell + offset;
                    if (visited.Contains(neighbor))
                        continue;

                    Vector3 neighborWorld = CellToWorld(neighbor);

                    // Straight-line distance is always <= any real path distance, so it's a
                    // safe, physics-free early-out before spending any raycasts on this cell.
                    float straightDist = Vector3.Distance(origin, neighborWorld);
                    if (straightDist > radius)
                        continue;
                    if (neighborWorld.y < floorClampY)
                        continue;

                    totalNeighborChecks++;
                    if (CellPathBlocked(cellWorld, neighborWorld) || CellObstructed(neighborWorld))
                    {
                        blockedNeighborChecks++;
                        continue;
                    }

                    // Only pay the detour penalty for cells the origin can't see directly.
                    float neighborDist = HasLineOfSight(origin, neighborWorld)
                        ? straightDist
                        : cellDist + cellSize;
                    if (neighborDist > radius)
                        continue;

                    visited.Add(neighbor);
                    pathDistance[neighbor] = neighborDist;
                    frontier.Push(neighbor, neighborDist);
                }

                if (frameBudget.Elapsed.TotalMilliseconds >= discoveryMillisecondsPerFrame)
                {
                    yield return null;
                    frameBudget.Restart();
                }
            }

            discoveryComplete = true;

            if (debugLogStats)
                Debug.Log($"[SmokeVolume] Discovery finished at {origin}. target={targetVoxelCount}, reachable={revealOrder.Count}, neighbor checks={totalNeighborChecks}, blocked by collider={blockedNeighborChecks}, obstacleMask={obstacleMask.value}", this);
        }

        /// <summary>Minimal binary min-heap used to expand the flood-fill in distance order.</summary>
        class MinHeap
        {
            readonly List<Vector3Int> cells = new List<Vector3Int>();
            readonly List<float> priorities = new List<float>();

            public int Count => cells.Count;

            public void Push(Vector3Int cell, float priority)
            {
                cells.Add(cell);
                priorities.Add(priority);
                int i = cells.Count - 1;
                while (i > 0)
                {
                    int parent = (i - 1) / 2;
                    if (priorities[parent] <= priorities[i])
                        break;
                    Swap(parent, i);
                    i = parent;
                }
            }

            public Vector3Int Pop()
            {
                Vector3Int root = cells[0];
                int last = cells.Count - 1;
                cells[0] = cells[last];
                priorities[0] = priorities[last];
                cells.RemoveAt(last);
                priorities.RemoveAt(last);

                int i = 0;
                int count = cells.Count;
                while (true)
                {
                    int left = i * 2 + 1;
                    int right = i * 2 + 2;
                    int smallest = i;
                    if (left < count && priorities[left] < priorities[smallest]) smallest = left;
                    if (right < count && priorities[right] < priorities[smallest]) smallest = right;
                    if (smallest == i) break;
                    Swap(smallest, i);
                    i = smallest;
                }

                return root;
            }

            void Swap(int a, int b)
            {
                (cells[a], cells[b]) = (cells[b], cells[a]);
                (priorities[a], priorities[b]) = (priorities[b], priorities[a]);
            }
        }

        // Lifecycle: Inactive (waiting for BeginGrowth) -> Growing (revealing cells) ->
        // Idle (fully formed, just waiting out lifeDuration) -> Dissipating (shrinking back
        // to nothing) -> Done (gameObject destroyed). Disturb()/healing run independently of
        // this switch, which is why TickHealing() is called unconditionally below.
        void Update()
        {
            switch (state)
            {
                case State.Growing:
                    TickGrowth();
                    break;
                case State.Idle:
                    stateTimer += Time.deltaTime;
                    if (stateTimer >= lifeDuration)
                    {
                        state = State.Dissipating;
                        stateTimer = 0f;
                    }
                    break;
                case State.Dissipating:
                    TickDissipate();
                    break;
            }

            TickHealing();
        }

        // Reveals cells at a steady rate (cellsPerSecond) instead of all at once. The
        // accumulator keeps the reveal rate correct regardless of frame rate: fractional
        // progress (e.g. 2.7 cells this frame) carries over to the next frame instead of
        // being dropped by FloorToInt every tick.
        void TickGrowth()
        {
            cellBudgetAccumulator += cellsPerSecond * Time.deltaTime;
            int budget = Mathf.FloorToInt(cellBudgetAccumulator);
            cellBudgetAccumulator -= budget;

            while (budget > 0 && revealIndex < revealOrder.Count)
            {
                FillCell(revealOrder[revealIndex]);
                revealIndex++;
                budget--;
            }

            // Gate on discoveryComplete, not just revealIndex catching up to the current
            // revealOrder.Count — otherwise reveal briefly outpacing discovery (before the next
            // batch of cells has been found yet) would look like growth finished early.
            if (discoveryComplete && revealIndex >= revealOrder.Count)
            {
                if (debugLogStats)
                    Debug.Log($"[SmokeVolume] Growth finished: filled={filledCells.Count}/{targetVoxelCount}", this);

                state = State.Idle;
                stateTimer = 0f;
            }
        }

        void FillCell(Vector3Int cell)
        {
            if (!filledCells.Add(cell))
                return;

            if (Random.value <= particlesPerCell)
                SpawnCellParticle(cell);
        }

        // Mirrors TickGrowth's budget idea in reverse: work out how many cells *should* still
        // be filled at this point in the fade (shouldRemain), then trim off however many extra
        // ones are still sitting in filledCells to get there. Which specific cells get picked
        // is arbitrary (HashSet iteration order) — fine here since dissipation is a one-off
        // fade-to-nothing, not something that needs to look directional.
        void TickDissipate()
        {
            stateTimer += Time.deltaTime;
            float t = Mathf.Clamp01(stateTimer / dissipateDuration);
            int shouldRemain = Mathf.RoundToInt(Mathf.Lerp(targetVoxelCount, 0, t));

            if (filledCells.Count > shouldRemain)
            {
                int toRemoveCount = filledCells.Count - shouldRemain;
                var removeList = new List<Vector3Int>(toRemoveCount);
                foreach (var cell in filledCells)
                {
                    removeList.Add(cell);
                    if (removeList.Count >= toRemoveCount)
                        break;
                }
                foreach (var cell in removeList)
                    filledCells.Remove(cell);
            }

            if (t >= 1f || filledCells.Count == 0)
            {
                state = State.Done;
                Destroy(gameObject, 1f);
            }
        }

        void TickHealing()
        {
            if (healTimers.Count == 0)
                return;

            // Once the cloud starts dissipating there's no "back to full" left to heal toward —
            // letting a timer fire here would pop a single cell back to a fresh, full-lifetime
            // particle while everything around it is fading out (or already gone), which reads
            // as a dense patch that refuses to clear.
            if (state == State.Dissipating || state == State.Done)
                return;

            List<Vector3Int> healedNow = null;
            var keys = new List<Vector3Int>(healTimers.Keys);
            foreach (var cell in keys)
            {
                float remaining = healTimers[cell] - Time.deltaTime;
                if (remaining <= 0f)
                {
                    (healedNow ??= new List<Vector3Int>()).Add(cell);
                }
                else
                {
                    healTimers[cell] = remaining;
                }
            }

            if (healedNow == null)
                return;

            foreach (var cell in healedNow)
            {
                healTimers.Remove(cell);
                clearedCells.Remove(cell);

                // The particle(s) that used to represent this cell were killed off in Disturb(),
                // so the visual needs to be re-spawned to match the logic coming back online.
                if (Random.value <= particlesPerCell)
                    SpawnCellParticle(cell);
            }
        }

        void SpawnCellParticle(Vector3Int cell)
        {
            var emitParams = new ParticleSystem.EmitParams
            {
                position = CellToWorld(cell) + Random.insideUnitSphere * (cellSize * 0.35f),
                startSize = cellSize * Random.Range(2.2f, 3.4f),
                startLifetime = lifeDuration + dissipateDuration,
                startColor = smokeTint,
            };
            smokeParticles.Emit(emitParams, 1);
        }

        /// <summary>Punches a temporary hole in the smoke (explosions, bullet impacts). The hole heals back after healSeconds.</summary>
        public void Disturb(Vector3 worldPos, float clearRadius, float healSeconds = 3f)
        {
            float sqRadius = clearRadius * clearRadius;
            foreach (var cell in filledCells)
            {
                if ((CellToWorld(cell) - worldPos).sqrMagnitude <= sqRadius)
                {
                    clearedCells.Add(cell);
                    healTimers[cell] = healSeconds;
                }
            }

            KillParticlesNear(worldPos, clearRadius);
        }

        /// <summary>
        /// Finds any live smoke particle within clearRadius of worldPos and fast-forwards it to
        /// the tail of its colorOverLifetime curve so it fades out almost immediately, instead of
        /// only updating the logic-side occupancy while the old puff keeps rendering in place.
        /// </summary>
        void KillParticlesNear(Vector3 worldPos, float clearRadius)
        {
            int count = smokeParticles.particleCount;
            if (count == 0)
                return;

            if (scratchParticles == null || scratchParticles.Length < count)
                scratchParticles = new ParticleSystem.Particle[Mathf.Max(count, 64)];

            int actual = smokeParticles.GetParticles(scratchParticles);
            float sqRadius = clearRadius * clearRadius;
            const float fadeOutTime = 0.2f;
            bool changed = false;

            for (int i = 0; i < actual; i++)
            {
                if (scratchParticles[i].remainingLifetime > fadeOutTime &&
                    (scratchParticles[i].position - worldPos).sqrMagnitude <= sqRadius)
                {
                    scratchParticles[i].remainingLifetime = fadeOutTime;
                    changed = true;
                }
            }

            if (changed)
                smokeParticles.SetParticles(scratchParticles, actual);
        }

        /// <summary>Disturbs every active smoke volume within reach of worldPos. Convenience wrapper for hit-detection callers that don't track which volume they hit.</summary>
        public static void DisturbAt(Vector3 worldPos, float clearRadius, float healSeconds = 3f)
        {
            for (int i = 0; i < ActiveVolumes.Count; i++)
            {
                SmokeVolume volume = ActiveVolumes[i];
                float reach = volume.radius + clearRadius;
                if ((volume.origin - worldPos).sqrMagnitude <= reach * reach)
                    volume.Disturb(worldPos, clearRadius, healSeconds);
            }
        }

        bool IsCellActive(Vector3Int cell) => filledCells.Contains(cell) && !clearedCells.Contains(cell);

        Vector3Int WorldToCell(Vector3 world)
        {
            Vector3 local = (world - origin) / cellSize;
            return new Vector3Int(Mathf.RoundToInt(local.x), Mathf.RoundToInt(local.y), Mathf.RoundToInt(local.z));
        }

        /// <summary>True if the given world position currently sits inside active (unhealed) smoke.</summary>
        public bool IsPositionInSmoke(Vector3 worldPos)
        {
            if (state == State.Inactive || state == State.Done)
                return false;
            if ((worldPos - origin).sqrMagnitude > radius * radius)
                return false;
            return IsCellActive(WorldToCell(worldPos));
        }

        /// <summary>Fraction (0-1) of the line between two points that passes through this volume's active smoke.</summary>
        public float GetObscuration(Vector3 viewerPos, Vector3 targetPos)
        {
            float dist = Vector3.Distance(viewerPos, targetPos);
            if (dist < 0.01f)
                return IsPositionInSmoke(viewerPos) ? 1f : 0f;

            int steps = Mathf.Max(1, Mathf.CeilToInt(dist / cellSize));
            int hits = 0;
            for (int i = 0; i <= steps; i++)
            {
                Vector3 p = Vector3.Lerp(viewerPos, targetPos, (float)i / steps);
                if (IsPositionInSmoke(p))
                    hits++;
            }
            return (float)hits / (steps + 1);
        }

        /// <summary>Highest obscuration between viewer and target across every active smoke volume in the scene.</summary>
        public static float GetTotalObscuration(Vector3 viewerPos, Vector3 targetPos)
        {
            float best = 0f;
            foreach (var volume in ActiveVolumes)
                best = Mathf.Max(best, volume.GetObscuration(viewerPos, targetPos));
            return best;
        }

        // Configured entirely in code instead of on a serialized ParticleSystem asset, so the
        // whole smoke module (this + the generated texture in SmokeTextureUtility + the custom
        // shader) drops into a project with zero external asset dependencies. Position/size/
        // color for every particle are supplied per-emit in SpawnCellParticle, so time-based
        // emission is turned off and the shape module (which would otherwise pick spawn
        // positions itself) is disabled.
        void SetupParticleSystem()
        {
            smokeParticles = gameObject.AddComponent<ParticleSystem>();

            var main = smokeParticles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = 0f;
            main.startSize = 1f;
            main.startLifetime = lifeDuration + dissipateDuration;
            main.maxParticles = 512;

            var emission = smokeParticles.emission;
            emission.rateOverTime = 0f;
            emission.rateOverDistance = 0f;

            var shape = smokeParticles.shape;
            shape.enabled = false;

            var colorOverLifetime = smokeParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.9f, 0.12f),
                    new GradientAlphaKey(0.75f, 0.8f),
                    new GradientAlphaKey(0f, 1f),
                });
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = smokeParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.6f, 1f, 1.15f));

            var renderer = smokeParticles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = BuildSmokeMaterial();
            renderer.alignment = ParticleSystemRenderSpace.View;
        }

        static Material sharedSmokeMaterial;

        // A stock URP Lit/Unlit shader needs several surface-type keywords and blend properties
        // set in lockstep to render as soft, alpha-blended particles, which is fragile to
        // replicate from script. Shipping a small dedicated shader (SmokeParticleUnlit) that's
        // alpha-blended by construction sidesteps that entirely.
        static Material BuildSmokeMaterial()
        {
            if (sharedSmokeMaterial != null)
                return sharedSmokeMaterial;

            Shader shader = Shader.Find("Custom/SmokeParticleUnlit");
            var mat = new Material(shader) { name = "SmokeParticle (Generated)" };
            mat.SetTexture("_MainTex", SmokeTextureUtility.GetSoftCircleTexture());
            sharedSmokeMaterial = mat;
            return mat;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.7f, 0.7f, 0.9f, 0.4f);
            Gizmos.DrawWireSphere(state == State.Inactive ? transform.position : origin, radius);

            if (!debugDrawCells || state == State.Inactive || filledCells == null)
                return;

            float cubeSize = cellSize * 0.85f;
            foreach (var cell in filledCells)
            {
                bool cleared = clearedCells.Contains(cell);
                Gizmos.color = cleared ? new Color(1f, 0.25f, 0.2f, 0.25f) : new Color(0.9f, 0.9f, 0.9f, 0.35f);
                Gizmos.DrawCube(CellToWorld(cell), Vector3.one * cubeSize);
            }
        }
    }
}

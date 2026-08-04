using Unity.Netcode;
using UnityEngine;

namespace SmokeSystem
{
    /// <summary>
    /// Attach to a camera (typically the player's) to whiteout/fog the view when its sightline
    /// passes through an active SmokeVolume — either standing directly inside it, or looking
    /// through it from outside. The latter matters a lot for a third-person shoulder camera: it
    /// sits behind/beside the character, so it's very often outside the cloud even while the
    /// player is looking straight through a wall of it, which an "is my own position inside
    /// smoke" check alone would completely miss.
    ///
    /// RenderSettings.fog is scene-global, not per-camera, so if this ever ends up on something
    /// that's spawned once per networked player (rather than the single local camera rig), every
    /// client would otherwise also apply remote players' in-smoke state to their own screen and
    /// fight over the same global fog settings. The NetworkObject/IsOwner check below makes that
    /// safe either way: a no-op on the current single, non-networked camera setup, and correct
    /// (local-viewer-only) if this is later moved onto a per-player prefab.
    /// </summary>
    public class SmokeVisionEffect : MonoBehaviour
    {
        [SerializeField] Color smokeFogColor = new Color(0.75f, 0.75f, 0.78f);
        [SerializeField] float maxFogDensity = 0.35f;
        [SerializeField] float blendSpeed = 4f;
        [Tooltip("How far ahead along the view direction to sample for smoke. Kept well short of weapon aim range — this only needs to cover roughly how far you could actually be looking through a cloud, and GetObscuration's cost scales with this distance.")]
        [SerializeField] float sightDistance = 20f;

        bool originalFogEnabled;
        Color originalFogColor;
        FogMode originalFogMode;
        float originalFogDensity;
        float currentInSmoke;
        NetworkObject networkObject;

        // Captured once so smoke can temporarily override the scene's fog and then cleanly
        // hand it back — both every frame once fully out of smoke, and in OnDestroy if this
        // component itself goes away mid-effect.
        void Start()
        {
            networkObject = GetComponentInParent<NetworkObject>();

            originalFogEnabled = RenderSettings.fog;
            originalFogColor = RenderSettings.fogColor;
            originalFogMode = RenderSettings.fogMode;
            originalFogDensity = RenderSettings.fogDensity;
        }

        void Update()
        {
            if (networkObject != null && !networkObject.IsOwner)
                return;

            float targetObscuration = ComputeObscuration();
            currentInSmoke = Mathf.MoveTowards(currentInSmoke, targetObscuration, blendSpeed * Time.deltaTime);

            if (currentInSmoke > 0.001f)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogColor = Color.Lerp(originalFogColor, smokeFogColor, currentInSmoke);
                RenderSettings.fogDensity = Mathf.Lerp(originalFogDensity, maxFogDensity, currentInSmoke);
            }
            else
            {
                RenderSettings.fog = originalFogEnabled;
                RenderSettings.fogMode = originalFogMode;
                RenderSettings.fogColor = originalFogColor;
                RenderSettings.fogDensity = originalFogDensity;
            }
        }

        /// <summary>
        /// How obstructed the view is, 0 (clear) to 1 (fully white-out). Standing directly
        /// inside smoke always reads as fully obstructed; otherwise this samples along the
        /// actual view ray — out to whatever it first hits, or sightDistance if nothing does —
        /// via SmokeVolume.GetTotalObscuration, rather than only checking this transform's own
        /// position.
        /// </summary>
        float ComputeObscuration()
        {
            foreach (var volume in SmokeVolume.ActiveVolumes)
            {
                if (volume.IsPositionInSmoke(transform.position))
                    return 1f;
            }

            float maxDist = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, sightDistance)
                ? hit.distance
                : sightDistance;

            return SmokeVolume.GetTotalObscuration(transform.position, transform.position + transform.forward * maxDist);
        }

        void OnDestroy()
        {
            RenderSettings.fog = originalFogEnabled;
            RenderSettings.fogMode = originalFogMode;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
        }
    }
}

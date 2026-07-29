using UnityEngine;

namespace SmokeSystem
{
    /// <summary>
    /// Thrown grenade: bounces around under normal physics for a fixed fuse time (like CS2's
    /// grenades, which pop on a timer rather than on impact), then goes inert and hands off to
    /// the SmokeVolume on the same object to grow the smoke cloud.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SmokeVolume))]
    public class SmokeGrenadeProjectile : MonoBehaviour
    {
        [Header("Fuse")]
        [SerializeField] float fuseTime = 2.2f;

        [Header("Physics")]
        [SerializeField] float bounciness = 0.45f;
        [SerializeField] float friction = 0.4f;

        Rigidbody rb;
        SmokeVolume smokeVolume;
        GameObject visual;
        float fuseTimer;
        bool thrown;
        bool detonated;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.2f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.material = new PhysicsMaterial("GrenadeBounce")
                {
                    bounciness = bounciness,
                    dynamicFriction = friction,
                    staticFriction = friction,
                    bounceCombine = PhysicsMaterialCombine.Average,
                    frictionCombine = PhysicsMaterialCombine.Average,
                };
            }

            smokeVolume = GetComponent<SmokeVolume>();
            smokeVolume.enabled = false;

            visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(transform, false);
            visual.transform.localScale = Vector3.one * 0.12f;
            var visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
                Destroy(visualCollider);

            var renderer = visual.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.16f, 0.19f, 0.13f);
            renderer.sharedMaterial = mat;
        }

        /// <summary>Launches the grenade with the given world-space velocity and starts the fuse.</summary>
        public void Throw(Vector3 velocity, Vector3? angularVelocity = null)
        {
            thrown = true;
            fuseTimer = fuseTime;
            rb.linearVelocity = velocity;
            rb.angularVelocity = angularVelocity ?? Random.insideUnitSphere * 6f;
        }

        void Update()
        {
            if (!thrown || detonated)
                return;

            fuseTimer -= Time.deltaTime;
            if (fuseTimer <= 0f)
                Detonate();
        }

        void Detonate()
        {
            detonated = true;

            if (visual != null)
                visual.SetActive(false);

            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            smokeVolume.enabled = true;
            smokeVolume.BeginGrowth(transform.position);
        }
    }
}

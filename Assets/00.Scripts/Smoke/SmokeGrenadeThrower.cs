using UnityEngine;
using UnityEngine.InputSystem;

namespace SmokeSystem
{
    /// <summary>Minimal test harness: right-click to throw a smoke grenade forward from this transform (or throwOrigin).</summary>
    public class SmokeGrenadeThrower : MonoBehaviour
    {
        [SerializeField] SmokeGrenadeProjectile grenadePrefab;
        [SerializeField] Transform throwOrigin;
        [SerializeField] float throwSpeed = 14f;
        [SerializeField] float upwardArc = 3f;

        void Update()
        {
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
                ThrowGrenade();
        }

        void ThrowGrenade()
        {
            if (grenadePrefab == null)
            {
                Debug.LogWarning("SmokeGrenadeThrower: no grenade prefab assigned.", this);
                return;
            }

            Transform origin = throwOrigin != null ? throwOrigin : transform;
            var instance = Instantiate(grenadePrefab, origin.position + origin.forward * 0.6f, Quaternion.identity);

            Vector3 velocity = origin.forward * throwSpeed + Vector3.up * upwardArc;
            instance.Throw(velocity);
        }
    }
}

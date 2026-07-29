using UnityEngine;
using UnityEngine.InputSystem;

namespace SmokeSystem
{
    /// <summary>Test-only stand-in for projectile/melee hit-detection: F fires a ray from this transform and disturbs whatever smoke it hits, so SmokeVolume.Disturb can be exercised without the full weapon stack.</summary>
    public class SmokeTestShooter : MonoBehaviour
    {
        [SerializeField] float disturbRadius = 0.8f;
        [SerializeField] float maxDistance = 100f;
        [SerializeField] float stepSize = 0.3f;

        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.fKey.wasPressedThisFrame)
                return;

            float distance = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxDistance)
                ? hit.distance
                : maxDistance;

            // Smoke has no collider, so a plain raycast sails through it and only ever stops on
            // solid geometry beyond it — disturbing just hit.point would usually land past the
            // cloud entirely. Walk the ray instead, like a bullet's Update() would, so every
            // smoke cell actually along the path gets a chance to be disturbed.
            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / stepSize));
            for (int i = 0; i <= steps; i++)
            {
                Vector3 point = transform.position + transform.forward * Mathf.Min(i * stepSize, distance);
                SmokeVolume.DisturbAt(point, disturbRadius);
            }
        }
    }
}

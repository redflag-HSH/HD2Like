using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SmokeSystem
{
    /// <summary>
    /// Right-click to throw a smoke grenade forward from this transform (or throwOrigin).
    /// When this component is part of a spawned network object (e.g. attached under a player
    /// prefab), the throw goes through ThrowServerRpc so the grenade is spawned and simulated
    /// server-authoritatively and replicated to every client - mirroring how Weapon/
    /// PlayingMovement spawn networked projectiles. Falls back to a plain local Instantiate when
    /// used standalone (IsSpawned == false), e.g. the SmokeTest scene with no NetworkManager running.
    /// </summary>
    public class SmokeGrenadeThrower : NetworkBehaviour
    {
        [SerializeField] SmokeGrenadeProjectile grenadePrefab;
        [SerializeField] Transform throwOrigin;
        [SerializeField] float throwSpeed = 14f;
        [SerializeField] float upwardArc = 3f;

        void Update()
        {
            // Once this is part of a spawned network object, only the owning client's own input
            // should be able to trigger a throw from it.
            if (IsSpawned && !IsOwner)
                return;

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
            Vector3 spawnPos = origin.position + origin.forward * 0.6f;
            Vector3 velocity = origin.forward * throwSpeed + Vector3.up * upwardArc;

            if (IsSpawned)
                ThrowServerRpc(spawnPos, velocity);
            else
                Instantiate(grenadePrefab, spawnPos, Quaternion.identity).Throw(velocity);
        }

        [ServerRpc]
        void ThrowServerRpc(Vector3 spawnPos, Vector3 velocity)
        {
            var instance = Instantiate(grenadePrefab, spawnPos, Quaternion.identity);
            instance.GetComponent<NetworkObject>().Spawn();
            instance.Throw(velocity);
        }
    }
}

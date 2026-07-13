using Unity.Netcode;
using UnityEngine;

// Per-player networked role, mirroring the NetworkVariable pattern used by
// playerCustom.cs. Read permission is Owner-only so nobody but the server
// and the owning client can ever learn a player's role - this is what makes
// cultists anonymous to each other, with no extra code needed.
public class PlayerRole : NetworkBehaviour
{
    public NetworkVariable<RoleType> role = new NetworkVariable<RoleType>(
        RoleType.Survivor,
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server);

    // Visible to everyone: death is already public knowledge via DieClientRpc
    // hiding the player's model for all clients.
    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            role.Value = RoleAssignmentCache.GetRole(OwnerClientId);

        if (IsOwner)
        {
            role.OnValueChanged += OnRoleChanged;
            // Handles the host's own player, where role.Value is already correct
            // by this point. Remote owner clients get the correct value shortly
            // after via OnRoleChanged once the server's write propagates.
            RoleRevealUI.Instance?.ShowRole(role.Value);
        }

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
            role.OnValueChanged -= OnRoleChanged;

        base.OnNetworkDespawn();
    }

    void OnRoleChanged(RoleType previous, RoleType current)
    {
        RoleRevealUI.Instance?.ShowRole(current);
    }

    public void SetAlive(bool alive)
    {
        if (!IsServer) return;
        isAlive.Value = alive;
    }

    // Called server-side (Lobby.OnHostStart) before a new match begins -
    // re-rolls the role and clears any death from the previous match.
    public void ResetForNewMatch()
    {
        if (!IsServer) return;
        role.Value = RoleAssignmentCache.GetRole(OwnerClientId);
        isAlive.Value = true;
    }
}

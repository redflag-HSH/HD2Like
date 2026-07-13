using System.Collections.Generic;
using UnityEngine;

// Server-only, plain (non-networked) role assignment table. Populated by
// Lobby.OnHostStart() before the network scene load to PlayScene, then read
// by each player's PlayerRole component when it spawns in PlayScene. Plain
// static state survives the scene load naturally since it's process memory
// on the host, not a scene object - no NetworkObject/DontDestroyOnLoad needed.
public static class RoleAssignmentCache
{
    static readonly Dictionary<ulong, RoleType> _roles = new Dictionary<ulong, RoleType>();

    public static void Assign(IReadOnlyList<ulong> clientIds)
    {
        _roles.Clear();

        List<ulong> shuffled = new List<ulong>(clientIds);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        int cultistCount = Mathf.Max(1, shuffled.Count / 4);
        for (int i = 0; i < shuffled.Count; i++)
            _roles[shuffled[i]] = i < cultistCount ? RoleType.Cultist : RoleType.Survivor;
    }

    public static RoleType GetRole(ulong clientId)
    {
        return _roles.TryGetValue(clientId, out RoleType role) ? role : RoleType.Survivor;
    }
}

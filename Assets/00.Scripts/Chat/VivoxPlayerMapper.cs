using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Maps NGO client IDs (ulong) ↔ Vivox display names (string).
///
/// Place this NetworkObject in any scene that uses voice chat.
/// Each player calls RegisterLocalPlayer() from PlayingMovement.OnNetworkSpawn
/// (IsOwner only). The server stores the mapping and broadcasts it to all clients.
///
/// Query from anywhere:
///   VivoxPlayerMapper.Instance.TryGetDisplayName(clientId, out string name)
///   VivoxPlayerMapper.Instance.TryGetClientId(displayName, out ulong id)
/// </summary>
public class VivoxPlayerMapper : NetworkBehaviour
{
    public static VivoxPlayerMapper Instance { get; private set; }

    // clientId → Vivox display name
    readonly Dictionary<ulong, string> _clientToDisplay = new Dictionary<ulong, string>();
    // Vivox display name → clientId
    readonly Dictionary<string, ulong> _displayToClient = new Dictionary<string, ulong>();

    /// <summary>Fired on all clients when a new mapping is confirmed.</summary>
    public event Action<ulong, string> OnMappingAdded;
    /// <summary>Fired on all clients when a player disconnects and their mapping is removed.</summary>
    public event Action<ulong, string> OnMappingRemoved;

    // ─────────────────────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

        _clientToDisplay.Clear();
        _displayToClient.Clear();

        base.OnNetworkDespawn();
    }

    // ─────────────────────────────────────────────────────────────
    //  Registration — call from PlayingMovement.OnNetworkSpawn (IsOwner)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Call this from the local player on spawn to register their NGO clientId
    /// with their Vivox display name. Reads the name from VivoxSceneHandler automatically.
    /// </summary>
    public void RegisterLocalPlayer(ulong clientId)
    {
        string displayName = ResolveLocalDisplayName(clientId);
        RegisterServerRpc(clientId, displayName);
    }

    string ResolveLocalDisplayName(ulong clientId)
    {
        VivoxSceneHandler handler = FindFirstObjectByType<VivoxSceneHandler>();
        if (handler != null && !string.IsNullOrEmpty(handler.playerName))
            return handler.playerName;

        // Fallback: use a generic name so the mapping is never empty
        return $"Player_{clientId}";
    }

    // ─────────────────────────────────────────────────────────────
    //  RPCs
    // ─────────────────────────────────────────────────────────────

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void RegisterServerRpc(ulong clientId, string displayName)
    {
        // Server stores the mapping first so it exists before the broadcast fires
        StoreMapping(clientId, displayName);
        BroadcastMappingClientRpc(clientId, displayName);
    }

    [ClientRpc]
    void BroadcastMappingClientRpc(ulong clientId, string displayName)
    {
        // Host already stored in RegisterServerRpc; StoreMapping is idempotent,
        // so calling it again on the host is safe — event fires only on first insert.
        StoreMapping(clientId, displayName);
    }

    [ClientRpc]
    void RemoveMappingClientRpc(ulong clientId, string displayName)
    {
        RemoveMapping(clientId, displayName);
    }

    // ─────────────────────────────────────────────────────────────
    //  Internal helpers
    // ─────────────────────────────────────────────────────────────

    void StoreMapping(ulong clientId, string displayName)
    {
        bool isNew = !_clientToDisplay.ContainsKey(clientId);
        _clientToDisplay[clientId] = displayName;
        _displayToClient[displayName] = clientId;
        if (isNew)
            OnMappingAdded?.Invoke(clientId, displayName);
    }

    void RemoveMapping(ulong clientId, string displayName)
    {
        _clientToDisplay.Remove(clientId);
        _displayToClient.Remove(displayName);
        OnMappingRemoved?.Invoke(clientId, displayName);
    }

    // Server-only: called when a client drops
    void OnClientDisconnected(ulong clientId)
    {
        if (!_clientToDisplay.TryGetValue(clientId, out string displayName)) return;
        RemoveMapping(clientId, displayName);
        RemoveMappingClientRpc(clientId, displayName);
    }

    // ─────────────────────────────────────────────────────────────
    //  Public query API
    // ─────────────────────────────────────────────────────────────

    /// <returns>True if the clientId has a registered Vivox display name.</returns>
    public bool TryGetDisplayName(ulong clientId, out string displayName)
        => _clientToDisplay.TryGetValue(clientId, out displayName);

    /// <returns>True if the Vivox display name has a registered NGO client ID.</returns>
    public bool TryGetClientId(string displayName, out ulong clientId)
        => _displayToClient.TryGetValue(displayName, out clientId);

    /// <returns>The Vivox display name for clientId, or null if not registered.</returns>
    public string GetDisplayName(ulong clientId)
        => _clientToDisplay.TryGetValue(clientId, out string name) ? name : null;

    /// <returns>The NGO client ID for the Vivox display name, or null if not registered.</returns>
    public ulong? GetClientId(string displayName)
        => _displayToClient.TryGetValue(displayName, out ulong id) ? id : (ulong?)null;

    /// <summary>Read-only view of all current clientId → displayName mappings.</summary>
    public IReadOnlyDictionary<ulong, string> AllMappings => _clientToDisplay;
}

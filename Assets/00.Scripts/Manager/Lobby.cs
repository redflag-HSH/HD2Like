using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Lobby : NetworkBehaviour
{
    // tracks how many clients (non-host) are ready
    NetworkVariable<int> readyCount = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // tracks total connected clients (non-host)
    NetworkVariable<int> clientCount = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] Interactor readyInteractor;  // place near each player's spawn
    [SerializeField] Interactor startInteractor;  // only visible/active for host

    public override void OnNetworkSpawn()
    {
        if (IsHost)
            OnHostSpawn();
        else
            OnClientSpawn();

        readyCount.OnValueChanged += OnReadyCountChanged;
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        readyCount.OnValueChanged -= OnReadyCountChanged;

        if (IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        base.OnNetworkDespawn();
    }

    void OnHostSpawn()
    {
        if (startInteractor != null) startInteractor.gameObject.SetActive(false);
        if (readyInteractor != null) readyInteractor.gameObject.SetActive(false);

        // Seed from whoever's already connected - matters when this Lobby
        // instance spawns because everyone returned from PlayScene, since
        // OnClientConnectedCallback only fires for *new* connections.
        clientCount.Value = Mathf.Max(0, NetworkManager.Singleton.ConnectedClientsList.Count - 1);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    void OnClientSpawn()
    {
        if (startInteractor != null) startInteractor.gameObject.SetActive(false);
        // readyInteractor stays active for clients
    }

    void OnClientConnected(ulong clientId)
    {
        if (!IsHost) return;
        if (clientId == NetworkManager.Singleton.LocalClientId) return;
        clientCount.Value++;
    }

    void OnClientDisconnected(ulong clientId)
    {
        if (!IsHost) return;
        if (clientId == NetworkManager.Singleton.LocalClientId) return;
        clientCount.Value = Mathf.Max(0, clientCount.Value - 1);
        CheckAllReady();
    }

    // Called by LobbyReadyInteractor when a client toggles ready
    public void OnPlayerToggleReady(PlayingMovement player, bool ready)
    {
        if (player.IsOwner)
            NotifyToggleReadyServerRpc(ready);
    }

    // Called by LobbyStartInteractor when the host interacts
    public void OnHostStart(PlayingMovement player)
    {
        if (!IsHost) return;

        List<ulong> clientIds = new List<ulong>();
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            clientIds.Add(client.ClientId);
        RoleAssignmentCache.Assign(clientIds);

        // Players who died or held a role from a previous match need to be
        // reset before the next one starts - their player object persists
        // across the Lobby/PlayScene transition rather than respawning.
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;

            client.PlayerObject.GetComponent<PlayerRole>()?.ResetForNewMatch();
            client.PlayerObject.GetComponent<PlayerStat>()?.ResetForNewMatch();
            client.PlayerObject.GetComponent<PlayingMovement>()?.ReviveClientRpc();
        }

        ClientManager cm = FindFirstObjectByType<ClientManager>();
        if (cm != null)
            cm.LoadSceneNetwork("PlayScene");
        else
            Debug.LogWarning("Lobby: ClientManager not found");
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void NotifyToggleReadyServerRpc(bool ready)
    {
        readyCount.Value = Mathf.Max(0, readyCount.Value + (ready ? 1 : -1));
        CheckAllReady();
    }

    void CheckAllReady()
    {
        if (!IsHost) return;
        bool allReady = clientCount.Value > 0 && readyCount.Value >= clientCount.Value;
        startInteractor?.gameObject.SetActive(allReady);
    }

    void OnReadyCountChanged(int _, int __)
    {
        // server already handles CheckAllReady; extend here for "X/Y ready" display if needed
    }
}

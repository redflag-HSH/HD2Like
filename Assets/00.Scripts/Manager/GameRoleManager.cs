using System.Collections;
using Unity.Netcode;
using UnityEngine;

// Lives in PlayScene as an in-scene NetworkObject, alongside RoutineManager/
// ItemManager/MultiManager - same singleton pattern as ItemManager (instance
// set unconditionally in Awake so every client has a local reference to route
// RPCs through, authoritative logic gated by IsServer checks).
public class GameRoleManager : NetworkBehaviour
{
    public static GameRoleManager instance;

    [Header("Match end")]
    [SerializeField] float returnToLobbyDelay = 5f;

    // Tracked for progress/UI purposes only - no longer influences win conditions.
    public NetworkVariable<int> tasksCompleted = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    bool _gameEnded;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            this.enabled = false;
    }

    // Called by Generator.OnInteract on the interacting client.
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ReportTaskServerRpc(ulong clientId)
    {
        if (_gameEnded) return;
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client)) return;
        if (client.PlayerObject == null) return;

        PlayerRole pr = client.PlayerObject.GetComponent<PlayerRole>();
        if (pr == null || pr.role.Value != RoleType.Survivor) return;

        tasksCompleted.Value++;
    }

    // Called directly (server-only call path) from PlayerStat.Death().
    public void OnPlayerDied()
    {
        if (!IsServer) return;
        CheckWinCondition();
    }

    // Called directly (guarded by caller) from RoutineManager.DayPass().
    public void CheckDayLimit(int days, int dayLimit)
    {
        if (!IsServer || _gameEnded) return;
        if (days >= dayLimit)
            EndGame(RoleType.Survivor);
    }

    void CheckWinCondition()
    {
        if (!IsServer || _gameEnded) return;

        PlayerRole[] all = FindObjectsByType<PlayerRole>(FindObjectsSortMode.None);
        if (all.Length == 0) return;

        int aliveSurvivors = 0;
        foreach (PlayerRole pr in all)
        {
            if (pr.isAlive.Value && pr.role.Value == RoleType.Survivor)
                aliveSurvivors++;
        }

        if (aliveSurvivors == 0)
            EndGame(RoleType.Cultist);
    }

    void EndGame(RoleType winningTeam)
    {
        if (_gameEnded) return;
        _gameEnded = true;
        GameEndClientRpc(winningTeam, returnToLobbyDelay);
        StartCoroutine(ReturnToLobbyAfterDelay());
    }

    IEnumerator ReturnToLobbyAfterDelay()
    {
        yield return new WaitForSeconds(returnToLobbyDelay);
        FindFirstObjectByType<ClientManager>()?.ReturnToLobby();
    }

    [ClientRpc]
    void GameEndClientRpc(RoleType winningTeam, float returnDelay)
    {
        GameEndUI.Instance?.ShowResult(winningTeam, returnDelay);
    }
}

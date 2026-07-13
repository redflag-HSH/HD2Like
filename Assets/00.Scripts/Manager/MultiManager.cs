using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiManager : NetworkBehaviour
{
    public static MultiManager instance;
    List<PlayingMovement> players = new List<PlayingMovement>();
    PlayingMovement clientPlayer;
    List<Color> PlayerColors;
    private void Start()
    {
        if (instance == null && IsHost)
            instance = this;
        else
        {
            Debug.Log("there is already multiplayManager Existing");
            Destroy(this);
        }

        //LoadPlayers();
        foreach (PlayingMovement p in players)
        {
            if (p.OwnerClientId == this.OwnerClientId)
                clientPlayer = p;
            break;
        }
        PlayerColors = new List<Color>();

        if (IsServer)
            SpreadPlayers();
    }

    void SpreadPlayers()
    {
        RoutineManager rm = RoutineManager.instance;
        if (rm == null || rm.path == null || rm.path.Count == 0) return;

        PlayingMovement[] allPlayers = FindObjectsByType<PlayingMovement>(FindObjectsSortMode.None);
        if (allPlayers.Length == 0) return;

        // Shuffle waypoint indices
        List<int> indices = new List<int>();
        for (int i = 0; i < rm.path.Count; i++) indices.Add(i);
        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        for (int i = 0; i < allPlayers.Length; i++)
        {
            Transform wp = rm.path.GetWaypoint(indices[i % indices.Count]);
            CharacterController cc = allPlayers[i].GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            allPlayers[i].transform.position = wp.position;
            if (cc != null) cc.enabled = true;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void playerMatSetClientRpc()
    {
        for (int i = 0; i < PlayerColors.Count; i++)
        {
            players[i].plaObj.GetComponent<Renderer>().material.color = PlayerColors[i];
        }
    }
    //클라이언트에서도 작동 가능하지만 오직 서버에서만 결과가 도출됨
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void playerMatGetServerRpc(Color c)
    {
        PlayerColors.Add(c);
        playerMatSetClientRpc();
    }
}

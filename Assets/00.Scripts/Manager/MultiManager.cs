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
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void NewOnesLoadPlayersRPC()
    {
        //신규일경우
        //기존 리스트받고 loadplayersrpc

    }
    [Rpc(SendTo.Everyone)]
    public void LoadPlayersRPC()
    {
        //기존일경우
        //추가만
    }
}

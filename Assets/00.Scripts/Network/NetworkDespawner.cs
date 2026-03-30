using Unity.Netcode;

/// <summary>
/// Attach to item prefabs alongside NetworkObject to allow server-side despawn.
/// Item prefabs also need a NetworkObject component and must be registered in DefaultNetworkPrefabs.
/// </summary>
public class NetworkDespawner : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = false)]
    public void DespawnServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
    }
}

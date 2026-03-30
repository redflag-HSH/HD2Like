using UnityEngine;
using Unity.Netcode;

public class networkTest : NetworkBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;

    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkObject.InstantiateAndSpawn(prefabToSpawn, NetworkManager.Singleton);
    }

    void Update()
    {

    }

    /// <summary>
    /// Spawns a network prefab at the given position and rotation.
    /// Must be called on the server (or host).
    /// </summary>
    public void SpawnNetworkPrefab(Vector3 position, Quaternion rotation)
    {
        if (!IsServer)
        {
            SpawnServerRpc(position, rotation);
            return;
        }

        NetworkObject.InstantiateAndSpawn(prefabToSpawn, NetworkManager.Singleton, position: position, rotation: rotation);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SpawnServerRpc(Vector3 position, Quaternion rotation)
    {
        NetworkObject.InstantiateAndSpawn(prefabToSpawn, NetworkManager.Singleton, position: position, rotation: rotation);
    }
}

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ItemManager : NetworkBehaviour
{
    public static ItemManager instance;
    public List<ItemInfo> possibleItems;

    readonly List<NetworkObject> spawnedItems = new List<NetworkObject>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            this.enabled = false;
    }

    // Called by clients; only takes effect on the server so loot stays authoritative.
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void GenerateServerRpc(Vector3 gePos, float Gradius, int minCount, int maxCount)
    {
        GenerateAt(gePos, Gradius, Random.Range(minCount, maxCount + 1));
    }

    // Clears all currently-spawned loot and re-rolls every Generator spawn point.
    // Meant to be called on phase change; only does anything on the server.
    public void RegenerateAll()
    {
        if (!IsServer) return;

        foreach (var no in spawnedItems)
        {
            if (no != null && no.IsSpawned)
                no.Despawn();
        }
        spawnedItems.Clear();

        foreach (var gen in FindObjectsByType<Generator>(FindObjectsSortMode.None))
            GenerateAt(gen.transform.position, gen.radius, Random.Range(gen.minCount, gen.maxCount + 1));
    }

    void GenerateAt(Vector3 gePos, float Gradius, int Gcount)
    {
        for (int i = 0; i < Gcount; i++)
        {
            ItemInfo item = PickWeightedItem();
            if (item == null || item.Prefab == null || !item.Prefab.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"ItemManager: skipping '{item?.ItemName}' - no valid Prefab assigned.");
                continue;
            }

            Vector2 offset = Random.insideUnitCircle * Gradius;
            Vector3 pos = gePos + new Vector3(offset.x, 0f, offset.y);
            SpawnItem(item, pos);
        }
    }

    void SpawnItem(ItemInfo item, Vector3 pos)
    {
        var handle = Addressables.InstantiateAsync(item.Prefab, pos, Quaternion.identity);
        handle.Completed += op =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"ItemManager: failed to instantiate '{item.ItemName}' ({item.Prefab.RuntimeKey}).");
                return;
            }
            if (op.Result.TryGetComponent<NetworkObject>(out var no))
            {
                no.Spawn();
                spawnedItems.Add(no);
            }
            else
            {
                Debug.LogWarning($"ItemManager: '{item.ItemName}' prefab has no NetworkObject, it won't sync to other clients.");
            }
        };
    }

    ItemInfo PickWeightedItem()
    {
        if (possibleItems == null || possibleItems.Count == 0)
        {
            Debug.LogWarning("ItemManager: possibleItems is empty, nothing to spawn.");
            return null;
        }

        float total = 0f;
        foreach (var it in possibleItems) total += it.Weight;
        if (total <= 0f) return null;

        float roll = Random.Range(0f, total);
        foreach (var it in possibleItems)
        {
            if (roll < it.Weight) return it;
            roll -= it.Weight;
        }
        return possibleItems[possibleItems.Count - 1];
    }
}

using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "ItemInfo", menuName = "Scriptable Objects/ItemInfo")]
public class ItemInfo : ScriptableObject
{
    public string ItemName;
    public string ItemDesc;
    public AssetReferenceGameObject Prefab;
    public float Weight = 1f;
}

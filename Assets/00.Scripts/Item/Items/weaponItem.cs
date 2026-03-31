using Unity.Netcode;
using UnityEngine;

public class weaponItem : ItemBase
{
    [SerializeField] GameObject WeaponOBJ;
    // Must match the index in WeaponController.weaponDisplayPrefabs on the Player prefab
    [SerializeField] public int weaponPrefabIndex = -1;
    public int LeftAmmo = -1;

    public override void OnInteract(PlayingMovement m)
    {
        GameObject go = Instantiate(WeaponOBJ);
        // The weapon prefab has a NetworkObject for network display spawning.
        // The owner's local copy must not have one — NGO blocks SetParent on unspawned NetworkObjects.
        if (go.TryGetComponent<NetworkObject>(out var no))
            DestroyImmediate(no);
        m.AddWeapon(go.GetComponent<Weapon>(), LeftAmmo, weaponPrefabIndex);
        base.OnInteract(m); // despawns item via NetworkDespawner or SetActive(false)
    }
}

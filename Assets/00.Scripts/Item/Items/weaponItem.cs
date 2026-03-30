using UnityEngine;

public class weaponItem : ItemBase
{
    [SerializeField] GameObject WeaponOBJ;
    // Must match the index in WeaponController.weaponDisplayPrefabs on the Player prefab
    [SerializeField] public int weaponPrefabIndex = -1;
    public int LeftAmmo = -1;

    public override void OnInteract(PlayingMovement m)
    {
        m.AddWeapon(Instantiate(WeaponOBJ).GetComponent<Weapon>(), LeftAmmo, weaponPrefabIndex);
        base.OnInteract(m); // despawns item via NetworkDespawner or SetActive(false)
    }
}

using UnityEngine;

public class weaponItem : ItemBase
{
    [SerializeField] GameObject WeaponOBJ;
    public int LeftAmmo = -1;
    public override void OnInteract(PlayingMovement m)
    {
        m.AddWeapon(Instantiate(WeaponOBJ).GetComponent<Weapon>(), LeftAmmo);
        base.OnInteract(m);
    }
}

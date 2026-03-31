using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int damage;
    [SerializeField] private float attackCool;
    public bool canAttack;
    [SerializeField] private int MaxAmmo;
    [SerializeField] private weaponItem itemObject;
    public int LeftAmmo;
    public enum weaponType
    {
        meele,
        shooter
    }
    [Space(5)]
    public weaponType type;
    public Transform attackPoint;
    public float MeeleRadius = .2f;
    [SerializeField] GameObject projectile;
    public int projectilePrefabIndex = -1;
    public GameObject weaponModel;

    public void AmmoReset(int ammo)
    {
        LeftAmmo = ammo;
        canAttack = true;
    }
    public void AmmoReset()
    {
        LeftAmmo = MaxAmmo;
        canAttack = true;
    }

    public void Shoot()
    {
        if (!canAttack)
            return;
        LeftAmmo--;
        PlayingMovement pm = GetComponentInParent<PlayingMovement>();
        if (pm != null && pm.IsSpawned)
            pm.SpawnProjectileServerRpc(attackPoint.position, attackPoint.rotation, damage, projectilePrefabIndex);
        else
            Instantiate(projectile, attackPoint.position, attackPoint.transform.rotation).GetComponent<Projectile>().SetDamage(damage);
        StartCoroutine(wait());
    }
    public void Discard()
    {
        Instantiate(itemObject.gameObject, weaponModel.transform.position, transform.rotation);
        itemObject.LeftAmmo = LeftAmmo;
    }
    IEnumerator wait()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCool);
        canAttack = true;
    }
}

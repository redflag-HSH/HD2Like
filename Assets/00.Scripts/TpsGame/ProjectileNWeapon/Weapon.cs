using System.Collections;
using SmokeSystem;
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
        shooter,
        throwable
    }
    [Space(5)]
    public weaponType type;
    public Transform attackPoint;
    public float MeeleRadius = .2f;
    [SerializeField] GameObject projectile;
    public int projectilePrefabIndex = -1;
    public GameObject weaponModel;

    [Header("Throwable (weaponType.throwable only)")]
    [SerializeField] float throwSpeed = 14f;
    [SerializeField] float throwUpwardArc = 3f;

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
    // Mirrors Shoot(): server-spawns the networked grenade prefab (indexed into
    // PlayingMovement.grenadePrefabs, parallel to projectilePrefabs) when networked, otherwise
    // throws a local, non-networked copy of `projectile` directly.
    public void Throw()
    {
        if (!canAttack)
            return;
        LeftAmmo--;
        Vector3 velocity = attackPoint.forward * throwSpeed + Vector3.up * throwUpwardArc;
        PlayingMovement pm = GetComponentInParent<PlayingMovement>();
        if (pm != null && pm.IsSpawned)
            pm.SpawnGrenadeServerRpc(attackPoint.position, velocity, projectilePrefabIndex);
        else
            Instantiate(projectile, attackPoint.position, Quaternion.identity).GetComponent<SmokeGrenadeProjectile>().Throw(velocity);
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

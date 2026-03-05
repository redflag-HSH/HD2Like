using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] LayerMask aimMask;
    //[SerializeField] Transform followingTarget;
    [SerializeField] Transform weaponTrans;
    public List<float> frostDrag;
    public Weapon currentWeapon;
    public Transform meeleTransform;
    Entity _player;
    PlayingMovement _playerMovement;

    bool meeleAttacking;
    List<Entity> attackedEntis;

    public float AttackCoolSet;

    private void Awake()
    {
        attackedEntis = new List<Entity>();
        _player = GetComponentInParent<PlayerStat>();
        _playerMovement = GetComponentInParent<PlayingMovement>();
    }

    private void Update()
    {
        MeeleChecking();
    }

    [Header("매개변수")]
    public float DraggingPow;

    private void AmmoCheck()
    {
        if (currentWeapon.LeftAmmo <= 0)
        {
            _playerMovement.RemoveWeapon(currentWeapon);
            ChangeWeapon();
        }
    }
    public void Fire()
    {
        if (currentWeapon == null)
            return;
        if (currentWeapon.type == Weapon.weaponType.shooter)
        {
            //어택 포인트에 총알 생성
            currentWeapon.Shoot();
            AmmoCheck();
        }
        else if (currentWeapon.type == Weapon.weaponType.meele)
        {
            StartCoroutine(attackwait());
        }
    }

    public void FollowTarget()
    {
        if (currentWeapon != null)
        {
            Vector2 screenPoint = new Vector2(Screen.width / 2, Screen.height / 2);
            Ray ray = Camera.main.ScreenPointToRay(screenPoint);
            //레이캐스트
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 999f, aimMask))
            {
                Vector3 relativePos = hitInfo.point - weaponTrans.position;
                Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
                //관성 파워-동상정도
                float dragge = Time.deltaTime * (DraggingPow - frostDrag[GetComponentInParent<PlayingMovement>().frostLevel]);
                weaponTrans.rotation = Quaternion.Lerp(weaponTrans.rotation, rotation, dragge);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, dragge);
            }
        }
        else
        {
            weaponTrans.rotation=Quaternion.Euler(0f, 0f, 0f);
        }
    }
    public void BasicFollow(Transform forw)
    {
        transform.forward = forw.forward;
    }
    private void MeeleChecking()
    {
        if (meeleAttacking)
        {
            Collider[] detects = Physics.OverlapSphere(currentWeapon.attackPoint.position, currentWeapon.MeeleRadius);
            foreach (Collider col in detects)
            {
                if (col.TryGetComponent<Entity>(out Entity t))
                {
                    bool check = true;

                    foreach (Entity tt in attackedEntis)
                        if (tt == t)
                            check = false;

                    if (t == _player)
                        check = false;

                    if (check)
                    {
                        print("meelehitted");
                        t.Damage(currentWeapon.damage, Entity.damageType.meele);
                        attackedEntis.Add(t);
                    }
                }
            }
        }
    }
    IEnumerator attackwait()
    {
        GetComponentInParent<Animator>().SetTrigger("Attack");
        meeleAttacking = true;
        attackedEntis.Clear();
        yield return new WaitForSeconds(.5f);
        meeleAttacking = false;
    }
    public void ChangeWeapon(Weapon weapon)
    {
        if (currentWeapon == weapon)
            ChangeWeapon();
        else
        {
            if (currentWeapon != null)
            {
                currentWeapon.gameObject.SetActive(false);
            }
            currentWeapon = weapon;
            currentWeapon.gameObject.SetActive(true);
            weaponTrans = currentWeapon.weaponModel.transform;
        }
    }
    public void ChangeWeapon()
    {
        currentWeapon.gameObject.SetActive(false);
        currentWeapon = null;
        weaponTrans = null;
    }
}

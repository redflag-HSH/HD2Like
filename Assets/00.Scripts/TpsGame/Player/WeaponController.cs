using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.Tilemaps.Tilemap;

public class WeaponController : MonoBehaviour
{
    [SerializeField] LayerMask aimMask;
    [SerializeField] Transform followingTarget;
    [SerializeField] Transform weaponTrans;
    public List<float> frostDrag;
    public weaponState currentState;
    bool meeleAttacking;
    public enum weaponState
    {
        nothing,
        meele,
        shooter
    }
    private void Update()
    {
        MeeleChecking();
    }

    public Transform attackPoint;
    public GameObject projectile;
    //public GameObject projectile2;

    [Header("매개변수")]
    public float DraggingPow;
    public void Fire()
    {
        if (currentState == weaponState.shooter)
        {
            //어택 포인트에 총알 생성
            Instantiate(projectile, attackPoint.position, attackPoint.transform.rotation);
        }
        else if (currentState == weaponState.meele)
        {
            StartCoroutine(attackwait());
        }
    }
    public void FollowTarget()
    {
        Vector2 screenPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        //레이캐스트
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 999f, aimMask))
        {
            Vector3 relativePos = hitInfo.point - weaponTrans.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            //관성 파워-동상정도
            float dragge = Time.deltaTime * (DraggingPow - frostDrag[GetComponentInParent<Movement>().frostLevel]);
            weaponTrans.rotation = Quaternion.Lerp(weaponTrans.rotation, rotation, dragge);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, dragge);
        }
        else
        {

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
            Collider[] detects = Physics.OverlapSphere(attackPoint.position, 0.2f);
            foreach(Collider col in detects)
            {
                if(col.TryGetComponent<Entity>(out Entity t))
                {
                    print("meelehitted");
                }
            }
        }
    }
    IEnumerator attackwait()
    {
        GetComponentInParent<Animator>().SetTrigger("attack");
        meeleAttacking = true;
        yield return new WaitForSeconds(.5f);
        meeleAttacking = false;
    }
}

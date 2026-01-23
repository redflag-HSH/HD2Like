using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public weaponState currentState;
    public enum weaponState
    {
        nothing,
        meele,
        shooter
    }

public Transform attackPoint;
    public GameObject projectile;
    public void Fire()
    {
        if (currentState == weaponState.shooter)
        {
            //어택 포인트에 총알 생성
            Instantiate(projectile, attackPoint.position, transform.rotation);
        }
        else if (currentState == weaponState.meele)
        {
            //어택포인트에 sphere cast 생성 or 애니메이션 재생해서 접촉
        }
    }
}

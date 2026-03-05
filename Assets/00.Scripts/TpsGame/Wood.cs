using UnityEngine;

public class Wood : Entity
{
    public GameObject wood;
    public override void Damage(int i, damageType type)
    {
        if (type == damageType.meele)
            base.Damage(i, type);
    }
    protected override void Death()
    {
        base.Death();
        //애니메이션 재생

        //아이템 생성
        Instantiate(wood, transform.position, transform.rotation);
        //ItemManager.instance.ItemGenerate(transform.position, 0, 1);
        Destroy(gameObject);
    }
}

using UnityEngine;

public class Wood : HealthEntity
{
    public GameObject wood;
    public override void Damage(int i, IDamageable.DamageType type)
    {
        if (type == IDamageable.DamageType.meele)
            base.Damage(i, type);
    }
    protected override void Death()
    {
        base.Death();
        //�ִϸ��̼� ���

        //������ ����
        Instantiate(wood, transform.position, transform.rotation);
        //ItemManager.instance.ItemGenerate(transform.position, 0, 1);
        Destroy(gameObject);
    }
}

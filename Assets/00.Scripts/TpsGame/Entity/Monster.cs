using UnityEngine;

public class Monster : HealthEntity
{
    StateMachine _machine;
    protected override void Awake()
    {
        base.Awake();
        _machine = GetComponent<StateMachine>();
    }
    public override void Damage(int i, IDamageable.DamageType type)
    {
        base.Damage(i, type);
    }
    protected override void Death()
    {
        base.Death();
    }

    
}

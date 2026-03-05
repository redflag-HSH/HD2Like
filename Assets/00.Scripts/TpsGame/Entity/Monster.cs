using UnityEngine;

public class Monster : Entity
{
    StateMachine _machine;
    protected override void Awake()
    {
        base.Awake();
        _machine = GetComponent<StateMachine>();
    }
    public override void Damage(int i, damageType type)
    {
        base.Damage(i, type);
    }
    protected override void Death()
    {
        base.Death();
    }

    
}

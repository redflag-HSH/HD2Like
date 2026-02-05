using UnityEngine;

public class Wood : Entity
{
    public override void Damage(int i)
    {
        base.Damage(i);
    }
    protected override void Death()
    {
        base.Death();
        //애니메이션 재생
        //아이템 생성
    }
}

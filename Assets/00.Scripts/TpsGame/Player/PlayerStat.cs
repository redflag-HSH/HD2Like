using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : Entity
{
    //동상 정도
    //동상에 따라 플레이어의 관성 및 이동속도 디버프 증가
    //동상 진행 수준이 일정 수준 이상일 경우 점차 체력 감소
    public float frostbite;
    public bool frosting;
    public float Warmth;
    public Material overlapMat;
    public List<float> levelGates;
    //체력은 entity걸로

    //스테미너
    float stamina;
    //동상 단계에 따른 관성 및 이동속도 디버프 정보
    float frostDrag;
    float moveFrost;
    private void Update()
    {
        FrostBiteAdvance();
    }
    public void FrostBiteAdvance()
    {
        if (frosting)
            frostbite += Time.deltaTime;
        else
            Warmth -= Time.deltaTime;
        if (Warmth <= 0)
            frosting = true;
        overlapMat.SetFloat("_vignettePow", 12 - frostbite / 10);
        for (int i = 0; i < levelGates.Count; i++)
            if (frostbite < levelGates[i])
            {
                GetComponent<Movement>().frostLevel = i - 1;
                break;
            }
    }


    public override void Damage(int i)
    {
        base.Damage(i);
    }
    protected override void Death()
    {
        base.Death();
        //관전모드로 돌리기
    }
}

using System.Collections;
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
    [SerializeField] private int frostDamageCircle;
    private bool frostbiteDamageCan;
    private int freezeDamage = 1;

    public Material frostbiteOverlapMat;
    public Material bloodOverlapMat;
    public List<float> levelGates;
    //체력은 entity걸로
    //동상 단계에 따른 관성 및 이동속도 디버프 정보
    float frostDrag;
    float moveFrost;

    bool isAlive;
    private void Start()
    {
        isAlive = true;
        frostbiteDamageCan = true;
    }
    private void Update()
    {
        if (isAlive)
            FrostBiteAdvance();
    }
    public void FrostBiteAdvance()
    {
        if (frostbite >= 150)
        {
            FreezeDamage();
            return;
        }
        if (frosting)
            frostbite += Time.deltaTime;
        else
            Warmth -= Time.deltaTime;
        if (Warmth <= 0)
            frosting = true;
        frostbiteOverlapMat.SetFloat("_vignettePow", 15 - frostbite / 10);
        for (int i = 0; i < levelGates.Count; i++)
            if (frostbite < levelGates[i])
            {
                GetComponent<PlayingMovement>().frostLevel = i - 1;
                break;
            }
    }
    public void FreezeDamage()
    {
        if (frostbiteDamageCan)
        {
            this.Damage(freezeDamage, damageType.freeze);
            StartCoroutine(FrostDamageDelay());
        }
    }

    public override void Damage(int i, damageType type)
    {
        base.Damage(i, type);
        Debug.Log("player damage");
        bloodOverlapMat.SetFloat("_vignettePow", 15 * (health / maxHealth));
    }
    public void Heal(int pow)
    {
        health += pow;
        if (health > maxHealth)
            health = maxHealth;
        bloodOverlapMat.SetFloat("_vignettePow", 15 * (health / maxHealth));
    }
    protected override void Death()
    {
        base.Death();
        //관전모드로 돌리기
        GetComponent<PlayingMovement>().enabled = false;
        CameraController.instance.SwitchCameraStyle(CameraController.CameraStyle.TopDown);
    }
    IEnumerator FrostDamageDelay()
    {
        frostbiteDamageCan = false;
        yield return new WaitForSeconds(frostDamageCircle);
        frostbiteDamageCan = true;
    }
    private void OnDisable()
    {
        bloodOverlapMat.SetFloat("_vignettePow", 15 );
        frostbiteOverlapMat.SetFloat("_vignettePow", 15);
    }
}

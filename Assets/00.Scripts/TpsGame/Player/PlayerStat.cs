using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : HealthEntity
{
    //���� ����
    //���� ���� �÷��̾��� ���� �� �̵��ӵ� ����� ����
    //���� ���� ������ ���� ���� �̻��� ��� ���� ü�� ����
    public float frostbite;
    public bool frosting;
    public float Warmth;
    [SerializeField] private int frostDamageCircle;
    private bool frostbiteDamageCan;
    private int freezeDamage = 1;

    public Material frostbiteOverlapMat;
    public Material bloodOverlapMat;
    public List<float> levelGates;
    //ü���� entity�ɷ�
    //���� �ܰ迡 ���� ���� �� �̵��ӵ� ����� ����
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
            this.Damage(freezeDamage, IDamageable.DamageType.freeze);
            StartCoroutine(FrostDamageDelay());
        }
    }

    public override void Damage(int i, IDamageable.DamageType type)
    {
        base.Damage(i, type);
        if (health <= 0) return; // death RPC already handles the overlay reset
        Debug.Log("player damage");
        PlayingMovement pm = GetComponent<PlayingMovement>();
        float vignetteValue = 15f * health / maxHealth;
        if (pm != null && pm.IsSpawned)
            pm.ShowBloodOverlayClientRpc(vignetteValue);
        else
            bloodOverlapMat.SetFloat("_vignettePow", vignetteValue);
    }
    public void ApplyBloodOverlay(float value)
    {
        bloodOverlapMat.SetFloat("_vignettePow", value);
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
        PlayingMovement pm = GetComponent<PlayingMovement>();
        if (pm.IsSpawned)
            pm.DieClientRpc();
        else
            pm.OnDeath(); // fallback for offline/test mode
    }
    IEnumerator FrostDamageDelay()
    {
        frostbiteDamageCan = false;
        yield return new WaitForSeconds(frostDamageCircle);
        frostbiteDamageCan = true;
    }
    private void OnDisable()
    {
        bloodOverlapMat.SetFloat("_vignettePow", 15);
        frostbiteOverlapMat.SetFloat("_vignettePow", 15);
    }
}

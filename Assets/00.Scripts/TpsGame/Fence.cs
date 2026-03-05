using NUnit.Framework;
using UnityEngine;

public class Fence : Entity
{
    public GameObject[] fenceWires;

    protected override void Awake()
    {
        base.Awake();
    }
    public override void Damage(int i, damageType type)
    {
        if (type == damageType.meele)
        {
            Debug.Log("fence Damaged");
            base.Damage(i, type);
        }
    }

    protected override void Death()
    {
        foreach (GameObject fence in fenceWires)
        {
            fence.SetActive(false);
        }
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<FenceInteractor>().FenceCutted = true;
    }

    public void Revive()
    {
        foreach (GameObject fence in fenceWires)
        {
            fence.SetActive(true);
        }
    }
}

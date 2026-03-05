using UnityEngine;

public class WoodLogs : ItemBase
{
    public override void OnInteract(PlayingMovement m)
    {
        //MatManager.instance.GetMat(2, Random.Range(1, 4));
        base.OnInteract(m);
    }
}

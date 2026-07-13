using UnityEngine;

public class Generator : Interactor
{
    public float radius = 2.2f;
    public int minCount = 2;
    public int maxCount = 4;

    public override void OnInteract(PlayingMovement m)
    {
        base.OnInteract(m);
        ItemManager.instance.GenerateServerRpc(this.transform.position, radius, minCount, maxCount);
        GameRoleManager.instance?.ReportTaskServerRpc(m.OwnerClientId);
    }
}

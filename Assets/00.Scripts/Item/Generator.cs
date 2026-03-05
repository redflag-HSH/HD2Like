using UnityEngine;

public class Generator : Interactor
{
    public override void OnInteract(PlayingMovement m)
    {
        base.OnInteract(m);
        ItemManager.instance.ItemGenerate(this.transform.position, 2.2f, 4);
    }
}

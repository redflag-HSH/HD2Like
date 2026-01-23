using UnityEngine;

public class Generator : Interactor
{
    public override void OnInteract()
    {
        base.OnInteract();
        ItemManager.instance.ItemGenerate(this.transform.position, 2.2f, 4);
    }
}

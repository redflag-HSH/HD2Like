using UnityEngine;

public class FenceInteractor : Interactor
{
    public bool FenceCutted;
    public override void OnInteract(PlayingMovement m)
    {
        if (FenceCutted)
            if (MatManager.instance.UseMAt(MatManager.matType.scrap, 3))
            {
                GetComponent<Fence>().Revive();
            }
    }
}

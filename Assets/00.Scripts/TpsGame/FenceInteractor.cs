using UnityEngine;

public class FenceInteractor : Interactor
{
    public bool FenceCutted;
    Fence _fence;
    private void Start()
    {
        _fence = GetComponent<Fence>();
    }
    public override void OnInteract(PlayingMovement m)
    {
        if (FenceCutted)
            if (MatManager.instance.UseMAt(MatManager.matType.scrap, 3))
            {
                GetComponent<Fence>().Revive();
            }
    }
    public override string GetText()
    {
        return ("Ææ½º °íÄ¡±â [" + _fence.GetHealth() + "/" + _fence.maxHealth + "]");
    }
}

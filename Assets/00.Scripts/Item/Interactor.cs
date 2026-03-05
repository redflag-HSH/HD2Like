using UnityEngine;

public class Interactor : MonoBehaviour
{
    public enum InteractorType
    {
        item,
        key,
        special
    }
    public InteractorType type;

    public virtual void OnInteract(PlayingMovement m) { }
}

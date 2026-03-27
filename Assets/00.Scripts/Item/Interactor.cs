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
    [SerializeField] private string _text;

    public virtual void OnInteract(PlayingMovement m) { }
    public virtual string GetText() 
    {
        return _text;
    }
}

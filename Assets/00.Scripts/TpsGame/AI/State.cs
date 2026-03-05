using UnityEngine;

public abstract class State : MonoBehaviour
{
    public StateMachine machine;
    public abstract void Enter();
    public abstract void Perform();
    public abstract void Exit();
}
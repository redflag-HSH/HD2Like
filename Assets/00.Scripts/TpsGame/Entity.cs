using UnityEngine;

public class Entity : MonoBehaviour
{
    public int health;
    public virtual void Damage(int i)
    {
        health -= i;
        if(health <= 0)
            Death();
    }
    protected virtual void Death()
    {

    }
}

using UnityEngine;

public class Entity : MonoBehaviour
{
    public int maxHealth;
    protected int health;
    public enum damageType
    {
        meele,
        bullet,
        explosion,
        freeze
    }
    protected virtual void Awake()
    {
        health = maxHealth;
    }
    public virtual void Damage(int i,damageType type)
    {
        health -= i;
        if(health <= 0)
            Death();
    }
    protected virtual void Death()
    {

    }
}

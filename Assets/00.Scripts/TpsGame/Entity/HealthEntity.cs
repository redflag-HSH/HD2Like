using UnityEngine;

public abstract class HealthEntity : MonoBehaviour, IDamageable
{
    public int maxHealth;
    protected int health;

    public int GetHealth() { return health; }
    protected virtual void Awake()
    {
        health = maxHealth;
    }
    public virtual void Damage(int amount, IDamageable.DamageType type)
    {
        health -= amount;
        if (health <= 0)
            Death();
    }
    protected virtual void Death() { }
}

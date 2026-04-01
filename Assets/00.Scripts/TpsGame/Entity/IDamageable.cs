public interface IDamageable
{
    public enum DamageType
    {
        meele,
        bullet,
        explosion,
        freeze
    }

    void Damage(int amount, DamageType type);
}

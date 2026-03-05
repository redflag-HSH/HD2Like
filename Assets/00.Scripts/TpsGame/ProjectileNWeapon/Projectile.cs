using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] bool _special;
    [SerializeField] bool _gravityPerform;
    [SerializeField] float _gravityDivider;
    [SerializeField] float _moveForce;
    private int _damage;
    float _GravityMultiplyByTime;
    float _lifeTime = 0;
    public void Perform()
    {
        transform.position += transform.forward * _moveForce * Time.deltaTime;
        if (_gravityPerform)
        {
            _GravityMultiplyByTime += Time.deltaTime / 2;
            transform.position += new Vector3(0, _GravityMultiplyByTime * (-9.8f * Time.deltaTime / _gravityDivider), 0);
            _lifeTime += Time.deltaTime;
            if (_lifeTime > 30)
                gameObject.SetActive(false);
        }
    }
    public void SetDamage(int damage)
    {
        _damage = damage;
    }
    protected virtual void Update()
    {
        Perform();
        if (!_special)
            CheckCollide();
    }
    public void CheckCollide()
    {
        Collider[] detects = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (Collider col in detects)
        {
            if (col.TryGetComponent<Entity>(out Entity t))
            {
                t.Damage(_damage, Entity.damageType.bullet);
                gameObject.SetActive(false);
            }
        }
    }
}

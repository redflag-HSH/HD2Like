using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SmokeGrenadeProjectile : MonoBehaviour
{
    [Header("Fuse")]
    // CS2 방식: 바닥에 닿았는지와 상관없이 퓨즈가 다하면 그 자리(공중이어도)에서 터진다.
    [SerializeField] private float fuseTime = 2f;

    [Header("Bounce")]
    [SerializeField] private float bounceDamping = 0.45f;
    [SerializeField] private float spinAmount = 4f;

    [Header("Smoke")]
    [SerializeField] private GameObject smokeCloudPrefab;

    private Rigidbody _rb;
    private float _fuseRemaining;
    private bool _detonated;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _fuseRemaining = fuseTime;
    }

    public void Throw(Vector3 velocity)
    {
        _rb.linearVelocity = velocity;
        _rb.angularVelocity = Random.insideUnitSphere * spinAmount;
    }

    void Update()
    {
        if (_detonated) return;

        _fuseRemaining -= Time.deltaTime;
        if (_fuseRemaining <= 0f)
            Detonate();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_detonated) return;

        Vector3 reflected = Vector3.Reflect(_rb.linearVelocity, collision.GetContact(0).normal);
        _rb.linearVelocity = reflected * bounceDamping;
    }

    void Detonate()
    {
        _detonated = true;

        if (smokeCloudPrefab != null)
            Instantiate(smokeCloudPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}

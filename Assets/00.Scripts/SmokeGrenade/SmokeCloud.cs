using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 연막 구름 하나의 생애주기(팽창 -> 유지 -> 소산)와, 다른 시스템이 시야 차단 여부를
// 물어볼 수 있는 정적 API를 함께 가진다. 파티클 비주얼 설정은 전부 코드에서 처리하므로
// 프리팹에는 ParticleSystem 컴포넌트만 비어있는 상태로 붙어있으면 된다.
[RequireComponent(typeof(ParticleSystem))]
public class SmokeCloud : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float expandDuration = 1.5f;
    [SerializeField] private float holdDuration = 15f;
    [SerializeField] private float fadeDuration = 3f;

    [Header("Size")]
    [SerializeField] private float maxRadius = 4.5f;

    [Header("Look")]
    [SerializeField] private Color smokeColor = new Color(0.75f, 0.75f, 0.75f, 0.16f);
    [SerializeField] private float particleSize = 3f;

    private static readonly List<SmokeCloud> _active = new List<SmokeCloud>();

    private ParticleSystem _ps;
    private float _particleLifetime;

    public float CurrentRadius { get; private set; }

    void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
        ConfigureParticleSystem();
    }

    void OnEnable() => _active.Add(this);
    void OnDisable() => _active.Remove(this);

    void Start() => StartCoroutine(Lifecycle());

    void ConfigureParticleSystem()
    {
        _particleLifetime = Mathf.Max(fadeDuration, 2f);

        var main = _ps.main;
        main.loop = true;
        main.startLifetime = _particleLifetime;
        main.startSpeed = 0.2f;
        main.startSize = particleSize;
        main.startColor = smokeColor;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 500;

        var emission = _ps.emission;
        emission.rateOverTime = 0f;

        var shape = _ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;
        shape.radiusThickness = 1f;

        var noise = _ps.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.2f;

        var colorOverLifetime = _ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(smokeColor, 0f), new GradientColorKey(smokeColor, 1f) },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(smokeColor.a, 0.2f),
                new GradientAlphaKey(smokeColor.a, 0.7f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;
    }

    IEnumerator Lifecycle()
    {
        var shape = _ps.shape;
        var emission = _ps.emission;

        // 터지는 순간 확 퍼지는 느낌을 위한 초기 버스트
        _ps.Emit(60);
        emission.rateOverTime = 40f;

        float t = 0f;
        while (t < expandDuration)
        {
            t += Time.deltaTime;
            float p = EaseOutCubic(Mathf.Clamp01(t / expandDuration));
            CurrentRadius = Mathf.Lerp(0.05f, maxRadius, p);
            shape.radius = CurrentRadius;
            yield return null;
        }
        CurrentRadius = maxRadius;
        shape.radius = maxRadius;

        emission.rateOverTime = 18f;
        yield return new WaitForSeconds(holdDuration);

        emission.rateOverTime = 0f;
        float startRadius = CurrentRadius;
        float fadeT = 0f;
        while (fadeT < fadeDuration)
        {
            fadeT += Time.deltaTime;
            CurrentRadius = Mathf.Lerp(startRadius, 0f, fadeT / fadeDuration);
            yield return null;
        }
        CurrentRadius = 0f;

        // 이미 뿜어진 파티클들이 각자 수명대로 자연스럽게 사라질 시간을 준 뒤 정리
        yield return new WaitForSeconds(_particleLifetime);
        Destroy(gameObject);
    }

    static float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);

    public bool ContainsPoint(Vector3 point) =>
        (point - transform.position).sqrMagnitude <= CurrentRadius * CurrentRadius;

    public static bool TryGetContainingCloud(Vector3 point, out SmokeCloud cloud)
    {
        for (int i = 0; i < _active.Count; i++)
        {
            if (_active[i].ContainsPoint(point))
            {
                cloud = _active[i];
                return true;
            }
        }
        cloud = null;
        return false;
    }

    // 다른 시스템(AI 시야, 조준 등)이 두 지점 사이 시야가 연막에 막혔는지 물어볼 때 사용.
    public static bool IsLineBlocked(Vector3 a, Vector3 b)
    {
        for (int i = 0; i < _active.Count; i++)
        {
            SmokeCloud cloud = _active[i];
            if (cloud.CurrentRadius <= 0.05f) continue;
            if (SegmentIntersectsSphere(a, b, cloud.transform.position, cloud.CurrentRadius))
                return true;
        }
        return false;
    }

    static bool SegmentIntersectsSphere(Vector3 a, Vector3 b, Vector3 center, float radius)
    {
        Vector3 ab = b - a;
        float lengthSq = ab.sqrMagnitude;
        float t = lengthSq < 0.0001f ? 0f : Mathf.Clamp01(Vector3.Dot(center - a, ab) / lengthSq);
        Vector3 closest = a + ab * t;
        return (closest - center).sqrMagnitude <= radius * radius;
    }
}

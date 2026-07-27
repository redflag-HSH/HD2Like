using UnityEngine;

// 카메라(플레이어 시점)가 연막 구름 내부에 들어가면 화면을 뿌옇게 만든다.
// 기존 IgnoreFog.cs와 같은 방식으로 RenderSettings.fog를 사용해 URP에서 바로 동작한다.
public class SmokeVisionEffect : MonoBehaviour
{
    [SerializeField] private Color smokeFogColor = new Color(0.55f, 0.55f, 0.55f);
    [SerializeField] private float maxFogDensity = 0.35f;
    [SerializeField] private float blendSpeed = 3f;

    private bool _capturedDefaults;
    private bool _defaultFogEnabled;
    private Color _defaultFogColor;
    private float _defaultFogDensity;
    private FogMode _defaultFogMode;

    private float _blend;

    void Update()
    {
        if (!_capturedDefaults)
        {
            _defaultFogEnabled = RenderSettings.fog;
            _defaultFogColor = RenderSettings.fogColor;
            _defaultFogDensity = RenderSettings.fogDensity;
            _defaultFogMode = RenderSettings.fogMode;
            _capturedDefaults = true;
        }

        bool inSmoke = SmokeCloud.TryGetContainingCloud(transform.position, out _);
        _blend = Mathf.MoveTowards(_blend, inSmoke ? 1f : 0f, blendSpeed * Time.deltaTime);

        if (_blend <= 0.0001f)
        {
            RenderSettings.fog = _defaultFogEnabled;
            RenderSettings.fogColor = _defaultFogColor;
            RenderSettings.fogDensity = _defaultFogDensity;
            RenderSettings.fogMode = _defaultFogMode;
            return;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = Color.Lerp(_defaultFogColor, smokeFogColor, _blend);
        RenderSettings.fogDensity = Mathf.Lerp(_defaultFogDensity, maxFogDensity, _blend);
    }
}

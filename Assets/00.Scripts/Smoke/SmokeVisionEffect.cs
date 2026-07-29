using UnityEngine;

namespace SmokeSystem
{
    /// <summary>
    /// Attach to a camera (typically the player's) to whiteout/fog the view while standing
    /// inside an active SmokeVolume, using the scene's built-in fog so it needs no custom
    /// post-process pipeline.
    /// </summary>
    public class SmokeVisionEffect : MonoBehaviour
    {
        [SerializeField] Color smokeFogColor = new Color(0.75f, 0.75f, 0.78f);
        [SerializeField] float maxFogDensity = 0.35f;
        [SerializeField] float blendSpeed = 4f;

        bool originalFogEnabled;
        Color originalFogColor;
        FogMode originalFogMode;
        float originalFogDensity;
        float currentInSmoke;

        void Start()
        {
            originalFogEnabled = RenderSettings.fog;
            originalFogColor = RenderSettings.fogColor;
            originalFogMode = RenderSettings.fogMode;
            originalFogDensity = RenderSettings.fogDensity;
        }

        void Update()
        {
            bool insideSmoke = false;
            foreach (var volume in SmokeVolume.ActiveVolumes)
            {
                if (volume.IsPositionInSmoke(transform.position))
                {
                    insideSmoke = true;
                    break;
                }
            }

            currentInSmoke = Mathf.MoveTowards(currentInSmoke, insideSmoke ? 1f : 0f, blendSpeed * Time.deltaTime);

            if (currentInSmoke > 0.001f)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogColor = Color.Lerp(originalFogColor, smokeFogColor, currentInSmoke);
                RenderSettings.fogDensity = Mathf.Lerp(originalFogDensity, maxFogDensity, currentInSmoke);
            }
            else
            {
                RenderSettings.fog = originalFogEnabled;
                RenderSettings.fogMode = originalFogMode;
                RenderSettings.fogColor = originalFogColor;
                RenderSettings.fogDensity = originalFogDensity;
            }
        }

        void OnDestroy()
        {
            RenderSettings.fog = originalFogEnabled;
            RenderSettings.fogMode = originalFogMode;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
        }
    }
}

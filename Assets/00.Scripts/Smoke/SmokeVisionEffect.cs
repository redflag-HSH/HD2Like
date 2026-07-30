using Unity.Netcode;
using UnityEngine;

namespace SmokeSystem
{
    /// <summary>
    /// Attach to a camera (typically the player's) to whiteout/fog the view while standing
    /// inside an active SmokeVolume, using the scene's built-in fog so it needs no custom
    /// post-process pipeline.
    ///
    /// RenderSettings.fog is scene-global, not per-camera, so if this ever ends up on something
    /// that's spawned once per networked player (rather than the single local camera rig), every
    /// client would otherwise also apply remote players' in-smoke state to their own screen and
    /// fight over the same global fog settings. The NetworkObject/IsOwner check below makes that
    /// safe either way: a no-op on the current single, non-networked camera setup, and correct
    /// (local-viewer-only) if this is later moved onto a per-player prefab.
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
        NetworkObject networkObject;

        // Captured once so smoke can temporarily override the scene's fog and then cleanly
        // hand it back — both every frame once fully out of smoke, and in OnDestroy if this
        // component itself goes away mid-effect.
        void Start()
        {
            networkObject = GetComponentInParent<NetworkObject>();

            originalFogEnabled = RenderSettings.fog;
            originalFogColor = RenderSettings.fogColor;
            originalFogMode = RenderSettings.fogMode;
            originalFogDensity = RenderSettings.fogDensity;
        }

        void Update()
        {
            if (networkObject != null && !networkObject.IsOwner)
                return;

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

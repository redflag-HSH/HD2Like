using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ZeldaShader : MonoBehaviour
{
    Volume vlo;
    Tonemapping tone;
    Bloom bloom;
    Vignette vig;
    ChromaticAberration chromatic;
    LensDistortion lens;
    DepthOfField depth;
    private void Awake()
    {
        vlo = GetComponent<Volume>();

        vlo.profile.TryGet<Tonemapping>(out tone);
        vlo.profile.TryGet<Bloom>(out bloom);
        vlo.profile.TryGet<Vignette>(out vig);
        vlo.profile.TryGet<ChromaticAberration>(out chromatic);
        vlo.profile.TryGet<LensDistortion>(out lens);
        vlo.profile.TryGet<DepthOfField>(out depth);
    }
    public void onoff(bool onoff)
    {
        if (onoff)
        {
            tone.mode.value = TonemappingMode.ACES;
            bloom.intensity.value = 100f;
            vig.intensity.value = .5f;
            chromatic.intensity.value = 1;
            lens.intensity.value = .5f;
        }
        else
        {
            tone.mode.value = TonemappingMode.Neutral;
            bloom.intensity.value = .25f;
            vig.intensity.value = .35f;
            chromatic.intensity.value = 0;
            lens.intensity.value = 0;
        }
    }
    IEnumerator CutIn()
    {
        Time.timeScale = .5f;
        onoff(true);
        yield return new WaitForSeconds(2);
        onoff(false);
    }
}

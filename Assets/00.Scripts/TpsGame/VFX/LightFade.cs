using System.Collections;
using UnityEngine;

public class LightFade : MonoBehaviour
{
    public bool testtest;
    [SerializeField] Light lighta;
    float _intensity;
    float _radius;

    [Header("about fade")]
    [SerializeField] float _fadeTime;
    float _startIntensity;
    float _startRadius;

    [Header("about pop")]
    [SerializeField] float _PopTime;
    [SerializeField] float _goalIntensity;
    [SerializeField] float _goalRadius;

    float _timeCount;
    int mode = 0;
    private void Update()
    {

        if (mode == 0)
        {
            if (testtest)
            {
                testtest = false;
                PopStart();
            }
            else
                return;
        }
        if (mode == 1)
            PoPing();
        else if (mode == 2)
            Fading();
        lighta.range = _radius;
        lighta.intensity = _intensity;
    }

    private void PoPing()
    {
        //1초에 1을 더함
        _timeCount += Time.deltaTime;
        if (_timeCount >= _PopTime)
        {
            FadeStart();
            return;
        }


        //초기 값과 goal/poptime 만큼 더하기
        _intensity += (_goalIntensity / _PopTime) * Time.deltaTime;
        _radius += (_goalRadius / _PopTime) * Time.deltaTime;
    }
    private void Fading()
    {
        _timeCount += Time.deltaTime;
        if (_timeCount >= _fadeTime)
        {
            mode = 0;
            lighta.intensity = 0;
            return;
        }

        _intensity -= (_startIntensity / _fadeTime) * Time.deltaTime;
        _radius -= (_startRadius / _fadeTime) * Time.deltaTime;
    }

    public void PopStart()
    {
        mode = 1;
        _timeCount = 0;
        lighta.intensity = 0;
        lighta.range = 0;
        _intensity = 0;
        _radius = 0;
    }
    public void FadeStart()
    {
        mode = 2;
        _timeCount = 0;
        _radius = lighta.range;
        _startRadius = _radius;
        _intensity = lighta.intensity;
        _startIntensity = _intensity;
    }
}

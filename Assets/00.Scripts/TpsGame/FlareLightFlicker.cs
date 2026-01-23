using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class FlareLightFlicker : MonoBehaviour
{
    [SerializeField] float max, min, intesity;
    Light lighjt;
    private void Start()
    {
        flicker();
    }
    public void flicker()
    {
        lighjt = GetComponent<Light>();
        StartCoroutine(flick());
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator flick()
    {
        yield return new WaitForSeconds(intesity);
        lighjt.intensity = Random.Range(min, max);
        StartCoroutine(flick());
    }
}

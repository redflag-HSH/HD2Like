using UnityEngine;
using UnityEngine.VFX;

public class BonFire : MonoBehaviour
{
    GameObject modelObject;
    public GameObject[] models;
    float radius;
    public LayerMask playerlay;
    public int level;
    public float firePow;
    VisualEffect effect;
    public void SafeZone()
    {
        Collider[] hitted = Physics.OverlapSphere(transform.position, radius, playerlay);
        foreach (Collider col in hitted)
        {
            if (col.TryGetComponent<PlayerStat>(out PlayerStat sta))
            {
                sta.frosting = false;
                sta.frostbite = 0;
                sta.Warmth = 3;
            }
        }
    }
    public void SetLevel()
    {
        if (modelObject != null)
                Destroy(modelObject);
        modelObject = Instantiate(models[level].gameObject,transform.position,transform.rotation);
        //단계는 1드럼통 2 모닥불 3 화톳불 
        if (firePow > 50)
        {
            GetComponent<Light>().intensity = 70;
            effect.SetFloat("Size", 10);
        }
        else if (firePow > 30)
        {
            GetComponent<Light>().intensity = 50;
            effect.SetFloat("Size", 15);
        }
        else
        {
            GetComponent<Light>().intensity = 20;
            effect.SetFloat("Size", 20);
        }
        radius = firePow * 1.5f;
        
        //2.광원 단계에 따라 커지게
        //* 3.눈 단계에 따라 점점 사라지게
        //게이지 최하지인 경우 광원 단계 및 눈 단계 비활성화
    }
    private void Start()
    {
        SetLevel();
    }
    private void Update()
    {
        SafeZone();
    }
}

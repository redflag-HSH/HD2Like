using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class RoutineManager : MonoBehaviour
{
    public Transform MainFire;
    public Light Directional;
    [Header("라운드 시간 관련")]
    public float dayTime;
    public float nightTime;
    [Header("날짜 관련")]
    public int days;
    public bool dayNight;
    private void Awake()
    {
        ToNextPhase();
    }
    public void DayPass()
    {
        days++;
        //7일 지날경우 게임 종료

    }
    public void ToNextPhase()
    {
        dayNight = !dayNight;
        if (dayNight)
        {
            //조명 교체
            Directional.transform.rotation = quaternion.Euler(65, -30, 0);
            RenderSettings.fog = true;
            RenderSettings.fogColor = Color.white;
            RenderSettings.fogDensity = .01f;

            //랜덤하게 아이템 생성
            //생존자측 미션
            //컬티측 미션
            DayPass();
        }
        else
        {
            //조명 교체
            Directional.transform.rotation = quaternion.Euler(270, -30, 0);
            Debug.Log(Directional.transform.eulerAngles);
            RenderSettings.fog = false;
            RenderSettings.fogDensity = 0f;
            //생존자측 미션
            //컬티측 미션
            //몬스터 웨이브 계산
        }
        StartCoroutine(dayNightChangeWait());
    }
    IEnumerator dayNightChangeWait()
    {
        if (dayNight)
            yield return new WaitForSeconds(dayTime);
        else
            yield return new WaitForSeconds(nightTime);
        ToNextPhase();
    }
}

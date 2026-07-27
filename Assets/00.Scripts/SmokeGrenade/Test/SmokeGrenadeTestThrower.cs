using UnityEngine;
using UnityEngine.InputSystem;

// 테스트 씬 전용 투척 입력. 인벤토리/탄약 시스템과 무관하게 클릭하면 바로 던진다.
// CS2처럼 일반 투척(LMB)과 로브 투척(RMB) 두 가지 궤적을 지원한다.
public class SmokeGrenadeTestThrower : MonoBehaviour
{
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform throwOrigin;

    [Header("일반 투척 (LMB)")]
    [SerializeField] private float normalThrowForce = 16f;
    [SerializeField] private float normalUpwardBias = 0.12f;

    [Header("로브 투척 (RMB)")]
    [SerializeField] private float lobThrowForce = 9f;
    [SerializeField] private float lobUpwardBias = 0.9f;

    private Transform Origin => throwOrigin != null ? throwOrigin : transform;

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || grenadePrefab == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
            Throw(normalThrowForce, normalUpwardBias);
        else if (mouse.rightButton.wasPressedThisFrame)
            Throw(lobThrowForce, lobUpwardBias);
    }

    void Throw(float force, float upwardBias)
    {
        Vector3 direction = (Origin.forward + Vector3.up * upwardBias).normalized;
        Vector3 spawnPos = Origin.position + Origin.forward * 0.6f;

        GameObject grenade = Instantiate(grenadePrefab, spawnPos, Quaternion.identity);
        grenade.GetComponent<SmokeGrenadeProjectile>().Throw(direction * force);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 460, 40),
            "LMB: 일반 투척   RMB: 로브 투척   WASD/QE + 마우스: 이동/시점   Esc: 커서 잠금 해제");

        float size = 4f;
        GUI.DrawTexture(new Rect(Screen.width / 2f - size / 2f, Screen.height / 2f - size / 2f, size, size),
            Texture2D.whiteTexture);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

// 테스트 씬 전용 자유 이동 카메라. 실제 플레이어/네트워크 시스템과는 무관하며,
// 연막탄을 던지고 관찰하는 용도로만 쓰인다.
public class SmokeGrenadeTestPlayer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float lookSensitivity = 0.1f;

    private float _yaw;
    private float _pitch;

    void Start()
    {
        _yaw = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }

        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 delta = mouse.delta.ReadValue();
            _yaw += delta.x * lookSensitivity;
            _pitch = Mathf.Clamp(_pitch - delta.y * lookSensitivity, -85f, 85f);
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        Vector3 move = Vector3.zero;
        if (keyboard.wKey.isPressed) move += transform.forward;
        if (keyboard.sKey.isPressed) move -= transform.forward;
        if (keyboard.aKey.isPressed) move -= transform.right;
        if (keyboard.dKey.isPressed) move += transform.right;
        if (keyboard.eKey.isPressed) move += Vector3.up;
        if (keyboard.qKey.isPressed) move -= Vector3.up;

        float speed = moveSpeed * (keyboard.leftShiftKey.isPressed ? sprintMultiplier : 1f);
        transform.position += move.normalized * speed * Time.deltaTime;
    }
}

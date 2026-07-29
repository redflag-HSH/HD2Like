using UnityEngine;
using UnityEngine.InputSystem;

namespace SmokeSystem
{
    /// <summary>Free-fly WASD + mouse-look rig for exercising the smoke system without the full player/network stack.</summary>
    public class SmokeTestPlayerController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float sprintMultiplier = 2f;
        [SerializeField] float lookSensitivity = 0.1f;

        float yaw;
        float pitch;

        void Start()
        {
            yaw = transform.eulerAngles.y;
            pitch = transform.eulerAngles.x;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (keyboard == null)
                return;

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                bool locked = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = locked;
            }

            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * lookSensitivity;
                pitch = Mathf.Clamp(pitch - delta.y * lookSensitivity, -85f, 85f);
                transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
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

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 480, 44),
                "WASD/QE + 마우스: 이동/시점   Shift: 빠르게   Esc: 커서 잠금 해제\n" +
                "RMB: 연막탄 투척   F: 정면에 구멍 뚫기(투사체/근접 판정 시뮬레이션)");

            const float size = 4f;
            GUI.DrawTexture(new Rect(Screen.width / 2f - size / 2f, Screen.height / 2f - size / 2f, size, size),
                Texture2D.whiteTexture);
        }
    }
}

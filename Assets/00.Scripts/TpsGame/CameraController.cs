using System.Linq;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Movement movement;

    TPSActions _actions;
    TPSActions.TpsDefalutActions _tpsActions;


    [Header("refernces")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    Vector2 _input;

    public float rotationSpeed;

    public Transform combatLookAt;
    public CameraStyle currentStyle;

    public GameObject[] cams;
    public enum CameraStyle
    {
        Basic,
        combat,
        TopDown
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _actions = new TPSActions();
        _tpsActions = _actions.tpsDefalut;
        _actions.Enable();
        SwitchCameraStyle(currentStyle);
        movement = player.GetComponent<Movement>();

    }
    private void Update()
    {
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;
        Vector2 input = _tpsActions.Move.ReadValue<Vector2>();
        if (movement.freeze)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, orientation.forward, rotationSpeed * Time.deltaTime);
        }
        else if (currentStyle == CameraStyle.Basic || currentStyle == CameraStyle.TopDown)
        {

            Vector3 inputDir = orientation.forward * input.y + orientation.right * input.x;

            if (inputDir != Vector3.zero)
            {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, rotationSpeed * Time.deltaTime);
            }
        }
        else if (currentStyle == CameraStyle.combat)
        {
            Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = dirToCombatLookAt.normalized;

            playerObj.forward = dirToCombatLookAt.normalized;
        }
    }

    public void LockM(bool onOff)
    {
        if (onOff)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
    public void SwitchCameraStyle(CameraStyle newStyle)
    {
        foreach (GameObject ga in cams)
            ga.SetActive(false);


        if (newStyle == CameraStyle.Basic)
            cams[0].SetActive(true);
        else if (newStyle == CameraStyle.combat)
            cams[1].SetActive(true);
        else if (newStyle == CameraStyle.TopDown)
            cams[2].SetActive(true);

    }
}

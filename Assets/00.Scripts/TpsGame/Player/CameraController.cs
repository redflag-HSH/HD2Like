using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public PlayingMovement movement;
    public WeaponController weaponController;

    TPSActions _actions;
    TPSActions.TpsDefalutActions _tpsActions;

    public List<float> frostDrag;

    [Header("refernces")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;


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
        movement = player.GetComponent<PlayingMovement>();
        if (instance == null)
            instance = this;
        else
            Destroy(this);
        foreach (GameObject cam in cams)
        {
            cam.GetComponent<CinemachineCamera>().Follow = player;
            cam.GetComponent<CinemachineCamera>().LookAt = player;
        }
        cams[1].GetComponent<CinemachineCamera>().LookAt = combatLookAt;

    }
    private void Update()
    {

        AimZoomCheck();


        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;
        Vector2 input = _tpsActions.Move.ReadValue<Vector2>();
        if (movement.movementFreeze)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, orientation.forward, (rotationSpeed - frostDrag[movement.frostLevel]) * Time.deltaTime);
        }
        else if (currentStyle == CameraStyle.Basic)
        {

            Vector3 inputDir = orientation.forward * input.y + orientation.right * input.x;

            if (inputDir != Vector3.zero)
            {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, (rotationSpeed - frostDrag[movement.frostLevel]) * Time.deltaTime);
            }
            weaponController.BasicFollow(playerObj);
        }
        else if (currentStyle == CameraStyle.combat)
        {
            Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = dirToCombatLookAt.normalized;

            playerObj.forward = dirToCombatLookAt.normalized;
            weaponController.FollowTarget();
        }
        else if (currentStyle == CameraStyle.TopDown)
        {

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

        currentStyle = newStyle;

    }
    private void AimZoomCheck()
    {
        if (!movement.isActiveAndEnabled)
        {
            SwitchCameraStyle(CameraStyle.TopDown);
            return; 
        } 
        /*else if (weaponController.currentWeapon == null)
        {
            if (currentStyle == CameraStyle.combat)
                SwitchCameraStyle(CameraStyle.Basic);
            return;
        }*/
        if (_tpsActions.AimZoom.WasPressedThisFrame())
            SwitchCameraStyle(CameraStyle.combat);
        else if (_tpsActions.AimZoom.WasReleasedThisFrame())
            SwitchCameraStyle(CameraStyle.Basic);
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayingMovement : NetworkBehaviour
{
    [Header("For Testing")]
    public bool isTest;

    [Header("for characterController")]
    CharacterController _characterController;

    [Header("Movement")]
    [SerializeField] float moveSpeed;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool _isGrounded;


    [Header("for cam")]
    public Transform orientation;
    public Transform plaObj;
    public Transform combatLookat;

    float _gravity = -4.8f;
    Vector3 _playerVelocity;

    public bool movementFreeze;
    public MovementState state;
    public enum MovementState
    {
        freeze,
        walking,
        sprinting,
        air
    }

    public bool sprinting;

    bool restricted;

    WeaponController controller;


    public TPSActions _actions;
    TPSActions.TpsDefalutActions _tpsActions;
    Vector2 _input;

    public int frostLevel;
    public List<float> frostDrag;

    public Interactor canInteractor;

    [Header("inventory")]
    List<Weapon> weapons;

    bool owner = false;

    private void Start()
    {
        if (isTest)
        {
            StartInitialize();
            owner = true;
        }
    }


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            owner = true;
            Debug.Log("owner is " + OwnerClientId);
        }
        else
        {
            Debug.Log("You're not owner of " + OwnerClientId);
            return;
        }


        StartInitialize();
        base.OnNetworkSpawn();
        StartCoroutine(RPCCall());
    }
    IEnumerator RPCCall()
    {
        playerCustom a = FindFirstObjectByType<playerCustom>();
        a.player = this;
        yield return new WaitForEndOfFrame();
        MultiManager.instance.playerMatGetServerRpc(a.GetPlayerMat());
    }


    public void StartInitialize()
    {
        controller = GetComponentInChildren<WeaponController>();

        CamReferenceSet();

        _characterController = GetComponent<CharacterController>();


        weapons = new List<Weapon>();

        _playerVelocity = Vector3.zero;

        _actions = new TPSActions();
        _actions.Enable();
        _tpsActions = _actions.tpsDefalut;
        //performed는 relase
        //started는 pressed
        _tpsActions.Attack.performed += ctx => AttackTrigger();
        _tpsActions.Interact.performed += ctx => InteractTry();
        _tpsActions._1.performed += ctx => ChangeWeapon(0);
        _tpsActions._2.performed += ctx => ChangeWeapon(1);
        _tpsActions._3.performed += ctx => ChangeWeapon(2);
        _tpsActions._4.performed += ctx => ChangeWeapon(3);
        _tpsActions.Discard.performed += ctx => Discard();
    }


    private void Update()
    {
        if (!owner)
            return;
        // ground check
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

    }

    private void FixedUpdate()
    {
        if (!owner)
            return;
        // Debug.Log(_tpsActions.Move.ReadValue<Vector2>());
        ProcessMove(_tpsActions.Move.ReadValue<Vector2>());
        InteractCheck();
    }

    public void CamReferenceSet()
    {
        CameraController cont;
        cont = FindFirstObjectByType<CameraController>();
        cont.playerObj = plaObj;
        cont.player = gameObject.transform;
        cont.orientation = orientation;
        cont.rb = GetComponent<Rigidbody>();
        cont.combatLookAt = combatLookat;
        cont.weaponController = controller;
        cont.enabled = true;
    }


    private void AttackTrigger()
    {
        controller.Fire();
    }

    public void ProcessMove(Vector2 input)
    {
        if (input != Vector2.zero)
        {
            _characterController.Move(orientation.forward * input.y * moveSpeed * Time.deltaTime);
            _characterController.Move(orientation.right * input.x * moveSpeed * Time.deltaTime);
        }

        _playerVelocity.y += _gravity * Time.deltaTime;
        if (_isGrounded && _playerVelocity.y < 0)
            _playerVelocity.y = -2f;
        _characterController.Move(_playerVelocity * Time.deltaTime);
    }



    private void InteractCheck()
    {
        Collider[] detects = Physics.OverlapSphere(transform.position, 2.2f);
        bool isThereaInteractor = false;
        foreach (Collider collider in detects)
        {
            if (collider.TryGetComponent<Interactor>(out Interactor t))
            {
                //ui띄우기

                //상호작용 가능 오브젝트에 할당
                canInteractor = t;
                isThereaInteractor = true;
                break;
            }
        }
        if (!isThereaInteractor)
            canInteractor = null;
    }
    private void InteractTry()
    {
        if (canInteractor != null)
        {
            canInteractor.OnInteract(this);
            canInteractor = null;
        }
    }
    public void AddWeapon(Weapon weapon, int ammo)
    {
        if (weapons.Count < 4)
        {
            Debug.Log(weapon.gameObject);
            if (weapon.type == Weapon.weaponType.shooter)
            {
                weapon.transform.position = transform.position;
                weapon.transform.rotation = controller.transform.rotation;
                weapon.transform.SetParent(controller.transform, true);
                if (ammo == -1)
                    weapon.AmmoReset();
                else
                    weapon.AmmoReset(ammo);
            }
            else if (weapon.type == Weapon.weaponType.meele)
            {
                weapon.transform.position = controller.meeleTransform.position;
                weapon.transform.rotation = controller.meeleTransform.rotation;
                weapon.transform.SetParent(controller.meeleTransform, true);
            }
            weapons.Add(weapon);
        }
        weapon.gameObject.SetActive(false);
    }
    public void RemoveWeapon(Weapon weapon)
    {
        foreach (Weapon weapon1 in weapons)
        {
            if (weapon1 == weapon)
            {
                weapons.Remove(weapon1);
                break;
            }
        }
    }
    public void ChangeWeapon(int num)
    {
        if (weapons[num] != null)
            controller.ChangeWeapon(weapons[num]);
    }
    public void Discard()
    {
        if (controller.currentWeapon != null)
        {
            controller.currentWeapon.Discard();
            controller.ChangeWeapon();
        }
    }


    public void MatSet(Color c)
    {
        plaObj.GetComponent<Renderer>().material.color = c;
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    WeaponController controller;


    public TPSActions _actions;
    TPSActions.TpsDefalutActions _tpsActions;

    public int frostLevel;
    public List<float> frostDrag;

    public Interactor canInteractor;

    [Header("Projectiles")]
    [SerializeField] public List<GameObject> projectilePrefabs;

    [Header("inventory")]
    List<Weapon> weapons;
    List<int> weaponPrefabIndices; // parallel to weapons, stores weaponItem.weaponPrefabIndex

    bool owner = false;

    [SerializeField] TextMeshPro _indicatorText;

    // Server-side reference to the currently spawned display weapon NetworkObject.
    // NGO replicates it automatically to all clients; owner is hidden via NetworkHide.
    NetworkObject _displayWeaponNetObj;


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
        controller = GetComponentInChildren<WeaponController>();

        if (IsOwner)
        {
            owner = true;
            Debug.Log("owner is " + OwnerClientId);
            StartInitialize();
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnNetworkSceneLoaded;

            if (VivoxPlayerMapper.Instance != null)
                VivoxPlayerMapper.Instance.RegisterLocalPlayer(OwnerClientId);
        }
        else
        {
            Debug.Log("You're not owner of " + OwnerClientId);
        }

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnNetworkSceneLoaded;

        base.OnNetworkDespawn();
    }

    void OnNetworkSceneLoaded(string sceneName, LoadSceneMode loadMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        CamReferenceSet();
    }


    public void NetworkInitilize()
    {

    }


    public void StartInitialize()
    {
        GetComponent<playerCustom>().SetRandomColor();

        controller = GetComponentInChildren<WeaponController>();

        CamReferenceSet();

        _characterController = GetComponent<CharacterController>();

        weapons = new List<Weapon>();
        weaponPrefabIndices = new List<int>();

        _playerVelocity = Vector3.zero;

        _actions = new TPSActions();
        _actions.Enable();
        _tpsActions = _actions.tpsDefalut;
        //performed= relase
        //started= pressed
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

        // Update Vivox 3D position for positional voice chat
        if (VivoxManager.Instance != null)
            VivoxManager.Instance.Set3DPosition(gameObject);
    }

    private void FixedUpdate()
    {
        if (!owner)
            return;
        ProcessMove(_tpsActions.Move.ReadValue<Vector2>());
        InteractCheck();
    }

    public void CamReferenceSet()
    {
        CameraController cont = FindFirstObjectByType<CameraController>();
        if (cont == null)
        {
            Debug.LogWarning("CamReferenceSet: CameraController not found in scene");
            return;
        }
        cont.playerObj = plaObj;
        cont.player = gameObject.transform;
        cont.orientation = orientation;
        cont.rb = GetComponent<Rigidbody>();
        cont.combatLookAt = combatLookat;
        cont.weaponController = controller;
        cont.enabled = true;
        cont.SetupPlayerReferences();
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
                IndicatorTextChange(t.GetText());
                canInteractor = t;
                isThereaInteractor = true;
                break;
            }
        }
        if (!isThereaInteractor)
        {
            canInteractor = null;
            IndicatorTextChange("");
        }
    }
    private void InteractTry()
    {
        if (canInteractor != null)
        {
            canInteractor.OnInteract(this);
            canInteractor = null;
        }
    }
    public void AddWeapon(Weapon weapon, int ammo, int prefabIndex = -1)
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
            weaponPrefabIndices.Add(prefabIndex);
        }
        weapon.gameObject.SetActive(false);
    }
    public void RemoveWeapon(Weapon weapon)
    {
        int idx = weapons.IndexOf(weapon);
        if (idx >= 0)
        {
            weapons.RemoveAt(idx);
            weaponPrefabIndices.RemoveAt(idx);
            if (controller.currentWeapon == weapon && IsSpawned)
                SpawnDisplayWeaponServerRpc(-1);
        }
    }
    public void ChangeWeapon(int num)
    {
        if (num < weapons.Count && weapons[num] != null)
        {
            controller.ChangeWeapon(weapons[num]);
            if (IsSpawned && num < weaponPrefabIndices.Count)
                SpawnDisplayWeaponServerRpc(weaponPrefabIndices[num]);
        }
    }
    public void Discard()
    {
        if (controller.currentWeapon != null)
        {
            controller.currentWeapon.Discard();
            controller.ChangeWeapon();
            if (IsSpawned)
                SpawnDisplayWeaponServerRpc(-1);
        }
    }

    [ClientRpc]
    public void ShowBloodOverlayClientRpc(float value)
    {
        if (IsOwner)
            GetComponent<PlayerStat>().ApplyBloodOverlay(value);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageServerRpc(int amount, IDamageable.DamageType type)
    {
        GetComponent<PlayerStat>().Damage(amount, type);
    }

    [ServerRpc]
    public void SpawnProjectileServerRpc(Vector3 pos, Quaternion rot, int damage, int prefabIndex)
    {
        if (prefabIndex < 0 || prefabIndex >= projectilePrefabs.Count) return;
        GameObject go = Instantiate(projectilePrefabs[prefabIndex], pos, rot);
        NetworkObject no = go.GetComponent<NetworkObject>();
        no.Spawn();
        go.GetComponent<Projectile>().SetDamage(damage);
    }

    // Runs on the server. Despawns the old display weapon and spawns a new one
    // parented to the weapon display target. The owner client is hidden from seeing
    // it (they already see their local weapon).
    [ServerRpc]
    void SpawnDisplayWeaponServerRpc(int prefabIndex)
    {
        if (_displayWeaponNetObj != null)
        {
            _displayWeaponNetObj.Despawn();
            _displayWeaponNetObj = null;
        }

        if (prefabIndex < 0 || prefabIndex >= controller.weaponDisplayPrefabs.Count)
            return;

        Transform spawnOn = controller.weaponDisplayTarget != null
            ? controller.weaponDisplayTarget
            : controller.transform;

        GameObject go = Instantiate(controller.weaponDisplayPrefabs[prefabIndex]);
        _displayWeaponNetObj = go.GetComponent<NetworkObject>();
        _displayWeaponNetObj.Spawn();
        _displayWeaponNetObj.TrySetParent(spawnOn, false); // false = keep local position (zeroed)

        // Hide from owner — they see their own local weapon instance
        _displayWeaponNetObj.NetworkHide(OwnerClientId);
    }


    public void MatSet(Color c)
    {
        plaObj.GetComponent<Renderer>().material.color = c;
    }

    public void IndicatorTextChange(string text)
    {
        _indicatorText.text = text;
    }

    [ClientRpc]
    public void DieClientRpc()
    {
        // Hide the dead player's model on all clients
        plaObj.gameObject.SetActive(false);

        // Owner-only: local input, camera, and UI cleanup
        if (IsOwner)
            OnDeath();
    }

    public void OnDeath()
    {
        movementFreeze = true;
        canInteractor = null;
        IndicatorTextChange("");
        enabled = false;
        GetComponent<PlayerStat>().ApplyBloodOverlay(15);
        CameraController.instance.SwitchCameraStyle(CameraController.CameraStyle.TopDown);
        if (VivoxManager.Instance != null)
            VivoxManager.Instance.SetSpectatorMode(true);
    }
}

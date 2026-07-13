using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// Floating world-space name tag above the player, driven by playerCustom's
// synced name/color. Setup: assign a world-space TextMeshPro child (not
// TextMeshProUGUI) to nameLabel on the player prefab.
[RequireComponent(typeof(playerCustom))]
public class PlayerNameplate : NetworkBehaviour
{
    [SerializeField] TextMeshPro nameLabel;
    [SerializeField] Vector3 offset = new Vector3(0f, 2.2f, 0f);

    playerCustom _custom;
    Transform _cam;

    void Awake()
    {
        _custom = GetComponent<playerCustom>();
    }

    public override void OnNetworkSpawn()
    {
        _custom.playerName.OnValueChanged += OnNameChanged;
        ApplyName(_custom.playerName.Value);
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        _custom.playerName.OnValueChanged -= OnNameChanged;
        base.OnNetworkDespawn();
    }

    void OnNameChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        ApplyName(current);
    }

    void ApplyName(FixedString32Bytes value)
    {
        if (nameLabel == null)
        {
            Debug.LogWarning($"[NameSync] nameplate got '{value}' but nameLabel is not assigned on {gameObject.name}");
            return;
        }
        Debug.Log($"[NameSync] nameplate applying '{value}' (owner: {IsOwner})");
        nameLabel.text = value.ToString();
    }

    void LateUpdate()
    {
        if (nameLabel == null) return;

        nameLabel.transform.position = transform.position + offset;

        if (_cam == null && Camera.main != null)
            _cam = Camera.main.transform;
        if (_cam != null)
            nameLabel.transform.rotation = Quaternion.LookRotation(nameLabel.transform.position - _cam.position);
    }
}

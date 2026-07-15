using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class playerCustom : NetworkBehaviour
{
    public static readonly Color[] PresetColors =
    {
        new Color(0.85f, 0.20f, 0.20f), // red
        new Color(0.20f, 0.45f, 0.85f), // blue
        new Color(0.20f, 0.75f, 0.30f), // green
        new Color(0.90f, 0.75f, 0.20f), // yellow
        new Color(0.85f, 0.40f, 0.85f), // pink
        new Color(0.95f, 0.55f, 0.15f), // orange
        new Color(0.40f, 0.85f, 0.85f), // cyan
        new Color(0.60f, 0.40f, 0.90f), // purple
    };

    // Written by the title-screen PlayerCustomizationUI, read here on spawn.
    public const string PrefsColorKey = "PlayerCustom_ColorIndex";
    public const string PrefsNameKey = "PlayerCustom_Name";

    NetworkVariable<Color> playerColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    [SerializeField] Renderer targetRenderer;

    public override void OnNetworkSpawn()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        playerColor.OnValueChanged += OnColorChanged;

        if (IsOwner)
        {
            int savedIndex = PlayerPrefs.GetInt(PrefsColorKey, Random.Range(0, PresetColors.Length));
            string savedName = PlayerPrefs.GetString(PrefsNameKey, "Player" + Random.Range(1000, 9999));
            SetColorIndex(savedIndex);
            SetName(savedName);
        }

        ApplyColor(playerColor.Value);

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        playerColor.OnValueChanged -= OnColorChanged;
        base.OnNetworkDespawn();
    }

    private void OnColorChanged(Color previous, Color current)
    {
        ApplyColor(current);
    }

    private void ApplyColor(Color c)
    {
        if (targetRenderer != null)
            targetRenderer.material.color = c;
    }

    public void SetRandomColor()
    {
        if (!IsOwner) return;
        SetColorIndex(Random.Range(0, PresetColors.Length));
    }

    public void SetColorIndex(int index)
    {
        if (!IsOwner) return;
        index = ((index % PresetColors.Length) + PresetColors.Length) % PresetColors.Length;
        playerColor.Value = PresetColors[index];
        PlayerPrefs.SetInt(PrefsColorKey, index);
    }

    public void SetName(string name)
    {
        if (!IsOwner)
        {
            Debug.LogWarning("[NameSync] SetName skipped: not owner");
            return;
        }
        if (string.IsNullOrWhiteSpace(name)) return;

        // FixedString32Bytes holds at most 29 bytes of UTF-8, not 32 chars -
        // append whole characters until full so long/Korean names truncate
        // cleanly instead of overflowing (which throws and loses the write).
        FixedString32Bytes fixedName = default;
        foreach (char c in name)
        {
            FixedString32Bytes candidate = fixedName;
            if (candidate.Append(c) != FormatError.None)
                break;
            fixedName = candidate;
        }
        if (fixedName.IsEmpty) return;

        Debug.Log($"[NameSync] SetName writing '{fixedName}' (typed '{name}')");
        playerName.Value = fixedName;
        PlayerPrefs.SetString(PrefsNameKey, fixedName.ToString());
    }

    public Color GetPlayerColor()
    {
        return playerColor.Value;
    }
}

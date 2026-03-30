using Unity.Netcode;
using UnityEngine;

public class playerCustom : NetworkBehaviour
{
    NetworkVariable<Color> playerColor = new NetworkVariable<Color>(
        Color.white,
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
            playerColor.Value = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

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
        playerColor.Value = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    public Color GetPlayerColor()
    {
        return playerColor.Value;
    }
}

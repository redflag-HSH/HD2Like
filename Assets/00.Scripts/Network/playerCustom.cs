using Unity.Netcode;
using UnityEngine;

public class playerCustom : MonoBehaviour
{
    NetworkVariable<Color> playerColor = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    Color plaColor;
    public PlayingMovement player;

    private void Awake()
    {
        plaColor = Color.white;
    }
    public void SetRandomColor()
    {
        plaColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        //playerColor.Value = plaColor;
    }
    public void SetPlayerMat(Renderer r)
    {
        r.material.color = plaColor;
    }
    public Color GetPlayerMat()
    {
        return plaColor;
    }
}

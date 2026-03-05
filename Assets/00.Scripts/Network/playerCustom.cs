using UnityEngine;

public class playerCustom : MonoBehaviour
{
    Color plaColor;
    public PlayingMovement player;

    private void Awake()
    {
        plaColor = Color.white;
    }
    public void SetRandomColor()
    {
        plaColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
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

using UnityEngine;

public class LobbyStartInteractor : Interactor
{
    [SerializeField] Lobby lobby;

    public override void OnInteract(PlayingMovement player)
    {
        lobby.OnHostStart(player);
    }
}

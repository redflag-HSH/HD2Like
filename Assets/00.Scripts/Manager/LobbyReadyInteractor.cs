using UnityEngine;

public class LobbyReadyInteractor : Interactor
{
    [SerializeField] Lobby lobby;
    [SerializeField] string readyText = "[E] Ready";
    [SerializeField] string cancelText = "[E] Cancel Ready";

    bool _isReady = false;

    public override void OnInteract(PlayingMovement player)
    {
        _isReady = !_isReady;
        lobby.OnPlayerToggleReady(player, _isReady);
    }

    public override string GetText()
    {
        return _isReady ? cancelText : readyText;
    }
}

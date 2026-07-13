using UnityEngine;

// World interactor placed in LobbyScene, mirroring LobbyReadyInteractor/
// LobbyStartInteractor. Lets any player - host or client - leave the
// session: for a client this just disconnects them, for the host this
// shuts the server down for everyone (see ClientManager.Disconnect).
public class LobbyLeaveInteractor : Interactor
{
    [SerializeField] string leaveText = "[E] Leave Game";

    public override void OnInteract(PlayingMovement player)
    {
        FindFirstObjectByType<ClientManager>()?.Disconnect();
    }

    public override string GetText()
    {
        return leaveText;
    }
}

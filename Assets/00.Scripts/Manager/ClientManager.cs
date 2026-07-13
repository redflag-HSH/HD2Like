using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(WaitForNetworkManager());
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
    IEnumerator WaitForNetworkManager()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton != null);
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    // A non-host client that loses its connection to the server (host quit,
    // network drop, kicked) falls back to the Title scene locally rather
    // than being left stuck in a dead session.
    void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return;
        if (clientId != NetworkManager.Singleton.LocalClientId) return;
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) return;
        LoadScene("Title");
    }
    public void LockM(bool onOff)
    {
        if (onOff)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name == "")
        {
            LockM(true);
        }
    }
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }
    public void LoadSceneNetwork(string name)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
    }
    // Host-only: sends everyone back to the lobby to set up another match.
    public void ReturnToLobby()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            LoadSceneNetwork("LobbyScene");
    }
    // Leaves the current session. For a client this just disconnects them;
    // for the host this shuts the server down, dropping every client.
    public void Disconnect()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            if (NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();
        }
        LoadScene("Title");
    }
    public void Quit()
    {
        Application.Quit();
    }
    /* public void Test()
     {
         network
         ClientManager
     }*/
}

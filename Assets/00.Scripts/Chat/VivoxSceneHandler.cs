using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives VivoxManager through the game's scene flow automatically.
/// Attach this to the same DontDestroyOnLoad GameObject as VivoxManager.
///
/// Flow:
///   Title      → InitializeAsync + LoginAsync
///   LobbyScene → JoinGroupChannelAsync   (flat voice/text for lobby)
///   PlayScene  → LeaveAllChannels + JoinProximityChannelAsync (3-D voice)
///   Disconnect → LeaveAllChannels + LogoutAsync
/// </summary>
public class VivoxSceneHandler : MonoBehaviour
{
    public static VivoxSceneHandler Instance { get; private set; }

    [Tooltip("Display name shown to other players in voice chat.")]
    public string playerName = "Player";

    [Tooltip("Room ID shared by all players in the same session.")]
    public string roomId = "default";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ─────────────────────────────────────────────────────────────
    //  Scene transitions
    // ─────────────────────────────────────────────────────────────

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Title":
                StartCoroutine(InitAndLogin());
                break;

            case "LobbyScene":
                StartCoroutine(JoinLobby());
                break;

            case "PlayScene":
                StartCoroutine(JoinPlay());
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Coroutines (Tasks can't run directly on scene callbacks)
    // ─────────────────────────────────────────────────────────────

    IEnumerator InitAndLogin()
    {
        var task = VivoxManager.Instance.InitializeAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        task = VivoxManager.Instance.LoginAsync(playerName);
        yield return new WaitUntil(() => task.IsCompleted);
    }

    IEnumerator JoinLobby()
    {
        // Leave any previous channels first
        var leave = VivoxManager.Instance.LeaveAllChannelsAsync();
        yield return new WaitUntil(() => leave.IsCompleted);

        // Flat group channel for lobby voice + text
        var join = VivoxManager.Instance.JoinGroupChannelAsync(roomId);
        yield return new WaitUntil(() => join.IsCompleted);
    }

    IEnumerator JoinPlay()
    {
        // Leave lobby channel
        var leave = VivoxManager.Instance.LeaveAllChannelsAsync();
        yield return new WaitUntil(() => leave.IsCompleted);

        // Positional voice channel for in-game proximity audio
        var join = VivoxManager.Instance.JoinProximityChannelAsync(roomId);
        yield return new WaitUntil(() => join.IsCompleted);
    }

    // ─────────────────────────────────────────────────────────────
    //  Public API — call these from your existing scripts
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Set the player's Vivox display name. Returns this for chaining.
    /// </summary>
    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    /// <summary>
    /// Set the shared room ID for all players in the session. Returns this for chaining.
    /// </summary>
    public void SetRoomId(string id)
    {
        roomId = id;
    }

    /// <summary>
    /// Initialize Vivox and log in with the current playerName.
    /// Call this after SetPlayerName (and optionally SetRoomId) on the Title screen.
    /// Example: VivoxSceneHandler.Instance.SetPlayerName("Alice").SetRoomId("room1").Initialize();
    /// </summary>
    public void Initialize()
    {
        StartCoroutine(InitAndLogin());
    }

    /// <summary>
    /// Call this on disconnect or when returning to Title.
    /// Wired to NetworkManager.OnClientDisconnectCallback in your lobby/network scripts.
    /// </summary>
    public void OnDisconnect()
    {
        StartCoroutine(CleanupVivox());
    }

    IEnumerator CleanupVivox()
    {
        var leave = VivoxManager.Instance.LeaveAllChannelsAsync();
        yield return new WaitUntil(() => leave.IsCompleted);

        var logout = VivoxManager.Instance.LogoutAsync();
        yield return new WaitUntil(() => logout.IsCompleted);
    }
}

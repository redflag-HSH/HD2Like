using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Full-screen win/lose banner shown to every client when GameRoleManager
// broadcasts the match result. The host automatically returns everyone to
// the lobby a few seconds later (see GameRoleManager.ReturnToLobbyAfterDelay)
// - this panel just displays the result and a countdown, then hides itself
// once the Lobby scene loads. Same singleton + UIDocument setup as
// VoiceRosterUI/RoleRevealUI.
//
// Setup: add a UIDocument to this GameObject, assign a PanelSettings asset
// and GameEndPanel.uxml as its Source Asset.
[RequireComponent(typeof(UIDocument))]
public class GameEndUI : MonoBehaviour
{
    public static GameEndUI Instance { get; private set; }

    UIDocument _document;
    VisualElement _root;
    Label _titleLabel;
    Label _countdownLabel;
    Coroutine _countdownRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _document = GetComponent<UIDocument>();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void OnEnable()
    {
        _root = _document.rootVisualElement;
        _titleLabel = _root.Q<Label>("game-end-title");
        _countdownLabel = _root.Q<Label>("game-end-countdown");

        _root.style.display = DisplayStyle.None;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // The auto-return lands everyone back in the lobby - the banner has
        // done its job by then.
        if (scene.name == "LobbyScene")
            SetVisible(false);
    }

    public void ShowResult(RoleType winningTeam, float returnDelay)
    {
        if (winningTeam == RoleType.Cultist)
        {
            _titleLabel.text = "Cultists Win";
            _titleLabel.RemoveFromClassList("game-end-title-survivor");
            _titleLabel.AddToClassList("game-end-title-cultist");
        }
        else
        {
            _titleLabel.text = "Survivors Win";
            _titleLabel.RemoveFromClassList("game-end-title-cultist");
            _titleLabel.AddToClassList("game-end-title-survivor");
        }

        SetVisible(true);

        if (_countdownRoutine != null) StopCoroutine(_countdownRoutine);
        _countdownRoutine = StartCoroutine(CountdownRoutine(returnDelay));
    }

    IEnumerator CountdownRoutine(float seconds)
    {
        float remaining = seconds;
        while (remaining > 0f)
        {
            _countdownLabel.text = $"Returning to lobby in {Mathf.CeilToInt(remaining)}...";
            yield return null;
            remaining -= Time.deltaTime;
        }
        _countdownLabel.text = "Returning to lobby...";
        _countdownRoutine = null;
    }

    void SetVisible(bool visible)
    {
        _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}

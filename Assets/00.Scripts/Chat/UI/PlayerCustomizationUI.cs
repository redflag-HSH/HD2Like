using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Lets the local player set their name and pick a color from playerCustom's
// preset palette. Same singleton + UIDocument setup as VoiceRosterUI/
// RoleRevealUI.
//
// Lobby-only: opens automatically when LobbyScene loads (or when the local
// player spawns there), can be reopened/closed with the P key (C is taken by
// Crouch), and is hidden in every other scene. Unlocks the cursor while open and restores the
// previous lock state on close.
//
// Setup: add a UIDocument to this GameObject, assign a PanelSettings asset
// and PlayerCustomizationPanel.uxml as its Source Asset.
[RequireComponent(typeof(UIDocument))]
public class PlayerCustomizationUI : MonoBehaviour
{
    public static PlayerCustomizationUI Instance { get; private set; }

    const string LobbySceneName = "LobbyScene";

    UIDocument _document;
    VisualElement _root;
    TextField _nameField;
    VisualElement _swatchContainer;
    Button _closeButton;

    playerCustom _target;
    bool _visible;
    CursorLockMode _prevLockState;
    bool _prevCursorVisible;

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
        _nameField = _root.Q<TextField>("name-field");
        _swatchContainer = _root.Q<VisualElement>("color-swatches");
        _closeButton = _root.Q<Button>("close-button");

        _closeButton.clicked += OnCloseClicked;
        _nameField.RegisterValueChangedCallback(OnNameFieldChanged);

        BuildSwatches();
        SetVisible(false);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        _closeButton.clicked -= OnCloseClicked;
        _nameField.UnregisterValueChangedCallback(OnNameFieldChanged);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    static bool InLobby => SceneManager.GetActiveScene().name == LobbySceneName;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == LobbySceneName)
            StartCoroutine(ShowWhenLocalPlayerReady());
        else
            SetVisible(false);
    }

    // The local player object can spawn a few frames after the scene loads
    // (or already exist when returning from PlayScene, since player objects
    // persist across the transition and OnNetworkSpawn won't re-fire).
    IEnumerator ShowWhenLocalPlayerReady()
    {
        float deadline = Time.unscaledTime + 10f;
        while (Time.unscaledTime < deadline && InLobby)
        {
            playerCustom local = FindLocalPlayerCustom();
            if (local != null)
            {
                Show(local);
                yield break;
            }
            yield return null;
        }
    }

    playerCustom FindLocalPlayerCustom()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null || nm.LocalClient == null || nm.LocalClient.PlayerObject == null)
            return null;
        return nm.LocalClient.PlayerObject.GetComponent<playerCustom>();
    }

    void Update()
    {
        if (!InLobby) return;

        Keyboard kb = Keyboard.current;
        if (kb == null || !kb.pKey.wasPressedThisFrame) return;

        if (_visible)
        {
            // Don't close on 'p' while the player is typing their name.
            if (_nameField.panel?.focusController?.focusedElement is VisualElement focused
                && (focused == _nameField || _nameField.Contains(focused)))
                return;
            SetVisible(false);
        }
        else
        {
            playerCustom local = _target != null ? _target : FindLocalPlayerCustom();
            if (local != null)
                Show(local);
        }
    }

    void BuildSwatches()
    {
        _swatchContainer.Clear();
        for (int i = 0; i < playerCustom.PresetColors.Length; i++)
        {
            int index = i;
            Button swatch = new Button(() => _target?.SetColorIndex(index));
            swatch.AddToClassList("color-swatch");
            swatch.style.backgroundColor = playerCustom.PresetColors[i];
            _swatchContainer.Add(swatch);
        }
    }

    void OnNameFieldChanged(ChangeEvent<string> evt)
    {
        // Explicit null check instead of ?. so a destroyed player object
        // (Unity fake-null) is caught too.
        if (_target == null)
        {
            Debug.LogWarning($"[NameSync] name field changed to '{evt.newValue}' but _target is null/destroyed - Show() never ran or the player object is gone");
            return;
        }
        Debug.Log($"[NameSync] name field changed to '{evt.newValue}', forwarding to playerCustom");
        _target.SetName(evt.newValue);
    }

    void OnCloseClicked()
    {
        SetVisible(false);
    }

    public void Show(playerCustom target)
    {
        Debug.Log($"[NameSync] customization panel shown for client {target.OwnerClientId}, current name '{target.playerName.Value}'");
        _target = target;
        _nameField.SetValueWithoutNotify(target.playerName.Value.ToString());
        SetVisible(true);
    }

    void SetVisible(bool visible)
    {
        _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        if (visible == _visible) return;
        _visible = visible;

        // Lobby gameplay locks the cursor; free it while the panel is open
        // so the swatches/name field are clickable, then put it back.
        if (visible)
        {
            _prevLockState = UnityEngine.Cursor.lockState;
            _prevCursorVisible = UnityEngine.Cursor.visible;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        else
        {
            UnityEngine.Cursor.lockState = _prevLockState;
            UnityEngine.Cursor.visible = _prevCursorVisible;
        }
    }
}

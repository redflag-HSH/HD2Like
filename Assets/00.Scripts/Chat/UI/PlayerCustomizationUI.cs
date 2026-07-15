using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Title-screen panel where the local player sets their name and picks a body
// color before hosting/joining. No network session exists yet, so edits are
// saved straight to PlayerPrefs (playerCustom.PrefsNameKey/PrefsColorKey);
// playerCustom reads those keys on spawn and syncs them to everyone. The name
// is also pushed into VivoxSceneHandler so voice chat uses it.
//
// Opens automatically in the Title scene, toggles with the P key there, and
// is hidden in every other scene.
//
// Setup: add a UIDocument to this GameObject, assign a PanelSettings asset
// and PlayerCustomizationPanel.uxml as its Source Asset.
[RequireComponent(typeof(UIDocument))]
public class PlayerCustomizationUI : MonoBehaviour
{
    public static PlayerCustomizationUI Instance { get; private set; }

    const string TitleSceneName = "Title";
    const string SelectedSwatchClass = "color-swatch--selected";

    UIDocument _document;
    VisualElement _root;
    TextField _nameField;
    VisualElement _swatchContainer;
    Button _closeButton;
    bool _visible;

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

        // Title is the first scene, so its sceneLoaded event can fire before
        // this handler is registered - check the active scene directly too.
        if (InTitle)
            ShowPanel();
    }

    void OnDisable()
    {
        _closeButton.clicked -= OnCloseClicked;
        _nameField.UnregisterValueChangedCallback(OnNameFieldChanged);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    static bool InTitle => SceneManager.GetActiveScene().name == TitleSceneName;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == TitleSceneName)
            ShowPanel();
        else
            SetVisible(false);
    }

    void Update()
    {
        if (!InTitle) return;

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
            ShowPanel();
        }
    }

    void ShowPanel()
    {
        string savedName = PlayerPrefs.GetString(playerCustom.PrefsNameKey, "");
        if (string.IsNullOrWhiteSpace(savedName))
        {
            // Same default playerCustom would generate on spawn; saving it now
            // keeps Vivox login and the first spawn consistent.
            savedName = "Player" + Random.Range(1000, 9999);
            SaveName(savedName);
        }

        _nameField.SetValueWithoutNotify(savedName);
        UpdateSelectedSwatch(PlayerPrefs.GetInt(playerCustom.PrefsColorKey, -1));
        SetVisible(true);
    }

    void BuildSwatches()
    {
        _swatchContainer.Clear();
        for (int i = 0; i < playerCustom.PresetColors.Length; i++)
        {
            int index = i;
            Button swatch = new Button(() => OnSwatchClicked(index));
            swatch.AddToClassList("color-swatch");
            swatch.style.backgroundColor = playerCustom.PresetColors[i];
            _swatchContainer.Add(swatch);
        }
    }

    void OnSwatchClicked(int index)
    {
        PlayerPrefs.SetInt(playerCustom.PrefsColorKey, index);
        UpdateSelectedSwatch(index);
    }

    void UpdateSelectedSwatch(int index)
    {
        for (int i = 0; i < _swatchContainer.childCount; i++)
            _swatchContainer[i].EnableInClassList(SelectedSwatchClass, i == index);
    }

    void OnNameFieldChanged(ChangeEvent<string> evt)
    {
        if (string.IsNullOrWhiteSpace(evt.newValue)) return;
        SaveName(evt.newValue);
    }

    void SaveName(string name)
    {
        Debug.Log($"[NameSync] saving name '{name}' to PlayerPrefs");
        PlayerPrefs.SetString(playerCustom.PrefsNameKey, name);

        // Keep the Vivox display name in sync so the voice roster shows the
        // same name (login happens on Title load, so set it as early as we can).
        if (VivoxSceneHandler.Instance != null)
            VivoxSceneHandler.Instance.SetPlayerName(name);
    }

    void OnCloseClicked()
    {
        SetVisible(false);
    }

    void SetVisible(bool visible)
    {
        _visible = visible;
        _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        // Title is a menu scene - make sure the cursor is usable (it can stay
        // locked from gameplay when a session ends mid-match).
        if (visible)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
    }
}

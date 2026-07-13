using UnityEngine;
using UnityEngine.UIElements;

// Lets the local player set their name and pick a color from playerCustom's
// preset palette. Same singleton + UIDocument setup as VoiceRosterUI/
// RoleRevealUI. Shown automatically once for the owner's player when
// playerCustom spawns; can be re-opened by calling Show(playerCustom) again.
//
// Setup: add a UIDocument to this GameObject, assign a PanelSettings asset
// and PlayerCustomizationPanel.uxml as its Source Asset.
[RequireComponent(typeof(UIDocument))]
public class PlayerCustomizationUI : MonoBehaviour
{
    public static PlayerCustomizationUI Instance { get; private set; }

    UIDocument _document;
    VisualElement _root;
    TextField _nameField;
    VisualElement _swatchContainer;
    Button _closeButton;

    playerCustom _target;

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
    }

    void OnDisable()
    {
        _closeButton.clicked -= OnCloseClicked;
        _nameField.UnregisterValueChangedCallback(OnNameFieldChanged);
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
    }
}

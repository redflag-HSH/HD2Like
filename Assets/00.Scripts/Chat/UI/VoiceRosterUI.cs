using System.Collections;
using System.Collections.Generic;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI Toolkit panel showing the local mic-mute toggle plus a per-participant
/// mute toggle and volume slider. Backed by VoiceRosterPanel.uxml/.uss and
/// a per-row VoiceParticipantRow.uxml template (assign in the inspector).
///
/// Persists for the whole game session (like VivoxManager) and stays hidden
/// until the player presses Tab (TPSActions/ToggleRoster) — Among Us style
/// player list, not an always-visible HUD element.
///
/// Setup: add a UIDocument to this GameObject, assign a PanelSettings asset
/// (Assets > Create > UI Toolkit > Panel Settings) and VoiceRosterPanel.uxml
/// as its Source Asset, then assign VoiceParticipantRow.uxml to
/// participantRowTemplate below.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class VoiceRosterUI : MonoBehaviour
{
    public static VoiceRosterUI Instance { get; private set; }

    [SerializeField] VisualTreeAsset participantRowTemplate;

    UIDocument _document;
    VisualElement _root;
    VisualElement _participantList;
    Toggle _micMuteToggle;
    bool _visible;

    TPSActions _actions;

    readonly Dictionary<string, VisualElement> _rows = new Dictionary<string, VisualElement>();

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
        _root            = _document.rootVisualElement;
        _participantList = _root.Q<VisualElement>("participant-list");
        _micMuteToggle   = _root.Q<Toggle>("mic-mute-toggle");

        SetVisible(false);

        _micMuteToggle.SetValueWithoutNotify(false);
        _micMuteToggle.RegisterValueChangedCallback(OnMicMuteToggleChanged);

        _actions = new TPSActions();
        _actions.Enable();
        _actions.tpsDefalut.ToggleRoster.performed += OnToggleRosterPerformed;

        // VivoxManager/VivoxInputController set their singleton in Awake(), which can
        // run after this OnEnable depending on script execution order — wait rather
        // than checking Instance once and silently giving up.
        StartCoroutine(WaitForVivoxInputController());
        StartCoroutine(WaitForVivoxManager());
    }

    void OnToggleRosterPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        SetVisible(!_visible);
    }

    public void SetVisible(bool visible)
    {
        _visible = visible;
        _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    IEnumerator WaitForVivoxInputController()
    {
        yield return new WaitUntil(() => VivoxInputController.Instance != null);
        _micMuteToggle.SetValueWithoutNotify(VivoxInputController.Instance.IsMicMuted);
        VivoxInputController.Instance.OnMicMuteChanged += OnMicMuteChangedExternally;
    }

    IEnumerator WaitForVivoxManager()
    {
        yield return new WaitUntil(() => VivoxManager.Instance != null);
        VivoxManager.Instance.OnParticipantChanged += OnParticipantChanged;
        foreach (KeyValuePair<string, VivoxParticipant> kvp in VivoxManager.Instance.Participants)
            AddRow(kvp.Key);
    }

    void OnDisable()
    {
        StopAllCoroutines();

        _micMuteToggle?.UnregisterValueChangedCallback(OnMicMuteToggleChanged);

        if (VivoxInputController.Instance != null)
            VivoxInputController.Instance.OnMicMuteChanged -= OnMicMuteChangedExternally;

        if (VivoxManager.Instance != null)
            VivoxManager.Instance.OnParticipantChanged -= OnParticipantChanged;

        _actions.tpsDefalut.ToggleRoster.performed -= OnToggleRosterPerformed;
        _actions.Disable();
        _actions.Dispose();

        _participantList?.Clear();
        _rows.Clear();
    }

    void OnMicMuteToggleChanged(ChangeEvent<bool> evt)
    {
        VivoxInputController.Instance?.SetMicMuted(evt.newValue);
    }

    void OnMicMuteChangedExternally(bool muted)
    {
        _micMuteToggle?.SetValueWithoutNotify(muted);
    }

    void OnParticipantChanged(string displayName, bool joined)
    {
        if (joined) AddRow(displayName);
        else        RemoveRow(displayName);
    }

    void AddRow(string displayName)
    {
        if (_rows.ContainsKey(displayName) || participantRowTemplate == null) return;

        VisualElement row     = participantRowTemplate.Instantiate();
        Label nameLabel       = row.Q<Label>("participant-name");
        Toggle muteToggle     = row.Q<Toggle>("mute-toggle");
        Slider volumeSlider   = row.Q<Slider>("volume-slider");

        nameLabel.text = displayName;
        volumeSlider.SetValueWithoutNotify(50f);

        muteToggle.RegisterValueChangedCallback(evt =>
            VivoxInputController.Instance?.SetParticipantMuted(displayName, evt.newValue));

        volumeSlider.RegisterValueChangedCallback(evt =>
            VivoxInputController.Instance?.SetParticipantVolumeNormalized(displayName, evt.newValue / 100f));

        _participantList.Add(row);
        _rows[displayName] = row;
    }

    void RemoveRow(string displayName)
    {
        if (!_rows.TryGetValue(displayName, out VisualElement row)) return;
        _participantList.Remove(row);
        _rows.Remove(displayName);
    }
}

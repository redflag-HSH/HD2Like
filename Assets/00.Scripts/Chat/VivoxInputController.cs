using System;
using UnityEngine;

/// <summary>
/// Player-facing voice chat controls.
///
/// Mic mute: bound to both the "MicMute" input action (TPSActions/tpsDefalut) and
/// callable directly from a UI button — ToggleMicMute()/SetMicMuted(bool).
///
/// Participant volume/mute: UI-driven only. Wire a per-participant slider/button in
/// the roster UI to SetParticipantVolume(displayName, volume) / SetParticipantMuted(displayName, muted).
/// </summary>
public class VivoxInputController : MonoBehaviour
{
    public static VivoxInputController Instance { get; private set; }

    TPSActions _actions;
    TPSActions.TpsDefalutActions _tpsActions;

    bool _micMuted;
    public bool IsMicMuted => _micMuted;

    /// <summary>Raised whenever the local mic mute state changes, for UI to reflect (e.g. icon toggle).</summary>
    public event Action<bool> OnMicMuteChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        _actions = new TPSActions();
        _actions.Enable();
        _tpsActions = _actions.tpsDefalut;
        _tpsActions.MicMute.performed += OnMicMutePerformed;
    }

    void OnDisable()
    {
        _tpsActions.MicMute.performed -= OnMicMutePerformed;
        _actions.Disable();
        _actions.Dispose();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void OnMicMutePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        ToggleMicMute();
    }

    // ─────────────────────────────────────────────────────────────
    //  Mic mute — input action + UI
    // ─────────────────────────────────────────────────────────────

    public void ToggleMicMute() => SetMicMuted(!_micMuted);

    public void SetMicMuted(bool muted)
    {
        _micMuted = muted;
        VivoxManager.Instance?.SetMicrophoneMuted(muted);
        OnMicMuteChanged?.Invoke(muted);
    }

    // ─────────────────────────────────────────────────────────────
    //  Participant volume/mute — UI only
    // ─────────────────────────────────────────────────────────────

    /// <summary>volume: -50 (silent) to 50 (loudest). Call from a UI slider's OnValueChanged.</summary>
    public void SetParticipantVolume(string displayName, int volume)
        => VivoxManager.Instance?.SetParticipantVolume(displayName, volume);

    /// <summary>Convenience overload for a 0-1 slider mapped to the -50..50 Vivox volume range.</summary>
    public void SetParticipantVolumeNormalized(string displayName, float normalized01)
        => SetParticipantVolume(displayName, Mathf.RoundToInt(Mathf.Lerp(-50, 50, normalized01)));

    /// <summary>Call from a per-participant mute button in the roster UI.</summary>
    public void SetParticipantMuted(string displayName, bool muted)
        => VivoxManager.Instance?.SetParticipantMuted(displayName, muted);
}

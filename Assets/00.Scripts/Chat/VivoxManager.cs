using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

/// <summary>
/// Manages Vivox voice chat — Vivox SDK 16.x.
///
/// Channels:
///   • Positional  ("proximity-{roomName}") – 3-D audio, fades with distance.
///   • Group       ("lobby-{roomName}")     – flat audio for lobby.
/// </summary>
public class VivoxManager : MonoBehaviour
{
    public static VivoxManager Instance { get; private set; }

    [Header("Channel Names")]
    [SerializeField] string proximityChannelPrefix = "proximity";
    [SerializeField] string groupChannelPrefix     = "lobby";

    [Header("Positional Audio")]
    [SerializeField] int   audibleDistance    = 32;
    [SerializeField] int   conversationalDist = 16;
    [SerializeField] float audioFadeIntensity = 1f;

    bool   _initialized;
    bool   _loggedIn;
    string _proximityChannel;
    string _groupChannel;

    // Keyed by DisplayName — one entry per unique player across channels.
    readonly Dictionary<string, VivoxParticipant> _participants = new Dictionary<string, VivoxParticipant>();

    /// <summary>Raised when a participant joins or leaves a channel.</summary>
    public event Action<string /*displayName*/, bool /*joined*/> OnParticipantChanged;

    /// <summary>Read-only snapshot of all currently tracked participants (keyed by display name).</summary>
    public IReadOnlyDictionary<string, VivoxParticipant> Participants => _participants;

    // ─────────────────────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
        Instance = null;
    }

    // ─────────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            await VivoxService.Instance.InitializeAsync();
            SubscribeEvents();
            _initialized = true;
            Debug.Log("[VivoxManager] Initialized.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[VivoxManager] InitializeAsync failed: {e.Message}");
        }
    }

    public async Task LoginAsync(string displayName)
    {
        if (!_initialized) { Debug.LogWarning("[VivoxManager] Not initialized."); return; }
        if (_loggedIn)      { Debug.LogWarning("[VivoxManager] Already logged in."); return; }
        try
        {
            await VivoxService.Instance.LoginAsync(new LoginOptions { DisplayName = displayName });
            _loggedIn = true;
            Debug.Log($"[VivoxManager] Logged in as '{displayName}'.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[VivoxManager] LoginAsync failed: {e.Message}");
        }
    }

    /// <summary>3-D positional voice channel for in-game proximity audio.</summary>
    public async Task JoinProximityChannelAsync(string roomName)
    {
        if (!_loggedIn) { Debug.LogWarning("[VivoxManager] Must login first."); return; }

        _proximityChannel = $"{proximityChannelPrefix}-{roomName}";

        var properties = new Channel3DProperties(
            audibleDistance,
            conversationalDist,
            audioFadeIntensity,
            AudioFadeModel.LinearByDistance);

        try
        {
            await VivoxService.Instance.JoinPositionalChannelAsync(
                _proximityChannel, ChatCapability.AudioOnly, properties);
            Debug.Log($"[VivoxManager] Joined positional channel: {_proximityChannel}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[VivoxManager] JoinProximityChannelAsync failed: {e.Message}");
        }
    }

    /// <summary>Flat group voice channel for lobby.</summary>
    public async Task JoinGroupChannelAsync(string roomName)
    {
        if (!_loggedIn) { Debug.LogWarning("[VivoxManager] Must login first."); return; }

        _groupChannel = $"{groupChannelPrefix}-{roomName}";
        try
        {
            await VivoxService.Instance.JoinGroupChannelAsync(
                _groupChannel, ChatCapability.AudioOnly);
            Debug.Log($"[VivoxManager] Joined group channel: {_groupChannel}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[VivoxManager] JoinGroupChannelAsync failed: {e.Message}");
        }
    }

    /// <summary>
    /// Update 3-D position for positional audio. Call every frame from the local player.
    /// </summary>
    public void Set3DPosition(GameObject speaker)
    {
        if (!_loggedIn || string.IsNullOrEmpty(_proximityChannel)) return;
        VivoxService.Instance.Set3DPosition(speaker, _proximityChannel, true);
    }

    /// <summary>
    /// Spectator mode: mutes the microphone so the player can hear but not speak.
    /// Call with true when the player dies/becomes a spectator.
    /// </summary>
    public void SetSpectatorMode(bool isSpectator)
    {
        if (!_loggedIn) return;
        if (isSpectator) VivoxService.Instance.MuteInputDevice();
        else             VivoxService.Instance.UnmuteInputDevice();
        Debug.Log($"[VivoxManager] Spectator mode: {isSpectator}");
    }

    /// <summary>Mute/unmute the local microphone.</summary>
    public void SetMicrophoneMuted(bool muted)
    {
        if (!_loggedIn) return;
        if (muted) VivoxService.Instance.MuteInputDevice();
        else       VivoxService.Instance.UnmuteInputDevice();
    }

    /// <summary>Mute/unmute remote audio output.</summary>
    public void SetSpeakerMuted(bool muted)
    {
        if (!_loggedIn) return;
        if (muted) VivoxService.Instance.MuteOutputDevice();
        else       VivoxService.Instance.UnmuteOutputDevice();
    }

    public async Task LeaveAllChannelsAsync()
    {
        try { await VivoxService.Instance.LeaveAllChannelsAsync(); }
        catch (Exception e) { Debug.LogError($"[VivoxManager] LeaveAllChannelsAsync failed: {e.Message}"); }

        _proximityChannel = null;
        _groupChannel     = null;
        Debug.Log("[VivoxManager] Left all channels.");
    }

    public async Task LogoutAsync()
    {
        if (!_loggedIn) return;
        try
        {
            await VivoxService.Instance.LogoutAsync();
            _loggedIn = false;
            Debug.Log("[VivoxManager] Logged out.");
        }
        catch (Exception e) { Debug.LogError($"[VivoxManager] LogoutAsync failed: {e.Message}"); }
    }

    // ─────────────────────────────────────────────────────────────
    //  Vivox Events
    // ─────────────────────────────────────────────────────────────

    void SubscribeEvents()
    {
        VivoxService.Instance.ParticipantAddedToChannel    += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;
    }

    void UnsubscribeEvents()
    {
        if (VivoxService.Instance == null) return;
        VivoxService.Instance.ParticipantAddedToChannel    -= OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;
    }

    void OnParticipantAdded(VivoxParticipant participant)
    {
        _participants[participant.DisplayName] = participant;
        OnParticipantChanged?.Invoke(participant.DisplayName, true);
    }

    void OnParticipantRemoved(VivoxParticipant participant)
    {
        _participants.Remove(participant.DisplayName);
        OnParticipantChanged?.Invoke(participant.DisplayName, false);
    }

    // ─────────────────────────────────────────────────────────────
    //  Per-participant volume (client-side only, does not affect others)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Adjust how loud a specific player sounds to the local client.
    /// volume: -50 (silent) to 50 (loudest). 0 = default.
    /// Only affects the local listener; has no network effect.
    /// </summary>
    /// <summary>
    /// Adjust how loud a specific player sounds to the local client.
    /// volume: -50 (silent) to 50 (loudest). 0 = default.
    /// Only affects the local listener; has no network effect.
    /// </summary>
    public void SetParticipantVolume(string displayName, int volume)
    {
        if (!_participants.TryGetValue(displayName, out VivoxParticipant participant))
        {
            Debug.LogWarning($"[VivoxManager] Participant '{displayName}' not found.");
            return;
        }
        // SetLocalVolume clamps internally, but we clamp here too for clarity
        participant.SetLocalVolume(Mathf.Clamp(volume, -50, 50));
        Debug.Log($"[VivoxManager] Volume for '{displayName}' set to {volume}.");
    }

    /// <summary>
    /// Mute or unmute a specific player's voice for the local client only.
    /// </summary>
    public void SetParticipantMuted(string displayName, bool muted)
    {
        if (!_participants.TryGetValue(displayName, out VivoxParticipant participant))
        {
            Debug.LogWarning($"[VivoxManager] Participant '{displayName}' not found.");
            return;
        }
        if (muted) participant.MutePlayerLocally();
        else        participant.UnmutePlayerLocally();
        Debug.Log($"[VivoxManager] Participant '{displayName}' local mute: {muted}.");
    }
}

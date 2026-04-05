using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Plays a bell sound on every client when Ring() is called.
/// Any client or server can trigger it — the call is always routed
/// through the server so all clients stay in sync.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BellRinger : NetworkBehaviour
{
    [SerializeField] AudioClip bellClip;

    AudioSource _audio;

    void Awake() => _audio = GetComponent<AudioSource>();

    /// <summary>Call from anywhere — handles server/client routing automatically.</summary>
    public void Ring()
    {
        if (IsServer)
            RingClientRpc();
        else
            RingServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void RingServerRpc() => RingClientRpc();

    [ClientRpc]
    void RingClientRpc() => _audio.PlayOneShot(bellClip);
}

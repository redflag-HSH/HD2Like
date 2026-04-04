using UnityEngine;

/// <summary>
/// Attach alongside BellRinger so players can ring the bell
/// through the standard Interactor system.
/// </summary>
[RequireComponent(typeof(BellRinger))]
public class BellRingerInteractor : Interactor
{
    BellRinger _bell;

    void Start()
    {
        _bell = GetComponent<BellRinger>();
        _text = "종을 울린다";
    }

    public override void OnInteract(PlayingMovement m)
    {
        _bell.Ring();
    }
}

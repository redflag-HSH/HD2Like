using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

// Owner-only "You are a Survivor/Cultist" reveal, shown once when PlayerRole
// syncs the local player's role. Persists for the whole session and stays
// hidden otherwise, mirroring VoiceRosterUI's singleton + UIDocument setup.
//
// Setup: add a UIDocument to this GameObject, assign a PanelSettings asset
// and RoleRevealPanel.uxml as its Source Asset.
[RequireComponent(typeof(UIDocument))]
public class RoleRevealUI : MonoBehaviour
{
    public static RoleRevealUI Instance { get; private set; }

    [SerializeField] float displaySeconds = 4f;

    UIDocument _document;
    VisualElement _root;
    Label _titleLabel;
    Label _descLabel;
    Coroutine _hideRoutine;

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
        _titleLabel = _root.Q<Label>("role-title");
        _descLabel = _root.Q<Label>("role-desc");
        SetVisible(false);
    }

    public void ShowRole(RoleType role)
    {
        if (role == RoleType.Cultist)
        {
            _titleLabel.text = "You are a Cultist";
            _titleLabel.RemoveFromClassList("role-title-survivor");
            _titleLabel.AddToClassList("role-title-cultist");
            _descLabel.text = "Blend in. Eliminate survivors before they outnumber you.";
        }
        else
        {
            _titleLabel.text = "You are a Survivor";
            _titleLabel.RemoveFromClassList("role-title-cultist");
            _titleLabel.AddToClassList("role-title-survivor");
            _descLabel.text = "Complete tasks and survive the night.";
        }

        SetVisible(true);

        if (_hideRoutine != null) StopCoroutine(_hideRoutine);
        _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    void SetVisible(bool visible)
    {
        _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displaySeconds);
        SetVisible(false);
        _hideRoutine = null;
    }
}

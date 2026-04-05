using UnityEngine;

[RequireComponent(typeof(StateMachine))]
public class AttackerMonster : MonoBehaviour
{
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask objectLayer;
    [SerializeField] WaypointPath waypointPath;

    StateMachine _machine;

    void Awake() => _machine = GetComponent<StateMachine>();

    void Start() => _machine.ChangeState(gameObject.AddComponent<Patrol>());

    public LayerMask GetPlayerLayer() => playerLayer;
    public WaypointPath GetWaypointPath() => waypointPath;

    public bool ObjectAhead(float range, out GameObject obj)
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range, objectLayer))
        {
            obj = hit.collider.gameObject;
            return true;
        }
        obj = null;
        return false;
    }
}
using UnityEngine;

/// <summary>
/// Holds an ordered list of waypoints for patrol routes.
/// Attach this to an empty GameObject and assign child transforms as waypoints.
/// </summary>
public class WaypointPath : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;

    public int Count => waypoints.Length;

    public Transform GetWaypoint(int index) => waypoints[index % waypoints.Length];

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform a = waypoints[i];
            Transform b = waypoints[(i + 1) % waypoints.Length];
            if (a == null || b == null) continue;

            Gizmos.DrawSphere(a.position, 0.25f);
            Gizmos.DrawLine(a.position, b.position);
        }
    }
}

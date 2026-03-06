using UnityEngine;

public class EnemyPath : MonoBehaviour
{
    public Transform[] waypoints;
    public Color pathColor = Color.green;

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.color = (i == 0) ? Color.cyan : (i == waypoints.Length - 1) ? Color.red : pathColor;
            Gizmos.DrawSphere(waypoints[i].position, 0.3f);

            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
            {
                Gizmos.color = pathColor;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}

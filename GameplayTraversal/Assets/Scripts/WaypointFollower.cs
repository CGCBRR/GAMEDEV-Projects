using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    [SerializeField] GameObject[] waypoints;
    [SerializeField] float speed = 1f;

    int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        if (waypoints[currentWaypointIndex] == null) return;

        Vector3 targetPos = waypoints[currentWaypointIndex].transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawSphere(waypoints[i].transform.position, 0.2f);

            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(
                    waypoints[i].transform.position,
                    waypoints[next].transform.position
                );
        }
    }
}
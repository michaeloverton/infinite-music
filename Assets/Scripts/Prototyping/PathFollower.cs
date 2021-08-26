using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float acceptableDistance = 1f;
    private int currentWaypointIndex;
    private Transform currentWaypoint;
    public Rigidbody follower;

    // Start is called before the first frame update
    void Start()
    {
        currentWaypoint = waypoints[0];
        currentWaypointIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate() {
        if(Vector3.Distance(follower.transform.position, currentWaypoint.position) < acceptableDistance) {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            currentWaypoint = waypoints[currentWaypointIndex];
        }

        Vector3 direction = currentWaypoint.position - follower.transform.position;

        follower.AddForce(5 * follower.mass * direction * Time.fixedDeltaTime);
        // follower.MovePosition(currentWaypoint.position * Time.fixedDeltaTime);
        // follower.transform.Translate(5 * direction * Time.fixedDeltaTime);
        // Vector3 moveDistance = Vector3.MoveTowards(follower.transform.position, currentWaypoint.position, 0.5f);
        // follower.transform.Translate(moveDistance, Space.World);
        // follower.transform.LookAt(currentWaypoint, Vector3.left);
    }
}

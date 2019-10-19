// ShowGoldenPath
using UnityEngine;
using UnityEngine.AI;

public class NormativeAgent : Agent
{	
    /// <summary>
    /// Agents preferred speed
    /// </summary>
	public float prefered_speed = 1.3f;

    /// <summary>
    /// Distance from waypoint when reached
    /// </summary>
    public float sqrWaypointDistance;

    /// <summary>
    /// 
    /// </summary>
    public float ksi = 0.5f;

    /// <summary>
    /// The current A* path
    /// </summary>
    private NavMeshPath path;

    /// <summary>
    /// Rigid body reference
    /// </summary>
    private Rigidbody rb;

    /// <summary>
    /// Current waypoint index in path corners
    /// </summary>
    int waypointIndex;

    /// <summary>
    /// That actual waypoint
    /// </summary>
    Vector3 currentWaypoint;

    /// <summary>
    /// Elapsed time
    /// </summary>
    private float elapsed = 0.0f;

    /// <summary>
    /// The target transform
    /// </summary>
	private Vector3 target;



    /// <summary>
    /// Starts this instance
    /// </summary>
    void Start()
	{
		rb = GetComponent<Rigidbody> ();
    }


    /// <summary>
    /// Sets the target for the agents and computes the path
    /// </summary>
    /// <param name="target"></param>
    public override void SetTarget(Vector3 targetPosition)
    {
        
        target = targetPosition;
//        Debug.Log(transform.position);
//        Debug.Log(target);
        path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
        if (path.corners.Length > 0)
        {
            waypointIndex = 1;
            currentWaypoint = path.corners[waypointIndex];
        }
        else
        {
            waypointIndex = 0;
            currentWaypoint = target;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override Vector3 GetCurrentGoal()
    {
        return currentWaypoint;
    }


    /// <summary>
    /// Physics update
    /// </summary>
	void FixedUpdate()
	{
        elapsed += Time.deltaTime;
        if (elapsed > 1.0f)
        {
            elapsed -= 1.0f;
            NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
            if (path.corners.Length > 0)
            {
                waypointIndex = 1;
                currentWaypoint = path.corners[waypointIndex];
            }
            else
            {
                waypointIndex = 0;
                currentWaypoint = target;
            }

            // Update the way to the goal every second.
            //for (int i = 0; i < path.corners.Length - 1; i++)
            //    Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red, 1f);
        }
        else
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                // Goal driven force.
                Vector3 preferredVelocity = currentWaypoint - transform.position;
                float goalDistance = preferredVelocity.sqrMagnitude;
                preferredVelocity *= prefered_speed / Mathf.Sqrt(goalDistance);
                Vector3 goalForce = (preferredVelocity - rb.velocity) / ksi;
                rb.AddForce(goalForce, ForceMode.Force);

                //Debug.DrawLine(transform.position, currentWaypoint, Color.green, 0.02f);

                if ((currentWaypoint - transform.position).sqrMagnitude < sqrWaypointDistance)
                {
                    if (waypointIndex + 1 < path.corners.Length)
                    {
                        waypointIndex++;
                        currentWaypoint = path.corners[waypointIndex];
                    }
                    else
                    {
                        waypointIndex = 0;
                        currentWaypoint = target;
                    }
                }
            }
        }
	}
}
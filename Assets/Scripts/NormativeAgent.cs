// ShowGoldenPath
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Normative agent type
/// </summary>
public class NormativeAgent : Agent
{	
    /// <summary>
    /// Distance from waypoint when reached
    /// </summary>
    public float sqrWaypointDistance;

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
	/// The type of agent
	/// </summary>
	private string typeOfAgent = "Normative";

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
    /// Get the agent type
    /// </summary>
    /// <returns>string agent type</returns>
    public override string GetAgentType()
	{
		return typeOfAgent;
	}

    /// <summary>
    /// Get the current goals
    /// </summary>
    /// <returns>Vector3 current waypoint in world position</returns>
    public override Vector3 GetCurrentGoal()
    {
        return currentWaypoint;
    }

    /// <summary>
    /// get the current final goal
    /// </summary>
    /// <returns>Vector3 final goal world position</returns>
    public override Vector3 GetFinalGoal()
	{
		return target;
	}

    /// <summary>
    /// Physics update
    /// </summary>
	void FixedUpdate()
	{
		//Debug.DrawLine (transform.position, currentWaypoint, Color.blue);

		for (int i = waypointIndex; i < path.corners.Length - 1; i++)
			Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red, 1f);
		
		Vector3 direction = path.corners [waypointIndex] - transform.position;
		int layerMask = 1 << 8; //only check further collisions with walls, which are on layer 8
		if (Physics.Raycast (transform.position, direction, direction.magnitude, layerMask)) {
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
        else
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                if ((currentWaypoint - transform.position).sqrMagnitude < sqrWaypointDistance)
                {
                    if (waypointIndex + 1 < path.corners.Length)
                    {
						//Only update the waypoint if the agent can see it
						direction = path.corners [waypointIndex+1] - transform.position;
						if (currentWaypoint != target && !Physics.Raycast (transform.position, direction, direction.magnitude, layerMask)) {
							waypointIndex++;
							currentWaypoint = path.corners[waypointIndex];
						}
                    }
                    else
                    {
						//Only update the waypoint if the agent can see it
						direction = path.corners [path.corners.Length - 1] - transform.position;
						if (!Physics.Raycast (transform.position, direction, direction.magnitude, layerMask)) {
							waypointIndex = 0;
							currentWaypoint = target;
						}
                    }
                }
            }
        }
	}
}
// ShowGoldenPath
using UnityEngine;
using UnityEngine.AI;

public class ShowGoldenPath_Distraction : Agent
{	
    /// <summary>
    /// Agents preferred speed
    /// </summary>
	public float prefered_speed = 1.3f;

	/// <summary>
	/// Agent's speed while distracted
	/// </summary>
	public float distraction_speed = 1.0f;

	/// <summary>
	/// Agent's current speed
	/// </summary>
	private float current_speed;

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
	/// The direction the agent is currently moving
	/// </summary>
	private Vector3 direction;

	/// <summary>
	/// Whether the agent is currently distracted
	/// </summary>
	private bool is_distracted = false;

	/// <summary>
	/// The current attentiveness of the agent
	/// </summary>
	private float current_attentiveness = 1.0f;

	/// <summary>
	/// The time the agent has been distracted
	/// </summary>
	private float elapsed_time_distracted = 0.0f;

	/// <summary>
	/// The time the agent has been alert
	/// </summary>
	private float elapsed_time_attentive = 0.0f;

	/// <summary>
	/// The attentiveness level of the agent while distracted.
	/// When the agent is distracted, how distracted are they?
	/// </summary>
	public float attentiveness_level = 0.1f;

	/// <summary>
	/// The lateral deviation factor 
	/// Affects how far left or right the agent drifts while distracted.
	/// </summary>
	public float deviation_factor = 0.1f;

	/// <summary>
	/// The minimum amount of time the agent remains distracted
	/// </summary>
	public float min_distraction_time = 2.0f;

	/// <summary>
	/// The maximum amount of time the agent remains distracted
	/// </summary>
	public float max_distraction_time = 5.0f;

	/// <summary>
	/// The minimum amount of time the agent is attentive before they can become distracted
	/// </summary>
	public float min_attentive_time = 5.0f;

	/// <summary>
	/// The likelihood an agent becomes distracted each second, expressed as a percentage
	/// </summary>
	public float percent_chance_become_distracted = 50; 

	/// <summary>
	/// The likelihood an agent becomes distracted each second, expressed as a decimal between 0 and 1
	/// </summary>
	private float distractedChance;

    /// <summary>
    /// Starts this instance
    /// </summary>
    void Start()
	{
		rb = GetComponent<Rigidbody> ();
		current_speed = prefered_speed;
		distractedChance = percent_chance_become_distracted / 100;
		float randDevDir = Random.value;
		if (randDevDir <= 0.5f) { // Determines whether the agents deviates left or right while distracted (e.g. whether the agent is right-brained or left-brained)
			deviation_factor = -deviation_factor;
		}
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
		direction = (currentWaypoint - transform.position);
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
    /// Getter method to check if the agent is distracted
    /// </summary>
    public bool checkDistracted(){
		return is_distracted;
	}

	/// <summary>
	/// Make the agent pay attention
	/// </summary>
	public void PayAttention(){
		elapsed_time_distracted = 0.0f;
		is_distracted = false;
		SetTarget (target);
		current_attentiveness = 1.0f;
		current_speed = prefered_speed;
	}

	/// <summary>
	/// Make the agent become distracted
	/// </summary>
	public void BecomeDistracted(){
		is_distracted = true;
		direction = (currentWaypoint - transform.position);
		direction += new Vector3 (-direction.z * deviation_factor, 0, direction.x * deviation_factor); //lateral deviation from a straight line while distracted
		current_attentiveness = attentiveness_level;
		elapsed_time_attentive = 0.0f;
		float randNum = Random.value;
		current_speed = distraction_speed;
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
                //Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red, 1f);

			//Distraction Logic
			float randNum = Random.value;
			if (is_distracted == false && randNum <= distractedChance && elapsed_time_attentive > min_attentive_time) {
				BecomeDistracted ();
			} else if (elapsed_time_distracted >= max_distraction_time || (elapsed_time_distracted >= min_distraction_time && randNum <= 1.0f - distractedChance)) {
				PayAttention (); 
			}
        }
        else
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
				if (is_distracted == false) {
					direction = (currentWaypoint - transform.position);
				}
                // Goal driven force.
				rb.AddForce((direction.normalized * current_speed - rb.velocity) / 0.5f);

                //Debug.DrawLine(transform.position, currentWaypoint, Color.green, 0.02f);

				if (current_attentiveness == 1.0f) {
					gameObject.GetComponentInChildren<Renderer> ().material.color = Color.green;
				} else {
					gameObject.GetComponentInChildren<Renderer> ().material.color = Color.red;
				}

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

		//increment alert/distracted time each frame
		if (is_distracted == true) {
			elapsed_time_distracted += Time.deltaTime;
		} else {
			elapsed_time_attentive += Time.deltaTime;
		}


	}
}
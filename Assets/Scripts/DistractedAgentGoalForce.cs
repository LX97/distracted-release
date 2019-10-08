// ShowGoldenPath
using UnityEngine;
using UnityEngine.AI;

public class DistractedAgentGoalForce : Agent
{	
    /// <summary>
    /// Agents preferred speed
    /// </summary>
	public float prefered_speed = 1.3f;

	/// <summary>
	/// Agent's speed while distracted
	/// </summary>
	private float distractionSpeed;

	/// <summary>
	/// Agent's min speed while distracted
	/// </summary>
	private float minDistractionSpeed;

	/// <summary>
	/// Agent's max speed while distracted
	/// </summary>
	private float maxDistractionSpeed;

	/// <summary>
	/// Agent's current speed
	/// </summary>
	private float currentSpeed;

	/// <summary>
	/// 
	/// </summary>
	public float ksi = 0.5f;

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
	private bool isDistracted = false;

	/// <summary>
	/// The current attentiveness of the agent
	/// </summary>
	private float currentAttentiveness = 1.0f;

	/// <summary>
	/// The time the agent has been distracted
	/// </summary>
	private float elapsedTimeDistracted = 0.0f;

	/// <summary>
	/// The time the agent has been alert
	/// </summary>
	private float elapsedTimeAttentive = 0.0f;

	/// <summary>
	/// The attentiveness level of the agent while distracted.
	/// When the agent is distracted, how distracted are they?
	/// </summary>
	public float attentivenessLevel = 0.1f;

	/// <summary>
	/// The lateral deviation factor 
	/// Affects how far left or right the agent drifts while distracted.
	/// </summary>
	public float deviationFactor = 0.1f;

	/// <summary>
	/// The minimum amount of time the agent remains distracted
	/// </summary>
	public float minDistractionTime = 2.0f;

	/// <summary>
	/// The maximum amount of time the agent remains distracted
	/// </summary>
	public float maxDistractionTime = 5.0f;

	/// <summary>
	/// The minimum amount of time the agent is attentive before they can become distracted
	/// </summary>
	public float minAttentiveTime = 5.0f;

	/// <summary>
	/// The likelihood an agent becomes distracted each second, expressed as a percentage
	/// </summary>
	public float percentChanceBecomeDistracted = 50; 

	/// <summary>
	/// The likelihood an agent becomes distracted each second, expressed as a decimal between 0 and 1
	/// </summary>
	private float distractedChance;

	/// <summary>
	/// If this is set, distractedChance is overridden and the agent always becomes distracted
	/// The agent still pays attention for 1 second when maxDistractionTime is reached, in order to recompute the path
	/// If this is set, minAttentiveTime will be set to 0 when the simulation starts
	/// </summary>
	public bool alwaysDistracted = false;

    /// <summary>
    /// Starts this instance
    /// </summary>
    void Start()
	{
		rb = GetComponent<Rigidbody> ();
		currentSpeed = prefered_speed;
		distractedChance = percentChanceBecomeDistracted / 100;
		float randDevDir = Random.value;
		if (randDevDir <= 0.5f) { // Determines whether the agents deviates left or right while distracted (e.g. whether the agent is right-brained or left-brained)
			deviationFactor = -deviationFactor;
		}

		// According to studies, distracted pedestrians move between 5-35% slower
		minDistractionSpeed = 0.65f * prefered_speed;
		maxDistractionSpeed = 0.95f * prefered_speed;

		if (alwaysDistracted == true) {
			minAttentiveTime = 0.0f;
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
		return isDistracted;
	}

	/// <summary>
	/// Make the agent pay attention
	/// </summary>
	public void PayAttention(){
		elapsedTimeDistracted = 0.0f;
		isDistracted = false;
		SetTarget (target);
		currentAttentiveness = 1.0f;
		currentSpeed = prefered_speed;
	}

	/// <summary>
	/// Make the agent become distracted
	/// </summary>
	public void BecomeDistracted(){
		isDistracted = true;
		direction = (currentWaypoint - transform.position);
		direction += new Vector3 (-direction.z * deviationFactor, 0, direction.x * deviationFactor); //lateral deviation from a straight line while distracted
		currentAttentiveness = attentivenessLevel;
		elapsedTimeAttentive = 0.0f;
		float randNum = Random.value;
		distractionSpeed = minDistractionSpeed + ((maxDistractionSpeed - minDistractionSpeed) * attentivenessLevel); //scale speed based on level of attentiveness
		currentSpeed = distractionSpeed;
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
			if (isDistracted == false && (alwaysDistracted == true || randNum <= distractedChance && elapsedTimeAttentive > minAttentiveTime)) {
				BecomeDistracted ();
			} else if (elapsedTimeDistracted >= maxDistractionTime || (alwaysDistracted == false && elapsedTimeDistracted >= minDistractionTime && randNum <= 1.0f - distractedChance)) {
				PayAttention (); 
			}
        }
        else
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
				if (isDistracted == false) {
					direction = (currentWaypoint - transform.position);
				}
                // Goal driven force.
				float goalDistance = direction.sqrMagnitude;
				direction *= currentSpeed / Mathf.Sqrt(goalDistance);
				Vector3 goalForce = (direction - rb.velocity) / ksi;
				rb.AddForce(goalForce, ForceMode.Force);

                //Debug.DrawLine(transform.position, currentWaypoint, Color.green, 0.02f);

				if (currentAttentiveness == 1.0f) {
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
		if (isDistracted == true) {
			elapsedTimeDistracted += Time.deltaTime;
		} else {
			elapsedTimeAttentive += Time.deltaTime;
		}


	}
}
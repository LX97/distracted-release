using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;

public class DistractedAgent : Agent
{	
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
    /// That current waypoint
	/// May be actual or fuzzy
    /// </summary>
    Vector3 currentWaypoint;

	/// <summary>
	/// That actual waypoint
	/// </summary>
	Vector3 actualWaypoint;

	/// <summary>
	/// The fuzzy waypoint while distracted
	/// </summary>
	private Vector3 fuzzyWaypoint;

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
	/// The maxium radius around the current waypoint to choose a fuzzy waypoint from while distracted
	/// </summary>
	public float maxFuzzyWaypointRadius = 3.0f;

	/// <summary>
	/// The current radius around the current waypoint to choose a fuzzy waypoint from while distracted
	/// </summary>
	private float currentFuzzyWaypointRadius;

	/// <summary>
	/// If this is set, distractedChance is overridden and the agent always becomes distracted
	/// The agent still pays attention for 1 second when maxDistractionTime is reached, in order to recompute the path
	/// If this is set, minAttentiveTime will be set to 0 when the simulation starts
	/// </summary>
	public bool alwaysDistracted = false;

	/// <summary>
	/// The type of agent
	/// </summary>
	private string typeOfAgent = "Distracted";

	/// <summary>
	/// Reference to this agent's behavior tree
	/// </summary>
	private BehaviorTree behaviorTree;

	/// <summary>
	/// Reference to Behavior Tree variable isDistracted
	/// </summary>
	SharedBool isDistractedBehaviorTree;

	private IEnumerator coroutine;

    /// <summary>
    /// Starts this instance
    /// </summary>
    void Start()
	{
		rb = GetComponent<Rigidbody> ();

		behaviorTree = GetComponent<BehaviorTree> ();
		isDistractedBehaviorTree = (SharedBool) behaviorTree.GetVariable ("isDistracted");

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
	public override string GetAgentType()
	{
		return typeOfAgent;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public override Vector3 GetFinalGoal()
	{
		return target;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override Vector3 GetCurrentGoal()
    {
		// currentWaypoint may be a fuzzy waypoint
        return currentWaypoint; 
    }
	

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public void MakeWaypointFuzzy()
	{
		actualWaypoint = currentWaypoint;
		currentFuzzyWaypointRadius = Mathf.Clamp ((currentWaypoint - transform.position).magnitude * 0.1f, 0.0f, maxFuzzyWaypointRadius);
		fuzzyWaypoint = Random.insideUnitCircle * currentFuzzyWaypointRadius;
		currentWaypoint += fuzzyWaypoint;
	}


    /// <summary>
    /// Getter method to check if the agent is distracted
    /// </summary>
    public bool checkDistracted(){
		return isDistracted;
	}

	/// <summary>
	/// Getter method to check the agent's currentAttentiveness
	/// </summary>
	public float getCurrentAttentiveness(){
		return currentAttentiveness;
	}

	/// <summary>
	/// Make the agent pay attention
	/// </summary>
	public void PayAttention(){

		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 1.0f;
		agentPAMScript.SetAttentiveness (currentAttentiveness, false);
		elapsedTimeDistracted = 0.0f;
		isDistracted = false;
		SetTarget (target);
		isDistractedBehaviorTree.SetValue (isDistracted);
		behaviorTree.SetVariableValue ("makeStopAndText", false);

		transform.Find ("phoneStopTextSprite").GetComponent<SpriteRenderer> ().enabled = false;
		transform.Find ("phoneTextSprite").GetComponent<SpriteRenderer> ().enabled = false;
		transform.Find ("phoneRingSprite").GetComponent<SpriteRenderer> ().enabled = false;
		transform.Find ("phoneSurfSprite").GetComponent<SpriteRenderer> ().enabled = false;
		transform.Find ("phoneReadSprite").GetComponent<SpriteRenderer> ().enabled = false;
		transform.Find ("phoneGameSprite").GetComponent<SpriteRenderer> ().enabled = false;
	}

	/// <summary>
	/// Make the agent become distracted, fully stopping to text
	/// </summary>
	public void BecomeDistractedStopAndText(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.0f;
		agentPAMScript.SetAttentiveness (currentAttentiveness, true);
		isDistractedBehaviorTree.SetValue (isDistracted);
		transform.Find ("phoneStopTextSprite").GetComponent<SpriteRenderer> ().enabled = true;
	}

	/// <summary>
	/// Make the agent become distracted texting on their phone
	/// </summary>
	public void BecomeDistractedWalkAndText(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.2f; 
		agentPAMScript.SetAttentiveness (currentAttentiveness, true);
		MakeWaypointFuzzy ();
		isDistractedBehaviorTree.SetValue (isDistracted);
		transform.Find ("phoneTextSprite").GetComponent<SpriteRenderer> ().enabled = true;
	}

	/// <summary>
	/// Make the agent become distracted reading on their phone
	/// </summary>
	public void BecomeDistractedWalkAndRead(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.2f; 
		agentPAMScript.SetAttentiveness (currentAttentiveness, true);
		MakeWaypointFuzzy ();
		isDistractedBehaviorTree.SetValue (isDistracted);
		transform.Find ("phoneReadSprite").GetComponent<SpriteRenderer> ().enabled = true;
	}

	/// <summary>
	/// Make the agent become distracted playing a mobile game
	/// </summary>
	public void BecomeDistractedWalkAndGame(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.2f;
		agentPAMScript.SetAttentiveness (currentAttentiveness, true);
		MakeWaypointFuzzy ();
		isDistractedBehaviorTree.SetValue (isDistracted);
		transform.Find ("phoneGameSprite").GetComponent<SpriteRenderer> ().enabled = true;
	}

	/// <summary>
	/// Make the agent become distracted talking on the phone
	/// </summary>
	public void BecomeDistractedWalkAndTalk(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.2f;
		agentPAMScript.SetAttentiveness (currentAttentiveness, false);
		MakeWaypointFuzzy ();
		isDistractedBehaviorTree.SetValue (isDistracted);
		transform.Find ("phoneRingSprite").GetComponent<SpriteRenderer> ().enabled = true;
	}

	/// <summary>
	/// Make the agent become distracted browsing apps or surfing the internet
	/// </summary>
	public void BecomeDistractedWalkAndSurf(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.2f;
		agentPAMScript.SetAttentiveness (currentAttentiveness, true);
		MakeWaypointFuzzy ();
		isDistractedBehaviorTree.SetValue (isDistracted);
		transform.Find ("phoneSurfSprite").GetComponent<SpriteRenderer> ().enabled = true;
	}

	/// <summary>
	/// Make the agent become distracted on click
	/// </summary>
	public void OnMouseDown(){
		behaviorTree.SetVariableValue ("makeStopAndText", true);
	}


    /// <summary>
    /// Physics update
    /// </summary>
	void FixedUpdate()
	{
        elapsed += Time.deltaTime;
		if (elapsed > 1.0f) {
				elapsed -= 1.0f;
				NavMesh.CalculatePath (transform.position, target, NavMesh.AllAreas, path);

			if (!isDistracted) {
				if (path.corners.Length > 0) {
					waypointIndex = 1;
					currentWaypoint = path.corners [waypointIndex];
				} else {
					waypointIndex = 0;
					currentWaypoint = target;
				}
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
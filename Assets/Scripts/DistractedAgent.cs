using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;

/// <summary>
/// Distracted Agent type
/// </summary>
public class DistractedAgent : Agent
{	
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
	/// Whether the agent is currently distracted
	/// </summary>
	private bool isDistracted = false;

	/// <summary>
	/// The current attentiveness of the agent
	/// </summary>
	private float currentAttentiveness = 1.0f;

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
	/// The maxium radius around the current waypoint to choose a fuzzy waypoint from while distracted
	/// </summary>
	public float maxFuzzyWaypointRadius = 3.0f;

	/// <summary>
	/// The current radius around the current waypoint to choose a fuzzy waypoint from while distracted
	/// </summary>
	private float currentFuzzyWaypointRadius;

	/// <summary>
	/// The type of agent
	/// </summary>
	private string typeOfAgent = "Distracted";

	/// <summary>
	/// The current attentive state (activity) of the agent
	/// </summary>
	private string attentiveState = "Paying Attention";

	/// <summary>
	/// Reference to this agent's behavior tree
	/// </summary>
	private BehaviorTree behaviorTree;

	/// <summary>
	/// Reference to Behavior Tree variable isDistracted
	/// </summary>
	SharedBool isDistractedBehaviorTree;

	/// <summary>
	/// Controls whether the agent deviates left or right while distracted
	/// </summary>
	private int deviationDirection = 1;

    /// <summary>
    /// The current fuzzy waypoint
    /// </summary>
	Vector3 fuzzyPoint;

	/// <summary>
	/// Starts this instance
	/// </summary>
	void Start()
	{
		rb = GetComponent<Rigidbody> ();

		behaviorTree = GetComponent<BehaviorTree> ();
		isDistractedBehaviorTree = (SharedBool) behaviorTree.GetVariable ("isDistracted");

		//Determine if this agent has right or left hemisphere attentional bias
		float rand = Random.value;
		if (rand < 0.1f) {
			deviationDirection = -1;
		}
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
		actualWaypoint = currentWaypoint;
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
	/// get the current final goal
	/// </summary>
	/// <returns>Vector3 final goal world position</returns>
	public override Vector3 GetFinalGoal()
	{
		return target;
	}

	/// <summary>
	/// Get the current goals
	/// </summary>
	/// <returns>Vector3 current waypoint in world position</returns>
	public override Vector3 GetCurrentGoal()
	{
		// currentWaypoint may be a fuzzy waypoint
		return currentWaypoint;
    }


    /// <summary>
    /// Make a fuzzy waypoint. Initializes a random position around the actual waypoint
    /// from the A* longterm path in a safe way:
    /// 1. Choose a random point in within the circle defiend by actual waypoint and fuzzy radius
    /// 2. Now use NavMesh.SamplePosition
    ///     a. Note that this function tries to find the closest point on the navmesh that is closest to the point from(1)
    ///     b. This is *not* part of the randomness, it is to ensure that the point you just selected is navigable, not just floating in space.
    ///     c. It will return the closest point, which adds to the randomness if the point you generated in (1) was not navigable
    ///     d. If you dont get a hit at all then(1) has failed poorly, go back to(1)
    /// 3. Now do a raycast from the point you got in (2) to the original waypoint
    ///     a. If this is blocked by a wall you;ll get a wall collider hit, and you can check its tag
    ///     b. If it hits a wall that means the point from (2) is in a different area or concavity, go back to (1)
	/// </summary>
	public void MakeWaypointFuzzy()
	{
		actualWaypoint = currentWaypoint;
		currentFuzzyWaypointRadius = Mathf.Clamp ((currentWaypoint - transform.position).magnitude * deviationFactor, 0.0f, maxFuzzyWaypointRadius);
		Vector3 forward = (currentWaypoint - transform.position);
		fuzzyPoint = (Quaternion.AngleAxis (Random.Range(0.0f, 360.0f), Vector3.up) * forward.normalized * currentFuzzyWaypointRadius) + actualWaypoint;
		bool foundPosition = false;
		NavMeshHit hit;
		for (int i = 0; i < 10; i++) {
			if (NavMesh.SamplePosition (fuzzyPoint, out hit, 1.0f, NavMesh.AllAreas)) {
				Vector3 direction = transform.position - hit.position;
				int layerMask = 1 << 8; //only check further collisions with walls, which are on layer 8
				if (!Physics.Raycast(hit.position, direction, direction.magnitude, layerMask)){
					fuzzyPoint = hit.position;
					foundPosition = true;
					break;
				}
			}
		}
		if (foundPosition) {
			currentWaypoint = fuzzyPoint;
		}
	}


	/// <summary>
	/// Getter method to check if the agent is distracted
	/// </summary>
	public bool CheckDistracted(){
		return isDistracted;
	}

	/// <summary>
	/// Getter method to check the agent's currentAttentiveness
	/// </summary>
	public float GetCurrentAttentiveness(){
		return currentAttentiveness;
	}

	/// <summary>
	/// Getter method to check the agent's attentiveness state
	/// </summary>
	public string GetAttentiveState(){
		return attentiveState;
	}

	/// <summary>
	/// Make the agent pay attention
	/// </summary>
	public void PayAttention(){

		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 1.0f;
		agentPAMScript.SetAttentiveness (currentAttentiveness, false);
		isDistracted = false;
		SetTarget (target);
		isDistractedBehaviorTree.SetValue (isDistracted);
		behaviorTree.SetVariableValue ("makeStopAndText", false);
		attentiveState = "Paying Attention";
		var sprites = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < sprites.Length; i++) {
			sprites [i].enabled = false;
		}
	}

	/// <summary>
	/// Make the agent become distracted, fully stopping to text
	/// </summary>
	public void BecomeDistractedStopAndText(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.0f;
		agentPAMScript.SetAttentiveness (currentAttentiveness, true);
		MakeWaypointFuzzy ();
		isDistractedBehaviorTree.SetValue (isDistracted);
		transform.Find ("phoneStopTextSprite").GetComponent<SpriteRenderer> ().enabled = true;
		attentiveState = "StopText";
	}

	/// <summary>
	/// Make the agent become distracted texting on their phone
	/// </summary>
	public void BecomeDistractedWalkAndText(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.2f; 
		agentPAMScript.SetAttentiveness (currentAttentiveness, true);
		agentPAMScript.SetDistractionParameters (0.8f, 48.0f, 1.0f);
		deviationFactor = 0.1f;
		MakeWaypointFuzzy ();
		isDistractedBehaviorTree.SetValue (isDistracted);
		//transform.Find ("phoneTextSprite").GetComponent<SpriteRenderer> ().enabled = true;
		attentiveState = "Text";
	}

	/// <summary>
	/// Make the agent become distracted reading on their phone
	/// </summary>
	public void BecomeDistractedWalkAndRead(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.2f; 
		agentPAMScript.SetAttentiveness (currentAttentiveness, true);
		agentPAMScript.SetDistractionParameters (0.88f, 48.0f, 1.0f);
		deviationFactor = 0.1f;
		MakeWaypointFuzzy ();
		isDistractedBehaviorTree.SetValue (isDistracted);
		//transform.Find ("phoneReadSprite").GetComponent<SpriteRenderer> ().enabled = true;
		attentiveState = "Read";
	}

	/// <summary>
	/// Make the agent become distracted playing a mobile game
	/// </summary>
	public void BecomeDistractedWalkAndGame(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.2f;
		agentPAMScript.SetAttentiveness (currentAttentiveness, true);
		agentPAMScript.SetDistractionParameters (0.81f, 48.0f, 1.0f);
		deviationFactor = 0.1f;
		MakeWaypointFuzzy ();
		isDistractedBehaviorTree.SetValue (isDistracted);
		transform.Find ("phoneGameSprite").GetComponent<SpriteRenderer> ().enabled = true;
		attentiveState = "Game";
	}

	/// <summary>
	/// Make the agent become distracted talking on the phone
	/// </summary>
	public void BecomeDistractedWalkAndTalk(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.2f;
		agentPAMScript.SetAttentiveness (currentAttentiveness, false);
		agentPAMScript.SetDistractionParameters (0.82f, 186.0f, 10.0f);
		deviationFactor = 0.0f;
		isDistractedBehaviorTree.SetValue (isDistracted);
		//transform.Find ("phoneRingSprite").GetComponent<SpriteRenderer> ().enabled = true;
		attentiveState = "Talk";
	}

	/// <summary>
	/// Make the agent become distracted browsing apps or surfing the internet
	/// </summary>
	public void BecomeDistractedWalkAndSurf(){
		isDistracted = true;
		var agentPAMScript = GetComponent<AgentPredictiveAvoidanceModel> ();
		currentAttentiveness = 0.2f;
		agentPAMScript.SetAttentiveness (currentAttentiveness, true);
		agentPAMScript.SetDistractionParameters (0.75f, 48.0f, 1.0f);
		deviationFactor = 0.1f;
		MakeWaypointFuzzy ();
		isDistractedBehaviorTree.SetValue (isDistracted);
		transform.Find ("phoneSurfSprite").GetComponent<SpriteRenderer> ().enabled = true;
		attentiveState = "Surf";
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
		Vector3 direction = currentWaypoint - transform.position;
		int layerMask = 1 << 8; //only check further collisions with walls, which are on layer 8

		if (Physics.Raycast(transform.position, direction, direction.magnitude, layerMask)){
			NavMesh.CalculatePath (transform.position, target, NavMesh.AllAreas, path);
				if (path.corners.Length > 0) {
					waypointIndex = 1;
				currentWaypoint = path.corners [waypointIndex];
				} else {
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
							currentWaypoint = path.corners [waypointIndex];
							if (isDistracted) {
								actualWaypoint = currentWaypoint;
								MakeWaypointFuzzy ();
							}
						} else {
							NavMesh.CalculatePath (transform.position, target, NavMesh.AllAreas, path);
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
						//Only update the waypoint if the agent can see it
						direction = path.corners [path.corners.Length - 1] - transform.position;
						if (!Physics.Raycast (transform.position, direction, direction.magnitude, layerMask)) {
							if (actualWaypoint != target) { // normal case, head to the goal or a fuzzy goal around the goal
								currentWaypoint = target;
								actualWaypoint = target;
								if (isDistracted) {
									actualWaypoint = currentWaypoint;
									MakeWaypointFuzzy ();
								}
							} else { // we have reached a fuzzy goal around the goal, head to the actual goal with no fuzziness.
								currentWaypoint = target;
								waypointIndex = 0;
							}
						}
					}
				}
			}
		}

	}
}
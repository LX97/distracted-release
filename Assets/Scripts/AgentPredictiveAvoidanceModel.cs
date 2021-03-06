using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]

/// <summary>
/// Unity C# implementation of the Predictive Avoidance Model (PAM)
/// originally in C++ by Ioannis Karamouzas et. al.
/// </summary>
public class AgentPredictiveAvoidanceModel : MonoBehaviour
{
	/// <summary>
	/// The agents field of view
	/// </summary>
	public float fieldOfView = 200f;

	/// <summary>
	/// The agents radius (assumed homogeneous in crowd)
	/// </summary>
	public float agentRadius = 0.5f;

	/// <summary>
	/// Minimum agent distance parameter
	/// </summary>
	public float minAgentDistance = 0.1f;

	/// <summary>
	/// the mid distance parameters in peicewise personal space function predictive force dist
	/// </summary>
	public float dmid = 4.0f;

	/// <summary>
	/// KSI parameter  
	/// </summary>
	public float ksi = 0.5f;

	/// <summary>
	/// Nearest Neighbour distance
	/// </summary>
	public float neighborDist = 10.0f;

	/// <summary>
	/// Maximum neighbours to consider
	/// </summary>
	public float maxNeighbors = 3;

    /// <summary>
    /// Maximum acceleration Parameter
    /// </summary>
    public float maxAccel = 20.0f;

    /// <summary> 
    /// Maximum speed Parameter
    /// </summary>
    public float maxSpeed = 2.0f;

    /// <summary>
    /// Preferred Speed Parameter
    /// </summary>
    public float preferredSpeed = 1.3f;

	/// <summary>
	/// Agent particle Radius
	/// </summary>
	public float radius = 0.5f;

	/// <summary>
	/// Goal acquired radius
	/// </summary>
	public float goalRadius = 1.0f;

    /// <summary>
    /// Time Horizon Parameter
    /// </summary>
    public float timeHorizon = 4.0f;

	/// <summary>
	/// Agent Distance Parameter
	/// </summary>
	public float agentDistance = .1f;

    /// <summary>
    /// Wall Distance Parameter
    /// </summary>
    public float wallDistance = .1f;

    /// <summary>
    /// Wall Steepnes Parameter
    /// </summary>
    public float wallSteepness = 2.0f;

    /// <summary>
    /// Agent Strength Parameter
    /// </summary>
    public float agentStrength = 1.0f;

    /// <summary>
    /// wFactor Parameter
    /// </summary>
    public float wFactor = .8f;

	/// <summary>
	/// Noise flag (should noise be added to the movement action)
	/// </summary>
	public bool noise = false;

	/// <summary>
	/// the min distance parameters in peicewise personal space function
	/// </summary>
	private float dmin;

    /// <summary>
    /// the max distance parameters in peicewise personal space function
    /// </summary>
    private float dmax;

	/// <summary>
	/// Agent Personal space
	/// </summary>
	private float agentPersonalSpace;

	/// <summary>
	/// FOV cosine
	/// </summary>
	private float _cosFov;

	/// <summary>
	/// self reference
	/// </summary>
	private Agent agentSelf;

	/// <summary>
	/// Array of current neighbours
	/// </summary>
	private GameObject[] neighbor_agents;

	/// <summary>
	/// Rigibody reference
	/// </summary>
	private Rigidbody rb;

	/// <summary>
	/// The agent's attentiveness, always 1 unless changed when an agent becomes distracted
	/// </summary>
	private float attentiveness = 1.0f;

	/// <summary>
	/// The initial preferred speed
	/// </summary>
	private float initialPreferredSpeed;

	/// <summary>
	/// The agents initial field of view
	/// </summary>
	private float initialFieldOfView;

	/// <summary>
	/// The initial neighbour distance
	/// </summary>
	private float initialNeighbourDist;

	/// <summary>
	/// The scaling factor applied to the preferred speed
	/// </summary>
	private float preferredSpeedScale;

	/// <summary>
	/// Start instance
	/// </summary>
	void Start()
	{
		agentPersonalSpace = agentRadius + minAgentDistance;
		dmin = agentRadius + agentPersonalSpace;
		dmax = timeHorizon * maxSpeed;

		agentSelf = GetComponent<Agent>();
		rb = GetComponent<Rigidbody>();
		neighbor_agents = GameObject.FindGameObjectsWithTag("Agent");

		_cosFov = Mathf.Cos((0.5f * Mathf.PI * fieldOfView) / 180.0f);   // agent field of view: 200 degree
		initialPreferredSpeed = preferredSpeed;
		initialFieldOfView = fieldOfView;
		initialNeighbourDist = neighborDist;
	}

	/// <summary>
	/// Change the attentiveness level
	/// </summary>
	public void SetAttentiveness(float attentivenessLevel, bool lookingDown)
	{
		attentiveness = attentivenessLevel;
		//SetDistractionParameters (fieldOfView, lookingDown);

		if (attentivenessLevel == 1.0f) {
			preferredSpeed = initialPreferredSpeed;
			fieldOfView = initialFieldOfView;
			neighborDist = initialNeighbourDist;
		} else if (attentiveness == 0.0f){
			agentStrength = 0.0f;
			preferredSpeed = 0.0f;
		}

	}

	/// <summary>
	/// Set distraction parameters
	/// </summary>
	public void SetDistractionParameters(float speedPercentage, float viewingField, float neighbourDistance)
	{
		// According to studies, distracted pedestrians move between 5-35% slower
		//float minDistractionScale = 0.65f;
		//float maxDistractionScale = 0.95f;

		if (attentiveness == 1.0f) {
			//preferredSpeedScale = 1.0f;
			preferredSpeed = initialPreferredSpeed;
			fieldOfView = initialFieldOfView;
			neighborDist = initialNeighbourDist;
		} else if (attentiveness == 0.0f){
			agentStrength = 0.0f;
			preferredSpeedScale = 0.0f;
		}else {
			//agentStrength = 0.0f;
			//preferredSpeedScale = minDistractionScale + ((maxDistractionScale - minDistractionScale) * attentiveness);
			preferredSpeed = speedPercentage * preferredSpeed;
			fieldOfView = viewingField;
			neighborDist = neighbourDistance;
		}

		_cosFov = Mathf.Cos((0.5f * Mathf.PI * fieldOfView) / 180.0f); 
		//preferredSpeed = preferredSpeedScale * initialPreferredSpeed;
	}


	/// <summary>
	/// Physics update
	/// </summary>
	void FixedUpdate()
	{
		List<KeyValuePair<float, GameObject>> t_pairs = new List<KeyValuePair<float, GameObject>>();

		bool collision_ = false;
		int count = 0;

		Vector3 preferredVelocity = agentSelf.GetCurrentGoal () - transform.position;

		float goalDistance = preferredVelocity.sqrMagnitude;
		preferredVelocity *= preferredSpeed / Mathf.Sqrt(goalDistance);

		// Goal Driven Force (Always added)
		Vector3 goalForce = (preferredVelocity - rb.velocity) / ksi;
		rb.AddForce(goalForce, ForceMode.Force);

		Vector3 drivingForce = Vector3.zero;

		Vector3 idealDrivingForce = (preferredVelocity - rb.velocity) / ksi;

		Vector3 desiredVelocity = rb.velocity + idealDrivingForce * Time.fixedDeltaTime; ///0.02 is the fixed update physics timestep
		float desiredSpeed = desiredVelocity.magnitude;

		for (int i = 0; i < neighbor_agents.Length; ++i) {
			GameObject agent = neighbor_agents [i];
			if (agent != null) {
				if ((agent.transform.position - transform.position).magnitude < neighborDist) {
					AgentPredictiveAvoidanceModel otherAgent = agent.GetComponent<AgentPredictiveAvoidanceModel> ();
					Rigidbody otherAgentRB = agent.GetComponent<Rigidbody> ();
					Transform otherAgentTransform = agent.GetComponent<Transform> ();


					// ignore own tag and far distance agent
					if (this.GetInstanceID () == agent.GetInstanceID ())
						continue;

					float combinedRadius = agentPersonalSpace + otherAgent.agentRadius;

					Vector3 w = otherAgentTransform.position - transform.position;
					if (w.sqrMagnitude < combinedRadius * combinedRadius) {
						collision_ = true;
						t_pairs.Add (new KeyValuePair<float, GameObject> (0.0f, agent));
					} else {
						Vector3 relDir = w.normalized;
						if (Vector3.Dot (relDir, rb.velocity.normalized) < _cosFov)
							continue;

						float tc = rayIntersectsDisc (transform.position, otherAgentTransform.position, desiredVelocity - otherAgentRB.velocity, combinedRadius);
						if (tc < timeHorizon) {
							if (t_pairs.Count < maxNeighbors) {
								t_pairs.Add (new KeyValuePair<float, GameObject> (tc, agent));
							} else if (tc < t_pairs.ToArray () [0].Key) {
								t_pairs.RemoveAt (t_pairs.Count - 1);
								t_pairs.Add (new KeyValuePair<float, GameObject> (tc, agent));
							} //What to do if max neighbours is reached THIS NEED TO BE IMPLEMENTED
						}
					}
				}

			}
		}

		//Debug.Log ("Adding Collision Force");
		for(int i = 0; i < t_pairs.Count; ++i)
		{
			float t_ = t_pairs[i].Key;
			GameObject agent = t_pairs[i].Value;

			Vector3 forceDirection = transform.position + desiredVelocity * t_ - agent.transform.position - agent.GetComponent<Rigidbody>().velocity * t_;
			float forceDistance = forceDirection.magnitude;
			if (forceDistance > 0f)
				forceDirection /= forceDistance;

			float collisionDistance = Mathf.Max(forceDistance - agentRadius - agent.GetComponent<AgentPredictiveAvoidanceModel>().agentRadius, .0f); // distance between their cylindrical bodies at the time of collision
			float D = Mathf.Max(desiredSpeed * t_ + collisionDistance, 0.001f); // D = input to evasive force magnitude piecewise function

			float forceMagnitude;
			if (D < dmin)
			{
				forceMagnitude = agentStrength * dmin / D;
			}
			else if (D < dmid)
			{
				forceMagnitude = agentStrength;
			}
			else if (D < dmax)
			{
				forceMagnitude = agentStrength * (dmax - D) / (dmax - dmid);
			}
			else
			{
				forceMagnitude = 0f;
				continue;
			}
			forceMagnitude *=  Mathf.Pow((collision_ ? 1.0f : wFactor), count++);
			drivingForce += forceMagnitude * forceDirection;
		}

		// Add noise for reducing deadlocks adding naturalness
		if (noise)
		{
			float angle = Random.value * 2.0f * Mathf.PI;
			float dist = Random.value * 0.001f;
			drivingForce += dist * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
		}

		//Clamp the force to within a reasonable range
		drivingForce = Vector3.ClampMagnitude (drivingForce, 40.0f);
		rb.AddForce(drivingForce, ForceMode.Force);
	}



	/// <summary>
	/// Ray disc intersection algorithm
	/// </summary>
	/// <param name="Pa"></param>
	/// <param name="Pb"></param>
	/// <param name="v"></param>
	/// <param name="radius"></param>
	/// <returns></returns>
	float rayIntersectsDisc(Vector3 Pa, Vector3 Pb, Vector3 v, float radius)
	{
		float t;
		Vector3 w = Pb - Pa;
		float a = ((v.x * v.x) + (v.y * v.y) + (v.z * v.z));//v*v;
		float b = ((w.x * v.x) + (w.y * v.y) + (w.z * v.z));//w*v;
		float c = ((w.x * w.x) + (w.y * w.y) + (w.z * w.z)) - (radius * radius);//w*w - radius*radius;
		float discr = b * b - a * c;
		if (discr > 0.0f)
		{
			t = (b - Mathf.Sqrt(discr)) / a;
			if (t< 0)
				t = 999999.0f;
		}
		else
			t = 999999.0f;

		return t;
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]
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
    /// 
    /// </summary>
    public float minAgentDistance = 0.1f;

    /// <summary>
    /// the mid distance parameters in peicewise personal space fnction predictive force dist
    /// </summary>
    public float dmid = 4.0f;

    /// <summary>
    /// 
    /// </summary>
    public float ksi = 0.5f;

    /// <summary>
    /// 
    /// </summary>
    public float neighborDist = 10.0f;

    /// <summary>
    /// 
    /// </summary>
    public float maxNeighbors = 3;

    /// <summary>
    /// 
    /// </summary>
    public float maxAccel = 20.0f;

    /// <summary>
    /// 
    /// </summary>
    public float maxSpeed = 2.0f;

    /// <summary>
    /// 
    /// </summary>
    public float preferredSpeed = 1.3f;

    /// <summary>
    /// 
    /// </summary>
    public float radius = 0.5f;

    /// <summary>
    /// 
    /// </summary>
    public float goalRadius = 1.0f;

    /// <summary>
    /// 
    /// </summary>
    public float timeHorizon = 4.0f;

    /// <summary>
    /// 
    /// </summary>
    public float agentDistance = .1f;

    /// <summary>
    /// 
    /// </summary>
    public float wallDistance = .1f;

    /// <summary>
    /// 
    /// </summary>
    public float wallSteepness = 2.0f;

    /// <summary>
    /// 
    /// </summary>
    public float agentStrength = 1.0f;

    /// <summary>
    /// 
    /// </summary>
    public float wFactor = .8f;

    /// <summary>
    /// 
    /// </summary>
    public bool noise = false;

    /// <summary>
    /// the min distance parameters in peicewise personal space fnction
    /// </summary>
    private float dmin;

    /// <summary>
    /// 
    /// </summary>
    private float dmax;

    /// <summary>
    /// Agent Personal space
    /// </summary>
    private float agentPersonalSpace;

    /// <summary>
    /// 
    /// </summary>
    private float _cosFov;

    /// <summary>
    /// 
    /// </summary>
    private Agent agentSelf;

    /// <summary>
    /// 
    /// </summary>
    private GameObject[] neighbor_agents;

    /// <summary>
    /// 
    /// </summary>
    private Rigidbody rb;
    


    /// <summary>
    /// Start insstance
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
    }
    


    /// <summary>
    /// Physics update
    /// </summary>
    void FixedUpdate()
    {
        List<KeyValuePair<float, GameObject>> t_pairs = new List<KeyValuePair<float, GameObject>>();

        bool collision_ = false;
        int count = 0;

        Vector3 preferredVelocity = agentSelf.GetCurrentGoal() - transform.position;
        float goalDistance = preferredVelocity.sqrMagnitude;
        preferredVelocity *= preferredSpeed / Mathf.Sqrt(goalDistance);

        // Goal Driven Force (Always added)
        Vector3 goalForce = (preferredVelocity - rb.velocity) / ksi;
        rb.AddForce(goalForce, ForceMode.Force);


        Vector3 drivingForce = Vector3.zero;

       Vector3 idealDrivingForce = (preferredVelocity - rb.velocity) / ksi;

        Vector3 desiredVelocity = rb.velocity + idealDrivingForce * Time.fixedDeltaTime; ///0.02 is the fixed update physics timestep
        float desiredSpeed = desiredVelocity.magnitude;

        for (int i = 0; i < neighbor_agents.Length; ++i)
        {
            GameObject agent = neighbor_agents[i];
			if (agent != null) {
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

            //Debug.DrawLine(transform.position, transform.position + drivingForce, Color.green, 0.02f);

            /// MELISSA, See HERE
            //ShowGoldenPath_Distraction distractionScript = GetComponent<ShowGoldenPath_Distraction>();
            //if (distractionScript == null || distractionScript.checkDistracted() == false){
            //	rb.AddForce(1000f*mag/2f * force_);
            //}        
        }

        // Add noise ofr deadlocks
        if (noise)
        {
            float angle = Random.value * 2.0f * Mathf.PI;
            float dist = Random.value * 0.001f;
            drivingForce += dist * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));

            // For debugging, only randomForce is redundant here
            //Vector3 randomForce = dist * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
            //Debug.DrawLine(transform.position, transform.position + randomForce, Color.blue, 0.02f);
        }

        //Debug.DrawLine(transform.position, transform.position + drivingForce, Color.red, 0.02f);

		//Clamp the force to within a reasonable range
		drivingForce = Vector3.ClampMagnitude (drivingForce, 90.0f);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reach and destroy tool for getting rid of finished agents
/// </summary>
public class ReachAndDestroy : MonoBehaviour {
	/// <summary>
	/// The reached destination distance.
	/// </summary>
	public float reachedDestinationDistanceSquared;

	/// <summary>
	/// The number of agents that have reached the goal
	/// </summary>
	private int countAgentsDestroyed = 0;

	/// <summary>
	/// The sum of all the distances travelled by agents who have reached this goal
	/// </summary>
	private float sumAgentDistance = 0.0f;

	/// <summary>
	/// The sum of all the average kinetic energies of agents who have reached this goal
	/// </summary>
	private float sumAgentAvgKineticEnergy = 0.0f;

	/// <summary>
	/// The sum of all the pleEnergies of agents who have reached this goal
	/// </summary>
	private float sumAgentEffort = 0.0f;

	/// <summary>
	/// The agent
	/// </summary>
	private UnityEngine.AI.NavMeshAgent agent;   

	/// <summary>
	/// Reference to a distance traveled script object
	/// </summary>
	private CalculateAgentMetrics agentMetricsScript;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
    {
		//agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
	}

	/// <summary>
	/// Update when an agent enters collider.
	/// </summary>
	void OnTriggerEnter(Collider collision)
	{
		
		if (collision.gameObject.tag == "Agent") {
			countAgentsDestroyed += 1;
			agentMetricsScript = collision.gameObject.GetComponent<CalculateAgentMetrics> ();
			if (agentMetricsScript != null) {
				sumAgentDistance += agentMetricsScript.GetDistanceTravelled ();
				float agentAvgMomentum = agentMetricsScript.GetDistanceTravelled () / agentMetricsScript.GetTimeEnabled ();
				float agentAvgKineticEnergy = 0.5f * agentMetricsScript.GetIntegralOfKineticEnergy () / agentMetricsScript.GetTimeEnabled ();
				sumAgentAvgKineticEnergy += agentAvgKineticEnergy;
				sumAgentEffort += agentMetricsScript.GetEffort ();
			}

			//collision.gameObject.GetComponent<AgentPredictiveAvoidanceModel> ().enabled = false;
			//collision.gameObject.GetComponentInChildren<CapsuleCollider> ().enabled = false;
			//collision.gameObject.GetComponentInChildren<MeshRenderer> ().enabled = false;
			//GameObject.Destroy (collision.gameObject);
		}

	}

	/// <summary>
	/// Getter method to retrieve the number of agents who have reached this goal
	/// </summary>
	public int GetNumAgentsReachedGoal()
    {
		return countAgentsDestroyed;
	}

	/// <summary>
	/// Getter method to retrieve the distance travelled by all agents who have reached this goal
	/// </summary>
	public float GetTotalAgentDistance()
    {
		return sumAgentDistance;
	}

	/// <summary>
	/// Getter method to retrieve the sum of kinetic energy of all agents who have reached this goal
	/// </summary>
	public float GetSumAgentAvgKineticEnergy()
    {
		return sumAgentAvgKineticEnergy;
	}

	/// <summary>
	/// Getter method to retrieve the sum of pleEnergies of all agents who have reached this goal
	/// </summary>
	public float GetSumAgentEffort()
    {
		return sumAgentEffort;
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
    {
		//if(Vector3.SqrMagnitude(transform.position - agent.destination) < reachedDestinationDistanceSquared)
		//	GameObject.Destroy(transform.gameObject);
	}
}

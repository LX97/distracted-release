using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	/// The number of agents that the simulation started with
	/// </summary>
	public int  initialNumberOfAgents;

	/// <summary>
	/// The time for all agents to reach the goal
	/// </summary>
	public float simulationCompletionTime;

	/// <summary>
	/// The flowRate calculated at the end of the simulation
	/// </summary>
	public float flowRate;

	/// <summary>
	/// The agent.
	/// </summary>
	private UnityEngine.AI.NavMeshAgent agent;   


	GameObject[] agents;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () {
		//agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();

		agents = GameObject.FindGameObjectsWithTag("Agent");
		initialNumberOfAgents = agents.Length;
	}

	/// <summary>
	/// Update when an agent enters collider.
	/// </summary>
	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Agent") {
			GameObject.Destroy (collision.gameObject);
			countAgentsDestroyed += 1;
		}

		if (countAgentsDestroyed == initialNumberOfAgents) {
			simulationCompletionTime = Time.fixedUnscaledTime;
			flowRate = initialNumberOfAgents / simulationCompletionTime;
		}
	}

	public int GetNumAgentsReachedGoal(){
		return countAgentsDestroyed;
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		//if(Vector3.SqrMagnitude(transform.position - agent.destination) < reachedDestinationDistanceSquared)
		//	GameObject.Destroy(transform.gameObject);
	}
}

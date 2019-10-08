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
	public int countAgentsDestroyed = 0;

	/// <summary>
	/// The number of agents that the simulation started with
	/// </summary>
	private int  initialNumberOfAgents;

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

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Agent") {
			GameObject.Destroy (collision.gameObject);
			countAgentsDestroyed += 1;
		}

		if (countAgentsDestroyed == initialNumberOfAgents) {
			Debug.Log (Time.fixedUnscaledTime);
			float flowRate = initialNumberOfAgents / Time.fixedUnscaledTime;
		}
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		//if(Vector3.SqrMagnitude(transform.position - agent.destination) < reachedDestinationDistanceSquared)
		//	GameObject.Destroy(transform.gameObject);
	}
}

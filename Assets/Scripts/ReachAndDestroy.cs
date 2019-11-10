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
	/// The sum of all the distances travelled by agents who have reached this goal
	/// </summary>
	private float sumAgentDistance = 0.0f;

	/// <summary>
	/// The agent
	/// </summary>
	private UnityEngine.AI.NavMeshAgent agent;   

	/// <summary>
	/// Reference to a distance traveled script object
	/// </summary>
	private CalculateDistanceTraveled distanceTraveledScript;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () {
		//agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
	}

	/// <summary>
	/// Update when an agent enters collider.
	/// </summary>
	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Agent") {
			countAgentsDestroyed += 1;
			distanceTraveledScript = collision.gameObject.GetComponent<CalculateDistanceTraveled> ();
			if (distanceTraveledScript != null) {
				sumAgentDistance += distanceTraveledScript.GetDistanceTravelled ();
			}
			GameObject.Destroy (collision.gameObject);
		}

	}

	/// <summary>
	/// Getter method to retrieve the number of agents who have reached this goal
	/// </summary>
	public int GetNumAgentsReachedGoal(){
		return countAgentsDestroyed;
	}

	/// <summary>
	/// Getter method to retrieve the distance travelled by all agents who have reached this goal
	/// </summary>
	public float GetTotalAgentDistance(){
		return sumAgentDistance;
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		//if(Vector3.SqrMagnitude(transform.position - agent.destination) < reachedDestinationDistanceSquared)
		//	GameObject.Destroy(transform.gameObject);
	}
}

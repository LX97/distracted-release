using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachAndDestroy : MonoBehaviour {
	/// <summary>
	/// The reached destination distance.
	/// </summary>
	public float reachedDestinationDistanceSquared;


	/// <summary>
	/// The agent.
	/// </summary>
	private UnityEngine.AI.NavMeshAgent agent;   


	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () {
		agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
	}


	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		if(Vector3.SqrMagnitude(transform.position - agent.destination) < reachedDestinationDistanceSquared)
			GameObject.Destroy(transform.gameObject);
	}
}

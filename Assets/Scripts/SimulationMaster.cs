using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationMaster : MonoBehaviour {

	/// <summary>
	/// All agents initially in the scene
	/// </summary>
	public GameObject[] agents;

	/// <summary>
	/// All goals
	/// </summary>
	private GameObject[] goals;

	/// <summary>
	/// The initial number of agents in the scene
	/// </summary>
	private int initialNumberOfAgents;

	/// <summary>
	/// The number of agents that have reached their goal
	/// </summary>
	private int sumAgentsReachedGoal = 0;

	/// <summary>
	/// The reach and destroy script
	/// </summary>
	private ReachAndDestroy reachAndDestroyScript;

	/// <summary>
	/// Whether the simulation is completed
	/// </summary>
	private bool simulationFinished = false;

	/// <summary>
	/// The time it took for the simulation to complete
	/// </summary>
	private float simulationCompletionTime;

	/// <summary>
	/// The flow rate
	/// </summary>
	private float flowRate;

	/// <summary>
	/// The average agent path length
	/// </summary>
	private float avgAgentPathLength;

	void Start(){
		agents = GameObject.FindGameObjectsWithTag("Agent");
		goals = GameObject.FindGameObjectsWithTag("Goal");
		initialNumberOfAgents = agents.Length;
	}

	public bool CheckSimulationComplete(){
		return simulationFinished;
	}

	public float GetSimulationCompletionTime(){
		return simulationCompletionTime;
	}

	public float GetFlowRate(){
		return flowRate;
	}

	public float GetAvgPathLength(){
		return avgAgentPathLength;
	}

	public int GetNumInitialAgents(){
		return initialNumberOfAgents;
	}

	public int GetNumAgentsReachedGoal(){
		return sumAgentsReachedGoal;
	}

	void Update(){
		
		if (simulationFinished == false) {
			sumAgentsReachedGoal = 0;
			for (int i = 0; i < goals.Length; i++) {
				reachAndDestroyScript = goals [i].GetComponent<ReachAndDestroy> ();
				sumAgentsReachedGoal += reachAndDestroyScript.GetNumAgentsReachedGoal ();
			}

			if (sumAgentsReachedGoal == initialNumberOfAgents) {
				simulationFinished = true;
				simulationCompletionTime = Time.fixedUnscaledTime;
				flowRate = initialNumberOfAgents / simulationCompletionTime;
				float sumAgentPathLength = 0.0f;
				for (int i = 0; i < goals.Length; i++) {
					reachAndDestroyScript = goals [i].GetComponent<ReachAndDestroy> ();
					sumAgentPathLength += reachAndDestroyScript.GetTotalAgentDistance ();
				}
				avgAgentPathLength = sumAgentPathLength / initialNumberOfAgents;
			}
		}
			
	}
}


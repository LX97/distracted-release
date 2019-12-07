using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTargetOnLoad : MonoBehaviour {

	// This script sets the agent's target on load. If enabled it will allow the target of an agent to be specified before runtime,
	// so that it can be set from editor scripts or through the inspector before playing the scene

	/// <summary>
	/// The target transform to set
	/// </summary>
	public Vector3 target;

	// Use this for initialization
	void Start () {
		var agentAI = this.transform.GetComponent<Agent> ();

		if (agentAI.GetAgentType() == "Normative") {
			NormativeAgent normativeAgentScript = GetComponent<NormativeAgent> ();
			normativeAgentScript.SetTarget (target);
		} else if (agentAI.GetAgentType() == "Distracted") {
			DistractedAgent distractedAgentScript = GetComponent<DistractedAgent> ();
			distractedAgentScript.SetTarget (target);
		}
	}

}

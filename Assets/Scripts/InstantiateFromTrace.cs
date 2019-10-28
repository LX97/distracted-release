using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateFromTrace : MonoBehaviour {

	/// <summary>
	/// Toggle whether the TraceReader is active or not
	/// </summary>
	public bool enabled = false;

	/// <summary>
	/// The csv file to be read. These are found in Assets/Traces/
	/// </summary>
	public TextAsset file;

	/// <summary>
	/// The normative agent prefab
	/// </summary>
	public Transform normativeAgentPrefab;

	/// <summary>
	/// The distracted agent prefab
	/// </summary>
	public Transform distractedAgentPrefab;

	List<Transform> agentList;
	private string[] lines;
	int count = 0;

	// Use this for initialization
	void Awake () {
		if (enabled) {
			lines = file.text.Split ("\n" [0]);
			for (int i = 0; i < lines.Length; i++) {

				//Splice the data on each line
				string[] data = lines [i].Split ("," [0]);

				// If Time == 0, instantiate the agent
				if (data [0] == "0") {
					Vector3 position = new Vector3 (float.Parse (data [2]), float.Parse (data [3]), float.Parse (data [4]));
					Vector3 goal = new Vector3 (float.Parse (data [5]), float.Parse (data [6]), float.Parse (data [7]));
					Transform agent;

					if (data [1] == "Distracted") {
						agent = Instantiate (distractedAgentPrefab, position, Quaternion.identity) as Transform;
					} else {
						agent = Instantiate (normativeAgentPrefab, position, Quaternion.identity) as Transform;
					}
					agent.transform.parent = this.transform;

					var agentAI = agent.transform.GetComponent<Agent> ();
					agentAI.SetTarget (goal);
				} else { //We are past Time 0
					break;
				}

			}
		}

	}
}

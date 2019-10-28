using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class TraceReader : MonoBehaviour {

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
	private int count = 0;
	private int numFrames = 0;
	private int currentFrame = 0;
	private int lineAgentDataBegins = 1;

	[SerializeField]
	private Material attentiveMat;
	[SerializeField]
	private Material distractedMat;

	// Use this for initialization
	void Awake () {
		if (enabled) {
			agentList = new List<Transform> ();
			lines = file.text.Split ("\n" [0]);
			//string[] lastLineData = lines [lines.Length - 1].Split ("," [0]);
			//SceneManager.LoadScene(lines[0]);

			//Debug.Log (lines [lines.Length - 1]);
			//numFrames = int.Parse(lastLineData [0]);

			for (int i = lineAgentDataBegins; i < lines.Length; i++) {

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
					agentList.Add (agent);
					agent.transform.parent = this.transform;
					foreach (MonoBehaviour script in agent.transform.GetComponents<MonoBehaviour>()) {
						script.enabled = false;
					}
				} else { //We are past Time 0
					break;
				}

			}
		}

	}

	
	// Update is called once per frame
	void FixedUpdate () {
		if (enabled) {
			count++;
			currentFrame++;
			int agentCount = 0;

			// Jump to the line where we left off
			for (int i = (currentFrame * agentList.Count) + lineAgentDataBegins; i < ((currentFrame + 1) * agentList.Count) + lineAgentDataBegins && i < lines.Length; i++) {
					
				string[] data = lines [i].Split ("," [0]);
				if (data.Length > 1) {
					Vector3 position = new Vector3 (float.Parse (data [2]), float.Parse (data [3]), float.Parse (data [4]));
					float attentiveness = float.Parse (data [8]);
					if (data [1] == "Distracted" && attentiveness < 1.0f) {
						agentList [agentCount].Find ("Graphics").GetComponent<Renderer> ().material = distractedMat;
					} else if (data [1] == "Distracted") { // and attentiveness is 1.0f
						agentList [agentCount].Find ("Graphics").GetComponent<Renderer> ().material = attentiveMat;
					}
					agentList [agentCount].transform.position = position;
				}
				agentCount++;
			}

			//Debug.Log (count);
		}
	}
}

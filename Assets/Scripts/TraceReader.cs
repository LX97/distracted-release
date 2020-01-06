using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor;

/// <summary>
/// Agent trace reader tool
/// </summary>
public class TraceReader : MonoBehaviour {

	/// <summary>
	/// The csv file to be read. These are found in Assets/Traces/
	/// </summary>
	public TextAsset file;

	/// <summary>
	/// The normative agent prefab
	/// </summary>
	private GameObject normativeAgentPrefab;

	/// <summary>
	/// The distracted agent prefab
	/// </summary>
	private GameObject distractedAgentPrefab;

    /// <summary>
    /// Agent transform list
    /// </summary>
	List<Transform> agentList;

    /// <summary>
    /// Lines list
    /// </summary>
	private string[] lines;

    /// <summary>
    /// Frame count
    /// </summary>
	private int count = 0;

    /// <summary>
    /// Number of frames
    /// </summary>
	private int numFrames = 0;

    /// <summary>
    /// Current frame id
    /// </summary>
	private int currentFrame = 0;

    /// <summary>
    /// Skip header value
    /// </summary>
	private int lineAgentDataBegins = 2;

    /// <summary>
    /// Initial number of agents
    /// </summary>
	private int initialNumberOfAgents;

    /// <summary>
    /// Is enabled?
    /// </summary>
	private bool enabled = true;

	[SerializeField]
	private Material attentiveMat;
	[SerializeField]
	private Material distractedMat;

	/// <summary>
    /// Start this instance
    /// </summary>
	void Start () {

		var crowdGenerators = GameObject.FindGameObjectsWithTag("Runtime Crowd Generator");
		for (int i = 0; i < crowdGenerators.Length; i++) {
			crowdGenerators [i].SetActive(false); // Deactivate any runtime crowd generators in the scene
		}
		var agents = GameObject.FindGameObjectsWithTag("Agent");
		for (int i = 0; i < agents.Length; i++) {
			agents [i].SetActive(false); // Deactivate any agents placed in the scene before runtime
		}
		normativeAgentPrefab = (GameObject) AssetDatabase.LoadAssetAtPath<Object>("Assets/Prefabs/NormativeAgent.prefab");
		distractedAgentPrefab = (GameObject) AssetDatabase.LoadAssetAtPath<Object>("Assets/Prefabs/DistractedAgent.prefab");
		agentList = new List<Transform> ();
		lines = file.text.Split ("\n" [0]);
		if (lines [0].Trim() == SceneManager.GetActiveScene ().name.Trim()) {
			initialNumberOfAgents = int.Parse (lines [1]);
			for (int i = lineAgentDataBegins; i < lineAgentDataBegins + initialNumberOfAgents; i++) {

				//Splice the data on each line
				string[] data = lines [i].Split ("," [0]);

				// If Time == 0, instantiate the agent
				if (data [0] == "0") {
					Vector3 position = new Vector3 (float.Parse (data [2]), float.Parse (data [3]), float.Parse (data [4]));
					Vector3 goal = new Vector3 (float.Parse (data [5]), float.Parse (data [6]), float.Parse (data [7]));
					GameObject agent;

					if (data [1] == "Distracted") {
						agent = Instantiate (distractedAgentPrefab, position, Quaternion.identity) as GameObject;
					} else {
						agent = Instantiate (normativeAgentPrefab, position, Quaternion.identity) as GameObject;
					}
					agentList.Add (agent.transform);
					agent.transform.parent = this.transform;
					foreach (MonoBehaviour script in agent.transform.GetComponents<MonoBehaviour>()) {
						script.enabled = false;
					}
				}
			}
		} else { // Scene name doesn't match scene stored in trace file
			enabled = false;
			Debug.Log ("Scene name doesn't match scene stored in trace file");
		}
	}
	
	/// <summary>
    /// Physics update
    /// </summary>
	void FixedUpdate () {
		if (enabled) {
			count++;
			currentFrame++;
			int agentCount = 0;

			// Jump to the line where we left off
			for (int i = (currentFrame * initialNumberOfAgents) + lineAgentDataBegins; i < ((currentFrame + 1) * initialNumberOfAgents) + lineAgentDataBegins && i < lines.Length; i++) {
					
				string[] data = lines [i].Split ("," [0]);
				if (data.Length > 1) {
					Vector3 position = new Vector3 (float.Parse (data [2]), float.Parse (data [3]), float.Parse (data [4]));
					float attentiveness = float.Parse (data [8]);
					if (data [1] == "Distracted" && attentiveness < 1.0f) {
						agentList [agentCount].Find ("Graphics").GetComponent<Renderer> ().material = distractedMat;
						Debug.Log ("distracted");
						var sprite = agentList [agentCount].Find ("phoneStopTextSprite").GetComponent<SpriteRenderer> ();
						sprite.enabled = true;
					} else if (data [1] == "Distracted") { // and attentiveness is 1.0f
						agentList [agentCount].Find ("Graphics").GetComponent<Renderer> ().material = attentiveMat;
					}
					agentList [agentCount].transform.position = position;
				}
				agentCount++;
			}

		}
	}
}

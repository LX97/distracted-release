using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System.Collections;

class SimulationSettingsWindow : EditorWindow {

	/// <summary>
	/// The normative agent prefab.
	/// </summary>
	private GameObject normativeAgentPrefab;

	/// <summary>
	/// The distracted agent prefab.
	/// </summary>
	private GameObject distractedAgentPrefab;

	/// <summary>
	/// The floor. This can be an empty game object, but it must be axis aligned
	/// The script simply uses the local scale and position to spawn agents in an axis aligned rectangle, or AABB
	/// </summary>
	public Transform floor;

	/// <summary>
	/// The target.
	/// </summary>
	public Transform target;

	/// <summary>
	/// The edge buffer.  Agents will spawn within an
	/// area the size of the floor, minus this buffer size on all sides
	/// </summary>
	public float edgeBuffer;

	/// <summary>
	/// The csv file to be read. These are found in Assets/Traces/
	/// </summary>
	public TextAsset file;

	/// <summary>
	/// Toggles whether traces should be loaded and read
	/// </summary>
	public bool boolReadTraceFile;

	/// <summary>
	/// Stores the lines of the trace file content, if there is one specified
	/// </summary>
	private string[] lines;

	/// <summary>
	/// The line in the trace file where the agent data starts
	/// </summary>
	private int lineAgentDataBegins = 2;

	/// <summary>
	/// How many normative agents to generate when using the random generator
	/// </summary>
	private int numNormativeAgents = 0;

	/// <summary>
	/// How many distracted agents to generate when using the random generator
	/// </summary>
	private int numDistractedAgents = 0;

	/// <summary>
	/// Whether to show crowd generator options in the window
	/// </summary>
	private bool showCrowdGenerator = true;

	/// <summary>
	/// Whether to show trace file options in the window
	/// </summary>
	private bool showTraceOptions = true;

	/// <summary>
	/// An empty game object to parent generated agents to
	/// </summary>
	private GameObject editorCrowdsObj;

	[MenuItem ("Tools/Simulation Settings")]

	public static void  ShowWindow () {
		EditorWindow.GetWindow(typeof(SimulationSettingsWindow));
	}

	void Awake(){
		normativeAgentPrefab = (GameObject) AssetDatabase.LoadAssetAtPath<Object>("Assets/Prefabs/NormativeAgent.prefab");
		distractedAgentPrefab = (GameObject) AssetDatabase.LoadAssetAtPath<Object>("Assets/Prefabs/DistractedAgent.prefab");
		editorCrowdsObj = GameObject.FindGameObjectWithTag ("Editor Crowd Generator");
	}

	void OnGUI () {

		GUILayout.Label("Global Simulation Settings", EditorStyles.boldLabel);
		EditorGUILayout.Space ();
		showCrowdGenerator = EditorGUILayout.Foldout(showCrowdGenerator, "Random Crowd Generator");
		if (showCrowdGenerator) {
			floor = (Transform)EditorGUILayout.ObjectField ("Floor", floor, typeof(Transform));
			target = (Transform)EditorGUILayout.ObjectField ("Goal", target, typeof(Transform));
			edgeBuffer = EditorGUILayout.FloatField ("Edgebuffer", edgeBuffer);
			numNormativeAgents = EditorGUILayout.IntSlider ("Normative Agents", numNormativeAgents, 0, 1000);
			numDistractedAgents = EditorGUILayout.IntSlider ("Distracted Agents", numDistractedAgents, 0, 1000);

			if (GUILayout.Button ("Create Agents")) {
				GenerateRandomAgents ();
			}

		}

		EditorGUILayout.Space ();
		if (GUILayout.Button ("Delete All Agents In Scene")) {
			GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
			for (int i = 0; i < agents.Length; i++) {
				DestroyImmediate (agents [i]);
			}
		}

		EditorGUILayout.Space ();
		showTraceOptions = EditorGUILayout.Foldout(showTraceOptions, "Trace Reader Options");
		if (showTraceOptions) {
			EditorGUILayout.LabelField ("To playback trace file, check the box.");
			EditorGUILayout.LabelField("To instantiate agents from the trace file, press the button (initialize only).");
			file = (TextAsset)EditorGUILayout.ObjectField ("Trace File", file, typeof(TextAsset));
			boolReadTraceFile = EditorGUILayout.Toggle ("Playback from trace", boolReadTraceFile);
			TraceReader traceReaderScript = Camera.main.GetComponent<TraceReader> ();
			if (boolReadTraceFile) {
				traceReaderScript.file = file;
				traceReaderScript.enabled = true;
			} else {
				traceReaderScript.enabled = false;
			}

			EditorGUILayout.Space ();
			if (GUILayout.Button ("Create Agents From Trace")) {
				GenerateAgentsFromTrace ();
			}
		}
	}

	void GenerateAgentsFromTrace(){
		Vector3 position;
		Vector3 goalPosition;
		GameObject agent;
		SetTargetOnLoad setTargetScript;
		lines = file.text.Split ("\n" [0]);
		int numAgents = int.Parse(lines [1]);
		for (int i = lineAgentDataBegins; i <= lineAgentDataBegins + numAgents; i++) {

			//Splice the data on each line
			string[] data = lines [i].Split ("," [0]);

			// If Time == 0, instantiate the agent
			if (data [0] == "0") {
				position = new Vector3 (float.Parse (data [2]), float.Parse (data [3]), float.Parse (data [4]));
				goalPosition = new Vector3 (float.Parse (data [5]), float.Parse (data [6]), float.Parse (data [7]));

				if (data [1] == "Distracted") {
					agent = (GameObject)PrefabUtility.InstantiatePrefab (distractedAgentPrefab as GameObject);
				} else {
					agent = (GameObject)PrefabUtility.InstantiatePrefab (normativeAgentPrefab as GameObject);
				}

				agent.transform.position = position;
				agent.transform.SetParent (editorCrowdsObj.transform);
				setTargetScript = agent.GetComponent<SetTargetOnLoad> ();
				setTargetScript.enabled = true;
				setTargetScript.target = goalPosition;

			}

		}

	}

	void GenerateRandomAgents(){
		Vector3 newPlacement = Vector3.zero;
		GameObject agent;
		SetTargetOnLoad setTargetScript;
		//Generate normative agents
		for (int i = 0; i < numNormativeAgents; i++) {
			newPlacement = FindFreeLocation (normativeAgentPrefab);
			agent = (GameObject)PrefabUtility.InstantiatePrefab (normativeAgentPrefab as GameObject);
			agent.transform.position = newPlacement;
			agent.transform.SetParent (editorCrowdsObj.transform);
			setTargetScript = agent.GetComponent<SetTargetOnLoad> ();
			setTargetScript.enabled = true;
			//GameObject circle = GameObject.FindGameObjectWithTag ("CircularSpawnRegion");
			//setTargetScript.target = (circle.transform.position - agent.transform.position).normalized * 5.0f;
			setTargetScript.target = target.position;
		}
		//Generate distracted agents
		for (int i = 0; i < numDistractedAgents; i++) {
			newPlacement = FindFreeLocation (distractedAgentPrefab);
			agent = (GameObject)PrefabUtility.InstantiatePrefab (distractedAgentPrefab as GameObject);
			agent.transform.position = newPlacement;
			agent.transform.SetParent (editorCrowdsObj.transform);
			setTargetScript = agent.GetComponent<SetTargetOnLoad> ();
			setTargetScript.enabled = true;
			setTargetScript.target = target.position;
		}

	}

	Vector3 FindFreeLocation(GameObject agentPrefab){
		bool foundNavMeshPosition = false;
		bool freePlacement = false;
		Vector3 newPlacement = Vector3.zero;
		while (!foundNavMeshPosition || !freePlacement) {

			newPlacement = new Vector3 (floor.position.x + Random.Range (-(floor.localScale.x / 2f) + edgeBuffer, (floor.localScale.x / 2f) - edgeBuffer), 0f, floor.position.z + Random.Range (-(floor.localScale.z / 2f) + edgeBuffer, (floor.localScale.z / 2f) - edgeBuffer));

			NavMeshHit hit;
			if (NavMesh.SamplePosition (newPlacement, out hit, 1.0f, NavMesh.AllAreas)) {
				newPlacement = hit.position;
				foundNavMeshPosition = true;
			} else {
				foundNavMeshPosition = false;
			}


			if (Physics.OverlapSphere (newPlacement + Vector3.up, agentPrefab.GetComponent<AgentPredictiveAvoidanceModel> ().agentRadius + 0.1f).Length == 0) {
				//Debug.DrawLine(newPlacement + Vector3.up, newPlacement + Vector3.up + Vector3.forward * agentPrefab.GetComponent<Agent_repulsive>().agentRadius, Color.green, 5f);
				freePlacement = true;
			} else {
				freePlacement = false;
			}//Debug.Log("PLacement was taken"); }
		}

		return newPlacement;
	}
}
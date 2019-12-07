using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;

public class TraceSaver : MonoBehaviour {

	private List<string[]> rowData = new List<string[]>();
	string filePath;
	string delimiter = ",";

	GameObject[] agents;

	int count = 0;

	Scene scene;
	StringBuilder stringBuilder;

	// Use this for initialization
	void Start () {

		stringBuilder = new StringBuilder ();
		agents = GameObject.FindGameObjectsWithTag ("Agent");
		scene = SceneManager.GetActiveScene();
		filePath = getPath ();

		stringBuilder.Append (scene.name);
		stringBuilder.AppendLine ();
		stringBuilder.Append (agents.Length);
		
	}

	// Following method is used to retrive the relative path as device platform
	private string getPath(){
		string filename = scene.name + "_" + DateTime.Now.ToString ("yyyy-mm-dd-hh-mm-ss") + ".csv";
		#if UNITY_EDITOR
		return Application.dataPath + "/Traces/" + filename ;
		#elif UNITY_ANDROID
		return Application.persistentDataPath + filename ;
		#elif UNITY_IPHONE
		return Application.persistentDataPath + "/" + filename;
		#else
		return Application.dataPath + "/" filename;
		#endif
	}

	void SaveTraces(){
		Debug.Log ("Saving traces");
		if (!File.Exists (filePath))
			File.WriteAllText (filePath, stringBuilder.ToString ());
		else
			File.AppendAllText (filePath, stringBuilder.ToString ());

		stringBuilder.Length = 0;
	}

	// Update is called once per frame
	void FixedUpdate () {

		if (enabled) {

			if (Input.GetKeyUp("escape"))
			{
				SaveTraces ();
			}

			for (int i = 0; i < agents.Length; i++){
				var agent = agents [i];
				if (agent != null) {
					var agentAI = agent.transform.GetComponent<Agent> ();
					Vector3 agentGoal = agentAI.GetFinalGoal ();		
					float agentAttentiveness;
					string agentAttentiveState;
					if (agentAI.GetAgentType () == "Distracted") {				
						var attentivenessScript = agent.transform.GetComponent<DistractedAgent> ();
						agentAttentiveness = attentivenessScript.GetCurrentAttentiveness ();
						agentAttentiveState = attentivenessScript.GetAttentiveState();

					} else {
						agentAttentiveness = 1.0f;
						agentAttentiveState = "Paying Attention";
					}
					

					//Current save format: Time, AgentPrefabType, PositionX, PositionY, PositionZ, GoalPositionX, GoalPositionY, GoalPositionZ
					string[] output = new string[] {
						count.ToString (),
						agentAI.GetAgentType (),
						agent.transform.position.x.ToString (),
						agent.transform.position.y.ToString (),
						agent.transform.position.z.ToString (),
						agentGoal.x.ToString (),
						agentGoal.y.ToString (),
						agentGoal.z.ToString (),
						agentAttentiveness.ToString (),
						agentAttentiveState
					};

					int length = output.Length;

					stringBuilder.AppendLine ();
					stringBuilder.Append (output [0]);
					for (int index = 1; index < length; index++) {
						stringBuilder.Append (delimiter + output [index]);
					}
				
				} else {
					// If the agent has been destroyed, just add an empty line
					stringBuilder.AppendLine ();
				}
			}

			count++;
		}
	}
}

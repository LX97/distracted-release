using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TraceSaver : MonoBehaviour {

	public bool enabled = false;

	private List<string[]> rowData = new List<string[]>();
	string filePath;
	string delimiter = ",";

	GameObject[] agents;

	int count = 0;


	StringBuilder stringBuilder;

	// Use this for initialization
	void Start () {
		if (enabled) {
			stringBuilder = new StringBuilder ();
			filePath = getPath ();
			agents = GameObject.FindGameObjectsWithTag ("Agent");
			Scene scene = SceneManager.GetActiveScene();
			if (!File.Exists (filePath))
				File.WriteAllText (filePath, scene.name.ToString ());
			else
				File.AppendAllText (filePath, scene.name.ToString ());
		}
	}

	// Following method is used to retrive the relative path as device platform
	private string getPath(){
		#if UNITY_EDITOR
		return Application.dataPath + "/Traces/" + DateTime.Now.ToString("yyyy-mm-dd-hh-mm-ss") + ".csv" ;
		#elif UNITY_ANDROID
		return Application.persistentDataPath + DateTime.Now.ToString("yyyy-mm-dd-hh-mm-ss") + ".csv" ;
		#elif UNITY_IPHONE
		return Application.persistentDataPath + "/" + DateTime.Now.ToString("yyyy-mm-dd-hh-mm-ss") + ".csv";
		#else
		return Application.dataPath + "/" + DateTime.Now.ToString("yyyy-mm-dd-hh-mm-ss") + ".csv" ;
		#endif
	}

	void SaveTraces(){
		Debug.Log ("Saving traces");
		if (!File.Exists (filePath))
			File.WriteAllText (filePath, stringBuilder.ToString ());
		else
			File.AppendAllText (filePath, stringBuilder.ToString ());
	}

	// Update is called once per frame
	void FixedUpdate () {

		if (enabled) {

			if (Input.GetKeyDown("escape"))
			{
				SaveTraces ();
			}

			foreach (GameObject agent in agents) {
				if (agent != null) {
					var agentAI = agent.transform.GetComponent<Agent> ();
					Vector3 agentGoal = agentAI.GetFinalGoal ();		

					float agentAttentiveness;
					var attentivenessScript = agent.transform.GetComponent<DistractedAgent> ();
					if (attentivenessScript != null) {
						agentAttentiveness = attentivenessScript.getCurrentAttentiveness ();
					} else {
						agentAttentiveness = 1.0f;
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
						agentAttentiveness.ToString ()
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

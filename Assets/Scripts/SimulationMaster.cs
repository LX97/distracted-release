using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The simulation controller
/// </summary>
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

	/// <summary>
	/// The global average of agent average kinetic energies
	/// </summary>
	private float globalAvgAgentAvgKineticEnergy;

	/// <summary>
	/// The average of agent pleEnergies
	/// </summary>
	private float avgAgentEffort;

	/// <summary>
	/// The number of trials (simulations) to run
	/// </summary>
	public int maxTrials;

	/// <summary>
	/// Ref to trial counter script
	/// </summary>
	public CountSimulationTrials trialCounter;

    /// <summary>
    /// Experiment file path
    /// </summary>
	string filePath;

    /// <summary>
    /// The experiment name
    /// </summary>
	public string experimentName = "Experiment 1";

    /// <summary>
    /// String builder ref
    /// </summary>
	StringBuilder stringBuilder;

    /// <summary>
    /// Experiment data file header
    /// </summary>
	string header = "Scenario, Experiment Name, Total Agents, # Distracted Agents, Simulation Time, Flow Rate, Average Agent Path Length, Average Agent Kinetic Energy Average, Average Agent Effort";

    /// <summary>
    /// The number of distracted agents
    /// </summary>
    int numDistractedAgents = 0;

    /// <summary>
    /// Field delimiter for experiment data
    /// </summary>
	string delimiter = ",";

    /// <summary>
    /// Save collected data flag
    /// </summary>
	public bool saveData = false;

    /// <summary>
    /// Audio ref
    /// </summary>
	AudioSource audioData;

    /// <summary>
    /// Start this instance
    /// </summary>
	void Start()
    {
		agents = GameObject.FindGameObjectsWithTag("Agent");
		goals = GameObject.FindGameObjectsWithTag("Goal");
		trialCounter = GameObject.FindGameObjectWithTag("TrialCounter").GetComponent<CountSimulationTrials>();
		trialCounter.SetMaxTrials (maxTrials);
		initialNumberOfAgents = agents.Length;
		stringBuilder = new StringBuilder ();
		filePath = getPath ();
		audioData = GetComponent<AudioSource>();


		// Count the number of distracted agents
		for (int i = 0; i < agents.Length; i++) {
			if (agents [i].GetComponent<Agent> ().GetAgentType () == "Distracted") {
				numDistractedAgents++;
			}
		}
	}

    /// <summary>
    /// Used to retrive the relative path as device platform
    /// </summary>
    /// <returns></returns>
    private string getPath()
    {
		string filename = "Simulation Data.csv";
		#if UNITY_EDITOR
		return Application.dataPath + "/Data/" + filename ;
		#elif UNITY_ANDROID
		return Application.persistentDataPath + filename ;
		#elif UNITY_IPHONE
		return Application.persistentDataPath + "/" + filename;
		#else
		return Application.dataPath + "/" filename;
		#endif
	}

    /// <summary>
    /// Save the stats out to the experiment data file
    /// </summary>
	void SaveStatisticsToDataFile()
    {
		Debug.Log ("Saving data");
		if (!File.Exists (filePath)) {
			File.WriteAllText (filePath, header);
			File.AppendAllText (filePath, stringBuilder.ToString ());
		}else
			File.AppendAllText (filePath, stringBuilder.ToString ());

		stringBuilder.Length = 0;
	}

    /// <summary>
    /// Is simulation complete?
    /// </summary>
    /// <returns>bool complete</returns>
	public bool CheckSimulationComplete()
    {
		return simulationFinished;
	}

    /// <summary>
    /// Get simulation completion time
    /// </summary>
    /// <returns>float simulation completion time</returns>
	public float GetSimulationCompletionTime()
    {
		return simulationCompletionTime;
	}

    /// <summary>
    /// Get the flow rate
    /// </summary>
    /// <returns>float flow rate</returns>
	public float GetFlowRate()
    {
		return flowRate;
	}

    /// <summary>
    /// Get the average path length
    /// </summary>
    /// <returns>float average path length</returns>
	public float GetAvgPathLength()
    {
		return avgAgentPathLength;
	}

    /// <summary>
    /// Get the number of agents intially
    /// </summary>
    /// <returns>int initial agent count</returns>
	public int GetNumInitialAgents()
    {
		return initialNumberOfAgents;
	}

    /// <summary>
    /// Get the nuber of agents that reached their goals
    /// </summary>
    /// <returns>int total goal completion</returns>
	public int GetNumAgentsReachedGoal()
    {
		return sumAgentsReachedGoal;
	}

    /// <summary>
    /// Update this instance
    /// </summary>
	void Update()
    {
		if (Input.GetKeyUp("space")){
		    string sceneName = SceneManager.GetActiveScene ().name;
		    string screenshotName = sceneName + "_" + experimentName + "_" + numDistractedAgents + "-distracted-agents.png";
		    ScreenCapture.CaptureScreenshot(screenshotName);
		    Debug.Log ("Screenshot saved");
		}

		if (simulationFinished == false) {
			sumAgentsReachedGoal = 0;
			for (int i = 0; i < goals.Length; i++) {
				reachAndDestroyScript = goals [i].GetComponent<ReachAndDestroy> ();
				sumAgentsReachedGoal += reachAndDestroyScript.GetNumAgentsReachedGoal ();
			}

			if (sumAgentsReachedGoal == initialNumberOfAgents) {
				simulationFinished = true;
				simulationCompletionTime = (Time.fixedUnscaledTime - trialCounter.GetTimeLastReload());
				flowRate = initialNumberOfAgents / simulationCompletionTime;
				float sumAgentPathLength = 0.0f;
				float sumAgentAvgKineticEnergy = 0.0f;
				float sumAgentEffort = 0.0f;
				for (int i = 0; i < goals.Length; i++) {
					reachAndDestroyScript = goals [i].GetComponent<ReachAndDestroy> ();
					sumAgentPathLength += reachAndDestroyScript.GetTotalAgentDistance ();
					sumAgentAvgKineticEnergy += reachAndDestroyScript.GetSumAgentAvgKineticEnergy ();
					sumAgentEffort += reachAndDestroyScript.GetSumAgentEffort ();
				}
				avgAgentPathLength = sumAgentPathLength / initialNumberOfAgents;
				globalAvgAgentAvgKineticEnergy = sumAgentAvgKineticEnergy / initialNumberOfAgents;
				avgAgentEffort = sumAgentEffort / initialNumberOfAgents;


				string sceneName = SceneManager.GetActiveScene ().name;
				if (saveData) {
					
					
					string[] output = new string[] {
						sceneName,
						experimentName,
						initialNumberOfAgents.ToString (),
						numDistractedAgents.ToString (),
						simulationCompletionTime.ToString (),
						flowRate.ToString (),
						avgAgentPathLength.ToString (),
						globalAvgAgentAvgKineticEnergy.ToString(),
						avgAgentEffort.ToString()
					};
					
					int length = output.Length;
					stringBuilder.AppendLine ();
					stringBuilder.Append (output [0]);
					for (int index = 1; index < length; index++) {
						stringBuilder.Append (delimiter + output [index]);
					}

					SaveStatisticsToDataFile ();
					trialCounter.IncrementTrials ();
					if (trialCounter.GetNumberOfTrials () < trialCounter.GetMaxTrials ()) {
						Debug.Log ("Trials finished: " + trialCounter.GetNumberOfTrials ().ToString ());
						Debug.Log (simulationCompletionTime);
						trialCounter.SetTimeLevelReloaded (Time.fixedUnscaledTime);
						SceneManager.LoadScene (sceneName); //Restart scene
					} else {
						audioData.Play ();
					}
				}
			}
		}
			
	}
}


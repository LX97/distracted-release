using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGraph : MonoBehaviour
{
	/* * * *
     * 
     *   [DebugGUIGraph]
     *   Renders the variable in a graph on-screen. Attribute based graphs will updates every Update.
     *    Lets you optionally define:
     *        max, min  - The range of displayed values
     *        r, g, b   - The RGB color of the graph (0~1)
     *        group     - The order of the graph on screen. Graphs may be overlapped!
     *        autoScale - If true the graph will readjust min/max to fit the data
     *   
     *   [DebugGUIPrint]
     *    Draws the current variable continuously on-screen as 
     *    $"{GameObject name} {variable name}: {value}"
     *   
     *   For more control, these features can be accessed manually.
     *    DebugGUI.SetGraphProperties(key, ...) - Set the properties of the graph with the provided key
     *    DebugGUI.Graph(key, value)            - Push a value to the graph
     *    DebugGUI.LogPersistent(key, value)    - Print a persistent log entry on screen
     *    DebugGUI.Log(value)                   - Print a temporary log entry on screen
     *    
     *   See DebugGUI.cs for more info
     * 
     * * * */

	// Disable Field Unused warning
	#pragma warning disable 0414

	private SimulationMaster simulationMasterScript;

	// User inputs, print and graph in one!
	[DebugGUIPrint]
	float numReachedGoal;

	[DebugGUIGraph(group: 0, r: 1, g: 0.3f, b: 0.3f, max:120)]
	float numRemaining;

	void Awake()
	{
		
		simulationMasterScript = Camera.main.GetComponent<SimulationMaster> ();

	}

	void Update()
	{
		// Update the fields our attributes are graphing
		numReachedGoal = (float) simulationMasterScript.GetNumAgentsReachedGoal();
		numRemaining = (float) simulationMasterScript.GetNumInitialAgents() - numReachedGoal;
		if (numRemaining == 0.0f) {
			DebugGUI.LogPersistent ("simulationTime", "Time to complete simulation: " + simulationMasterScript.GetSimulationCompletionTime().ToString());
			DebugGUI.LogPersistent ("Flow Rate", "Flow Rate: " + simulationMasterScript.GetFlowRate().ToString());
			DebugGUI.LogPersistent ("Avg. Path Length", "Avg. Path Length: " + simulationMasterScript.GetAvgPathLength().ToString());
		}

	}

	void FixedUpdate()
	{
	}

	void OnDestroy()
	{
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simulation trial counter
/// </summary>
public class CountSimulationTrials : MonoBehaviour
{
    /// <summary>
    /// Number of simulation trials
    /// </summary>
	private int simulationTrials;

    /// <summary>
    /// Maximum number of simulation trials
    /// </summary>
	private int maxSimulationTrials;

    /// <summary>
    /// Time the scene was reloaded
    /// </summary>
	private float timeSceneReloaded;


    /// <summary>
    /// Awaken this instance
    /// </summary>
	void Awake()
	{
		simulationTrials = 0;

		DontDestroyOnLoad(this.gameObject);
	}

    /// <summary>
    /// Set the maxmimum number of trials
    /// </summary>
    /// <param name="value">thenumber of trials</param>
	public void SetMaxTrials(int value){
		maxSimulationTrials = value;
	}

    /// <summary>
    /// Get the maximum number of trials
    /// </summary>
    /// <returns>int the maximum number of trials</returns>
	public int GetMaxTrials(){
		return maxSimulationTrials;
	}

    /// <summary>
    /// Set the time the lvel was reloaded
    /// </summary>
    /// <param name="value">time</param>
	public void SetTimeLevelReloaded(float value){
		timeSceneReloaded = value;
	}

    /// <summary>
    /// Get the time the level was last reloaded
    /// </summary>
    /// <returns>float time of reload</returns>
	public float GetTimeLastReload(){
		return timeSceneReloaded;
	}

    /// <summary>
    /// Increment the current trial number
    /// </summary>
	public void IncrementTrials(){
		simulationTrials++;
	}

    /// <summary>
    /// Get the currnt number of trials
    /// </summary>
    /// <returns>int number of trials</returns>
	public int GetNumberOfTrials(){
		return simulationTrials;
	}
}

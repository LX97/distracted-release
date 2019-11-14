using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountSimulationTrials : MonoBehaviour {

	private int simulationTrials;
	private int maxSimulationTrials;
	private float timeSceneReloaded;

	void Awake()
	{
		simulationTrials = 0;

		DontDestroyOnLoad(this.gameObject);
	}

	public void SetMaxTrials(int value){
		maxSimulationTrials = value;
	}

	public int GetMaxTrials(){
		return maxSimulationTrials;
	}

	public void SetTimeLevelReloaded(float value){
		timeSceneReloaded = value;
	}

	public float GetTimeLastReload(){
		return timeSceneReloaded;
	}

	public void IncrementTrials(){
		simulationTrials++;
	}

	public int GetNumberOfTrials(){
		return simulationTrials;
	}
}

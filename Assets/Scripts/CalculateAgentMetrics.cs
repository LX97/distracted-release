using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateAgentMetrics : MonoBehaviour {

	/// <summary>
	/// The total distance the agent has travelled so far
	/// </summary>
	private float sumDistanceTravelled = 0.0f;

	private Vector3 lastPosition;

	private Vector3 changeInPosition;
	private float distanceTravelledSinceLastFrame = 0.0f;
	private float timePassedSinceLastFrame = 0.0f;
	private float instantaneousSpeed;
	private Vector3 instantaneousVelocity;
	private float sumTimeEnabled = 0.0f;
	private float instantaneousKineticEnergy = 0.0f;
	private float integralOfKineticEnergy = 0.0f;

	//These should be constants
	private float MASS = 1.0f;
	private float E_S = 2.23f;
	private float E_W = 1.26f;
	private float pleEnergy = 0.0f;

	// Use this for initialization
	void Start () {
		lastPosition = transform.position;
	}

	public float GetDistanceTravelled(){
		return sumDistanceTravelled;
	}

	public float GetTimeEnabled(){
		return sumTimeEnabled;
	}

	public float GetEffort(){
		return pleEnergy;
	}

	public float GetIntegralOfKineticEnergy(){
		return integralOfKineticEnergy;
	}

	// Update is called once per frame
	void Update () {
		timePassedSinceLastFrame = Time.deltaTime;
		sumTimeEnabled += timePassedSinceLastFrame;
		distanceTravelledSinceLastFrame = (transform.position - lastPosition).magnitude;
		instantaneousSpeed = distanceTravelledSinceLastFrame / timePassedSinceLastFrame;

		changeInPosition = transform.position - lastPosition;
		instantaneousVelocity = changeInPosition / timePassedSinceLastFrame;

		pleEnergy += MASS * (E_S + E_W * instantaneousVelocity.sqrMagnitude) * timePassedSinceLastFrame;

		instantaneousKineticEnergy = 0.5f * instantaneousSpeed * instantaneousSpeed;
		integralOfKineticEnergy += 2.0f * instantaneousKineticEnergy * timePassedSinceLastFrame;

		sumDistanceTravelled += distanceTravelledSinceLastFrame;
		lastPosition = transform.position;
	}
}

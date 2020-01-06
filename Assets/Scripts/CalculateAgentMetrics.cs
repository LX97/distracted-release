using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Agent metric class for benchmarking and experiments
/// </summary>
public class CalculateAgentMetrics : MonoBehaviour {

	/// <summary>
	/// The total distance the agent has travelled so far
	/// </summary>
	private float sumDistanceTravelled = 0.0f;

    /// <summary>
    /// Last position
    /// </summary>
	private Vector3 lastPosition;

    /// <summary>
    /// Change in position, or position delta
    /// </summary>
	private Vector3 changeInPosition;

    /// <summary>
    /// Distance travelled since last frame
    /// </summary>
	private float distanceTravelledSinceLastFrame = 0.0f;

    /// <summary>
    /// Time passed since last frame
    /// </summary>
	private float timePassedSinceLastFrame = 0.0f;

    /// <summary>
    /// Instantaneous speed
    /// </summary>
	private float instantaneousSpeed;

    /// <summary>
    /// Instantaneou velocity
    /// </summary>
	private Vector3 instantaneousVelocity;

    /// <summary>
    /// Total time enabled
    /// </summary>
	private float sumTimeEnabled = 0.0f;

    /// <summary>
    /// Instantaneous kinetic energu
    /// </summary>
	private float instantaneousKineticEnergy = 0.0f;

    /// <summary>
    /// Integral of kinetic energy
    /// </summary>
	private float integralOfKineticEnergy = 0.0f;

	//These should be constants
    /// <summary>
    /// All agents = 1kg == ignoring mass difference amongst crowd members
    /// </summary>
	private float MASS = 1.0f;

    /// <summary>
    /// Resting/standing energy expenditure rate (average human)
    /// </summary>
	private float E_S = 2.23f;

    /// <summary>
    /// Moving energy expenditure rate (average human)
    /// </summary>
	private float E_W = 1.26f;

    /// <summary>
    /// PLEdestrians Energy cost (Total metabolic energy expenditure)
    /// </summary>
	private float pleEnergy = 0.0f;

	
    /// <summary>
    /// Start this instance
    /// </summary>
	void Start () {
		lastPosition = transform.position;
	}

    /// <summary>
    /// Get the distance travelled
    /// </summary>
    /// <returns>float distance travelled</returns>
	public float GetDistanceTravelled(){
		return sumDistanceTravelled;
	}

    /// <summary>
    /// Get the time enabled
    /// </summary>
    /// <returns>float time enabled</returns>
	public float GetTimeEnabled(){
		return sumTimeEnabled;
	}

    /// <summary>
    /// Get the effort
    /// </summary>
    /// <returns>float effort (PLEdestrians total metabolic energy expenditure)</returns>
	public float GetEffort(){
		return pleEnergy;
	}

    /// <summary>
    /// Get the integral of kinetic energy
    /// </summary>
    /// <returns>float integral of kinetic energy</returns>
	public float GetIntegralOfKineticEnergy(){
		return integralOfKineticEnergy;
	}

	/// <summary>
    /// Update this instance, once per frame
    /// </summary>
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

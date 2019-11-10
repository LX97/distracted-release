using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateDistanceTraveled : MonoBehaviour {

	/// <summary>
	/// The total distance the agent has travelled so far
	/// </summary>
	private float sumDistanceTravelled = 0.0f;

	private Vector3 lastPosition;

	// Use this for initialization
	void Start () {
		lastPosition = transform.position;
	}

	public float GetDistanceTravelled(){
		return sumDistanceTravelled;
	}

	// Update is called once per frame
	void Update () {
		sumDistanceTravelled += (transform.position - lastPosition).magnitude;
		lastPosition = transform.position;
	}
}

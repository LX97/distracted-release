using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour {
    /// <summary>
	///  Sets the target for the agents and computes the path
	/// </summary>
	public abstract void SetTarget(Vector3 targetPosition);

    public abstract Vector3 GetCurrentGoal();
}

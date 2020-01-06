using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract Agent class for agent type commonalities
/// </summary>
public abstract class Agent : MonoBehaviour {
    /// <summary>
	///  Sets the target for the agents and computes the path
	/// </summary>
	public abstract void SetTarget(Vector3 targetPosition);

    /// <summary>
    /// Returns an agent type string
    /// </summary>
    /// <returns>string describing agent type</returns>
	public abstract string GetAgentType();

    /// <summary>
    /// Handles returning a final scene goal
    /// </summary>
    /// <returns>Vector3 goal position in world</returns>
	public abstract Vector3 GetFinalGoal();

    /// <summary>
    /// Handles returning the currnet goal
    /// </summary>
    /// <returns>Vector3 current goal position in world</returns>
    public abstract Vector3 GetCurrentGoal();
}

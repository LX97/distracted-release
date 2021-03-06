using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WallRepulsiveForce : MonoBehaviour
{
	/// <summary>
	/// Wall distance parameters
	/// </summary>
	public float wallDistance = 0.1f;

	/// <summary>
	/// Wall steepness parameter
	/// </summary>
	public float wallSteepness = 2.0f;

	/// <summary>
	/// Ideal wall distane parameter
	/// </summary>
	private float idealWallDistance;
    
    /// <summary>
    /// List of agent gme objects
    /// </summary>
	GameObject[] agents;

    /// <summary>
    /// Unit wall east
    /// </summary>
	Vector3 x_positive= new Vector3(1.0f, 0.0f, 0.0f);

    /// <summary>
    /// Unit wall West
    /// </summary>
	Vector3 x_negative = new Vector3(-1.0f, 0.0f, 0.0f);

    /// <summary>
    /// Unit wall North
    /// </summary>
	Vector3 z_positive = new Vector3(0.0f, 0.0f, 1.0f);

    /// <summary>
    /// Unit wall South
    /// </summary>
	Vector3 z_negative = new Vector3(0.0f, 0.0f, -1.0f);

    /// <summary>
    /// Wall normal
    /// </summary>
	Vector3 wall_normal;

    /// <summary>
    /// Distance
    /// </summary>
	Vector3 distance_wa;

    /// <summary>
    /// Line definition
    /// </summary>
	Vector3 line_start, line_end;

    /// <summary>
    /// Box definition
    /// </summary>
	box box1;

	struct box
	{
		public float xmax, zmax, xmin, zmin;
	}

	/// <summary>
    /// Initialize this instance
    /// </summary>
	void Start () {
		agents = GameObject.FindGameObjectsWithTag("Agent");
		box1.xmin = transform.position.x + transform.localScale.x/2.0f; 
		box1.xmax = transform.position.x - transform.localScale.x/2.0f; 

		box1.zmax = transform.position.z + transform.localScale.z/2.0f; 
		box1.zmin = transform.position.z - transform.localScale.z/2.0f; 

	}

    /// <summary>
    /// Physics Update
    /// </summary>
	void FixedUpdate() {

		foreach (GameObject agent in agents) {
			if (agent != null) {
				AgentPredictiveAvoidanceModel agentOther = agent.GetComponent<AgentPredictiveAvoidanceModel> ();
				Rigidbody agentRigidBody = agent.GetComponent<Rigidbody> ();
				Transform agentTransform = agent.GetComponent<Transform> ();

				float idealWallDistance = agentOther.agentRadius + wallDistance;
				float safe = idealWallDistance * idealWallDistance;

				wall_normal = calc_wall_normal (agentTransform, box1);
				Pair<Vector3, Vector3> line_p = calcWallPointsFromNormal (box1, wall_normal);
				Vector3 n_w = agentTransform.position - closestPointLineSegment (line_p.First, line_p.Second, agentTransform.position);


				//Debug.DrawLine (line_p.First, line_p.Second, Color.blue, 0.02f);

				//Debug.DrawLine (new Vector3(5, 0, box1.zmin), new Vector3(5, 0, box1.zmax), Color.red, 0.02f);

				float d_w = n_w.sqrMagnitude;

				if (d_w < safe) {
					d_w = Mathf.Sqrt (d_w);
					if (d_w > 0)
						n_w /= d_w;

					float distanceMinimumRadius = (d_w - agentOther.agentRadius) < 0.001f ? 0.001f : d_w - agentOther.agentRadius;
					//Debug.Log("idealWallDistance: " + idealWallDistance);
					//Debug.Log("d_w: " + d_w);
					//Debug.Log("distanceMinimumRadius: " + distanceMinimumRadius);
					//Debug.Log("n_w.magnitude: " + n_w.magnitude);
					//Debug.Log("Final Magnitude: " + (idealWallDistance - d_w) / Mathf.Pow(distanceMinimumRadius, wallSteepness));
					Vector3 drivingForce = (idealWallDistance - d_w) / Mathf.Pow (distanceMinimumRadius, wallSteepness) * n_w;

					//Debug.DrawLine(agentTransform.position, agentTransform.position + drivingForce, Color.blue, 0.02f);
					agentRigidBody.AddForce (drivingForce, ForceMode.Force);

					// MELISSA check HERE
					DistractedAgent distractionScript = agent.GetComponent<DistractedAgent>();
					if (distractionScript != null){
						distractionScript.PayAttention();
					}
				}

			}
		}
	}

    /// <summary>
    /// Calculate the wall normal
    /// </summary>
    /// <param name="rb">Rigidbody reference</param>
    /// <param name="box1">box definition</param>
    /// <returns></returns>
	Vector3 calc_wall_normal(Transform rb, box box1)
	{

		if (rb.position.x > box1.xmax)
		{
			if (rb.position.z > box1.zmax)
			{
				if (Mathf.Abs(rb.position.z - box1.zmax) >
					Mathf.Abs(rb.position.x - box1.xmax))
				{
					return z_positive; 
				}
				else
				{
					return x_positive;
				}

			}
			else if (rb.position.z < box1.zmin)
			{
				if (Mathf.Abs(rb.position.z - box1.zmin) >
					Mathf.Abs(rb.position.x - box1.xmax))
				{
					return z_negative; 
				}
				else
				{
					return x_positive;
				}

			}
			else
			{ // in between zmin and zmax
				return x_positive;
			}

		}
		else if (rb.position.x < box1.xmin)
		{
			if (rb.position.z > box1.zmax)
			{
				if (Mathf.Abs(rb.position.z - box1.zmax) >
					Mathf.Abs(rb.position.x - box1.xmin))
				{
					return z_positive;
				}
				else
				{
					return x_negative;
				}

			}
			else if (rb.position.z < box1.zmin)
			{
				if (Mathf.Abs(rb.position.z - box1.zmin) >
					Mathf.Abs(rb.position.x - box1.xmin))
				{
					return z_negative; 
				}
				else
				{
					return x_negative;
				}

			}
			else
			{ // in between zmin and zmax
				return x_negative;
			}
		}
		else // between xmin and xmax
		{
			if (rb.position.z > box1.zmax)
			{
				return z_positive;
			}
			else if (rb.position.z < box1.zmin)
			{
				return z_negative;
			}
			else
			{ // What do we do if the agent is inside the wall?? Lazy Normal
				return x_positive;   // This is not accurate.
			}
		}


	}

    /// <summary>
    /// Calculate wall porints from normal
    /// </summary>
    /// <param name="box1">box definition</param>
    /// <param name="normal">wall normal</param>
    /// <returns></returns>
	Pair<Vector3, Vector3> calcWallPointsFromNormal(box box1, Vector3 normal)
	{
		Pair<Vector3, Vector3> line = new Pair<Vector3, Vector3>();

		if (normal == z_positive)
		{ 
			line.find_line(new Vector3(box1.xmin, 0, box1.zmax), new Vector3(box1.xmax,0, box1.zmax));
			return line;
		}
		else if (normal == z_negative)
		{
			line.find_line(new Vector3(box1.xmin, 0, box1.zmin), new Vector3(box1.xmax,0 , box1.zmin));
			return line;
		}
		else if (normal == x_positive)
		{
			line.find_line(new Vector3(box1.xmax, 0, box1.zmin), new Vector3(box1.xmax,0, box1.zmax));
			return line;
		}
		else
		{
			line.find_line(new Vector3(box1.xmin, 0, box1.zmin), new Vector3(box1.xmin,0 , box1.zmax));
			return line;
		}

	}

    /// <summary>
    /// Get the closest point on line segment
    /// </summary>
    /// <param name="line_start">line start point</param>
    /// <param name="line_end">line end point</param>
    /// <param name="p">point to test</param>
    /// <returns></returns>
	Vector3 closestPointLineSegment(Vector3 line_start,Vector3 line_end, Vector3 p)
	{
		//    return line_start;

		float dota = (p.x - line_start.x) * (line_end.x - line_start.x) + (p.z - line_start.z) * (line_end.z - line_start.z);
		if (dota <= 0) // point line_start is closest to p
			return line_start;

		float dotb = (p.x - line_end.x) * (line_start.x - line_end.x) + (p.z - line_end.z) * (line_start.z - line_end.z);
		if (dotb <= 0) // point line_end is closest to p
			return line_end;

		// find closest point
		float slope = dota / (dota + dotb);

		return line_start + (line_end - line_start)*slope;
	}

    /// <summary>
    /// Pair definition class
    /// </summary>
    /// <typeparam name="T">item 1</typeparam>
    /// <typeparam name="U">item 2</typeparam>
	public class Pair<T, U>
	{
		public Pair()
		{
		}

		public Pair(T first, U second)
		{
			this.First = first;
			this.Second = second;
		}

		public void find_line(T a, U b)
		{
			this.First = a;
			this.Second = b;
		}

		public T First { get; set; }
		public U Second { get; set; }
	};
}

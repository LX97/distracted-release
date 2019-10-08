using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class CrowdGeneratorStandard: MonoBehaviour {
	/// <summary>
	/// The agent prefab.
	/// </summary>
	public Transform agentPrefab;

	/// <summary>
	/// The floor. This can be an empty game object, but it must be axis aligned
	/// The script simply uses the local scale and position to spawn agents in an axis aligned rectangle, or AABB
	/// </summary>
	public Transform floor;

	/// <summary>
	/// The edge buffer.  Agents will spawn within an
	/// area the size of the floor, minus this buffer size on all sides
	/// </summary>
	public float edgeBuffer;

	/// <summary>
	/// The number of agents.
	/// </summary>
	public int numberOfAgents;

	/// <summary>
	/// The target.
	/// </summary>
	public Transform target;


	
	/// <summary>
	/// Initialize this instance. Use Awake() instead of Start() to ensure agents are generated before other scripts need them.
	/// </summary>
	void Awake ()
    {
		for (int i = 0; i < numberOfAgents; i++)
        {
			Vector3 newPlacement = Vector3.zero;
			bool foundNavMeshPosition = false;
            bool freePlacement = false;

            while (!foundNavMeshPosition || !freePlacement)
            {
				newPlacement = new Vector3 (floor.position.x + Random.Range (-(floor.localScale.x / 2f) + edgeBuffer, (floor.localScale.x / 2f) - edgeBuffer), 0f, floor.position.z + Random.Range (-(floor.localScale.z / 2f) + edgeBuffer, (floor.localScale.z / 2f) - edgeBuffer));

                NavMeshHit hit;
				if (NavMesh.SamplePosition(newPlacement, out hit, 1.0f, NavMesh.AllAreas))
                {
					newPlacement = hit.position;
					foundNavMeshPosition = true;
				} else { foundNavMeshPosition = false; }

                if (Physics.OverlapSphere(newPlacement + Vector3.up, agentPrefab.GetComponent<AgentRepulsiveForce>().agentRadius + 0.1f).Length == 0)
                {
                    //Debug.DrawLine(newPlacement + Vector3.up, newPlacement + Vector3.up + Vector3.forward * agentPrefab.GetComponent<Agent_repulsive>().agentRadius, Color.green, 5f);
                    freePlacement = true;
                }
                else { freePlacement = false; }//Debug.Log("PLacement was taken"); }
			}

			Transform agent = Instantiate (agentPrefab, newPlacement, Quaternion.identity) as Transform;
			
			agent.transform.parent = this.transform;

            var agentAI = agent.transform.GetComponent<Agent>();
			agentAI.SetTarget (target.position);
        }
    }
}

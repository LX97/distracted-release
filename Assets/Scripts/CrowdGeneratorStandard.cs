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
	/// Start this instance.
	/// </summary>
	void Start () {
		for (int i = 0; i < numberOfAgents; i++) {
			Vector3 newPlacement = Vector3.zero;
			bool foundNavMeshPosition = false;

			while (!foundNavMeshPosition) {
				newPlacement = new Vector3 (floor.position.x + Random.Range (-(floor.localScale.x / 2f) + edgeBuffer, (floor.localScale.x / 2f) - edgeBuffer), 0f, floor.position.z + Random.Range (-(floor.localScale.z / 2f) + edgeBuffer, (floor.localScale.z / 2f) - edgeBuffer));

				NavMeshHit hit;
				if (NavMesh.SamplePosition(newPlacement, out hit, 1.0f, NavMesh.AllAreas)){
					newPlacement = hit.position;
					foundNavMeshPosition = true;
				}
			}

			Transform agent = Instantiate (agentPrefab, newPlacement, Quaternion.identity) as Transform;
			
			agent.transform.parent = this.transform;

            var agentAI = agent.transform.GetComponent<ShowGoldenPath>();
            agentAI.SetTarget(target.position);
            //var agentAI = agent.transform.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            //agentAI.SetDestination(target.position);
        }
	}
}

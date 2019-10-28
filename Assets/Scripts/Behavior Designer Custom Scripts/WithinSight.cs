using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
public class WithinSight : Conditional {

	public float fieldOfViewAngle;
	public string targetTag;
	public SharedGameObjectList targetsInSight;
	public float maxSightDistance;

	private GameObject[] possibleTargets;

	public override void OnAwake(){
		
		possibleTargets = GameObject.FindGameObjectsWithTag (targetTag);
	}

	public override TaskStatus OnUpdate ()
	{
		int countTargets = 0;
		for (int i = 0; i < possibleTargets.Length; ++i) {
			if (withinSight(possibleTargets[i].transform, fieldOfViewAngle)){
				targetsInSight.Value.Add (possibleTargets [i]);
				countTargets++;
			}
		}
		if (countTargets != 0){
			return TaskStatus.Success;
		}else{
			return TaskStatus.Failure;
		}
	}


	public bool withinSight(Transform targetTransform, float fieldOfViewAngle){
		Vector3 direction = targetTransform.position - transform.position;
		if (Vector3.Angle (direction, transform.forward) < fieldOfViewAngle) {
			if (direction.magnitude < maxSightDistance) {
				return true;
			}
		}
		return false;
	}
}

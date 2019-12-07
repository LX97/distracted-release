using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class SetMaterialColor : Action {
	
	public Color color;

	public override TaskStatus OnUpdate(){
		gameObject.GetComponentInChildren<Renderer> ().material.color = color;
		return TaskStatus.Success;
	}
}

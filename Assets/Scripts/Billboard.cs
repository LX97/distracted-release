using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
	Transform cameraPos;

	void Start(){

		cameraPos = Camera.main.transform;
		transform.rotation = cameraPos.rotation;

	}


	void Update() 
	{
		transform.position = transform.parent.position + new Vector3(0, 3.0f, 0.5f);
		transform.rotation = Quaternion.Euler (cameraPos.rotation.eulerAngles.x, cameraPos.rotation.eulerAngles.y, 0);

	}
}
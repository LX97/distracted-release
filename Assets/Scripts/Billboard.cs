using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
	Vector3 cameraPos;
	
	void Update() 
	{
		transform.position = transform.parent.position + new Vector3(0, 4.0f, 2.0f);
		transform.rotation = Quaternion.Euler (90.0f, 0.0f, 0.0f);

		Vector3 targetPostition = new Vector3( cameraPos.x, 
			cameraPos.y, 
			transform.position.z ) ;
		this.transform.LookAt( targetPostition ) ;

	}
}
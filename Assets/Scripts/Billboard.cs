using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple billboard object script
/// </summary>
public class Billboard : MonoBehaviour
{
    /// <summary>
    /// Current camera transform
    /// </summary>
	Transform cameraPos;

    /// <summary>
    /// Start this instance
    /// </summary>
	void Start()
    {
		cameraPos = Camera.main.transform;
		transform.rotation = cameraPos.rotation;
	}

    /// <summary>
    /// Update this instance, once per frame
    /// </summary>
	void Update() 
	{
		transform.position = transform.parent.position + new Vector3(0, 3.0f, 0.5f);
		transform.rotation = Quaternion.Euler (cameraPos.rotation.eulerAngles.x, cameraPos.rotation.eulerAngles.y, 0);
	}
}
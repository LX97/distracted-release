using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Debugger : MonoBehaviour {
    public Renderer rend { get; set; }
    //public Transform end { get; set; }
    //public NavMeshAgent agent { get; set; }
    //GameObject[] objs;
    // Use this for initialization
    void Start () {
        /*
        agent = GetComponent<NavMeshAgent>();
        objs = GameObject.FindGameObjectsWithTag("Agent");
        end = objs[1].GetComponent<Transform>();
        agent.SetDestination(end.position);
        */
        rend = gameObject.GetComponent<Renderer>();
        rend.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        /*
        end = objs[0].GetComponent<Transform>();
        agent.destination = end.position;
        */
    }
}

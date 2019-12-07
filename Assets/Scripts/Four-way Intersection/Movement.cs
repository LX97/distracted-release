using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Movement : MonoBehaviour {

    public Transform[] points;
    private int destPoint = 0;
    private NavMeshAgent agent;
    public GameObject traffic;
    public GameObject pedestrian;
    private TrafficLights trafficLight;
    private Transform current;
    public bool isPedestrian;
    void Start()
    {
        current = gameObject.GetComponent<Transform>();
        trafficLight = traffic.GetComponent<TrafficLights>();
        agent = GetComponent<NavMeshAgent>();

        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        agent.autoBraking = false;
    }


    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (points.Length == 0)
            return;

        // Set the agent to go to the currently selected destination.
        if(destPoint <= points.Length-1)
        {
            agent.destination = points[destPoint].position;

            // Choose the next point in the array as the destination,
            // cycling to the start if necessary.
            destPoint = (destPoint + 1);
        }
        else
        {
            agent.autoBraking = false;
            gameObject.SetActive(false);
        }
    }
    void MoveCars()
    {
        float maxRange = 2.25f;
        RaycastHit hit;
       

        string state = trafficLight.currentState;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            switch (state)
            {
                case "RED":
                    agent.isStopped = true;
                    break;
                case "YELLOW":
                    agent.isStopped = false;
                    GotoNextPoint();
                    break;
                case "GREEN":
                    for(int i=0; i < 5; i++)
                    {
                        if (Vector3.Distance(pedestrian.GetComponent<Transform>().position, current.position) < maxRange)
                        {
                            if (Physics.Raycast(pedestrian.GetComponent<Transform>().position, (current.position - pedestrian.GetComponent<Transform>().position), out hit, maxRange))
                            {
                                agent.isStopped = true;
                            }
                        }
                    }

                    agent.isStopped = false;
                    GotoNextPoint();
                    break;
            }
        }
    }
    
    void MovePedestrian()
    {
        string state = trafficLight.currentState;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            switch (state)
            {
                case "RED":
                    agent.isStopped = true;
                    break;
                case "YELLOW":
                    agent.isStopped = false;
                    GotoNextPoint();
                    break;
                case "GREEN":
                    agent.isStopped = false;
                    GotoNextPoint();
                    break;
            }
        }
    }

    void Update()
    {
        if (isPedestrian)
        {
            MovePedestrian();
        }
        else
        {
            MoveCars();
        }
        
    }
}

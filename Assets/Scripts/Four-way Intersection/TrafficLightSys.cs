using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightSys : MonoBehaviour {
    public float timer = 60;
    GameObject[] lights1;
    GameObject[] lights2;
    TrafficLights[] traffic1;
    TrafficLights[] traffic2;
    int traffic1state = 1;
    int traffic2state = 0;
    // Use this for initialization
    void Start () {
        lights1 = GameObject.FindGameObjectsWithTag("TrafficLight1");
        TrafficLights[] traffic1dummy = new TrafficLights[lights1.Length];
        lights2 = GameObject.FindGameObjectsWithTag("TrafficLight2");
        TrafficLights[] traffic2dummy = new TrafficLights[lights2.Length];
        for(int i=0; i<lights1.Length; i++)
        {
            traffic1dummy[i] = lights1[i].GetComponent<TrafficLights>();
            traffic2dummy[i] = lights2[i].GetComponent<TrafficLights>();
        }
        traffic1 = traffic1dummy;
        traffic2 = traffic2dummy;
	}
	
	// Update is called once per frame
	void Update () {
        timer -= Time.deltaTime;
        Debug.Log(timer);
        TrafficState(traffic1state, timer, traffic1);
        TrafficState(traffic2state, timer, traffic2);
        if (timer <= 0)
        {
            timer = 60;
            int foo = traffic1state;
            traffic1state = traffic2state;
            traffic2state = foo;
        }
    }
    public void TrafficState(int state, float timer, TrafficLights[] lights)
    {
        switch (state)
        {
            case 1: //RRGY
                if(timer<=60 && timer > 30)
                {
                    foreach(TrafficLights light in lights)
                    {
                        light.ChangeColor("RED");
                    }
                }
                else if(timer <= 30 && timer > 15)
                {
                    foreach(TrafficLights light in lights)
                    {
                        light.ChangeColor("GREEN");
                    }
                }
                else if(timer<=15 && timer > 0)
                {
                    foreach(TrafficLights light in lights)
                    {
                        light.ChangeColor("YELLOW");
                    }
                }
                break;
            case 0: //GYRR
                if (timer <= 60 && timer > 45)
                {
                    foreach (TrafficLights light in lights)
                    {
                        light.ChangeColor("GREEN");
                    }
                }
                else if(timer<=45 && timer > 30)
                {
                    foreach (TrafficLights light in lights)
                    {
                        light.ChangeColor("YELLOW");
                    }
                }
                else if (timer <= 30 && timer > 0)
                {
                    foreach (TrafficLights light in lights)
                    {
                        light.ChangeColor("RED");
                    }
                }
                break;
        }
    }
}

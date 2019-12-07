using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLights : MonoBehaviour {
    //Function to change color
    Renderer rend;
    public string currentState;
    
    private void Start()
    {
        rend = GetComponent<Renderer>();
    }
    public void ChangeColor(string state)
    {
        currentState = state;
        switch (state)
        {
            case "RED":
                rend.material.color = Color.red;
                break;
            case "GREEN":
                rend.material.color = Color.green;
                break;
            case "YELLOW":
                rend.material.color = Color.yellow;
                break;
        }
    }
    //keep track of state
}

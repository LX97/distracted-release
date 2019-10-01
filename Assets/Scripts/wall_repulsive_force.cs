﻿

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class wall_repulsive_force : MonoBehaviour {

	GameObject[] agents;
	Rigidbody test;
    Transform test2;
	Vector3 x_positive= new Vector3(1.0f, 0.0f, 0.0f);
	Vector3 x_negative= new Vector3(-1.0f, 0.0f, 0.0f);
	Vector3 z_positive= new Vector3(0.0f, 0.0f, 1.0f);
	Vector3 z_negative= new Vector3(0.0f, 0.0f, -1.0f);

    Vector3 wall_normal;
    Vector3 distance_wa;

    Vector3 line_start, line_end;

    box box1;

    struct box
	{
		public float xmax, zmax, xmin, zmin;
	}

	// Use this for initialization
	void Start () {

        agents = GameObject.FindGameObjectsWithTag("Agent");

		box1.xmin = transform.position.x + transform.localScale.x/2.0f; 
		box1.xmax = transform.position.x - transform.localScale.x/2.0f; 

		box1.zmax = transform.position.z + transform.localScale.z/2.0f; 
		box1.zmin = transform.position.z - transform.localScale.z/2.0f; 

	
	}
	
	// Update is called once per frame
	void FixedUpdate() {


		foreach (GameObject agent in agents) {

			test = agent.GetComponent<Rigidbody>();
            test2 = agent.GetComponent<Transform>();

            wall_normal = calc_wall_normal(test2, box1);
            Pair<Vector3, Vector3> line_p = calcWallPointsFromNormal(box1, wall_normal);
            Vector3 n_w = test2.position - closestPointLineSegment(line_p.First, line_p.Second, test2.position);

			Debug.DrawLine (line_p.First, line_p.Second, Color.blue);

			Debug.DrawLine (new Vector3(5, 0, box1.zmin), new Vector3(5, 0, box1.zmax), Color.red);
            n_w.y = 0;

            float d_w = n_w.sqrMagnitude;

            if (d_w < 0.6f * 0.6f)    //0.6 : walldistance
            {
                d_w = (float)Math.Sqrt((double)d_w);
                if (d_w > 0)
                    n_w /= d_w;

                float distMinRadius = (d_w - 0.5f) < 0.001f ? 0.001f : d_w - 0.5f;     // radius =0.5
                float calc_1 =  (0.6f-d_w) / (float)Math.Pow((double)distMinRadius, (double)2.0f);   // wall steefness =2.0f

				Debug.Log(calc_1);
                
				test.AddForce(n_w*calc_1);
				ShowGoldenPath_Distraction distractionScript = agent.GetComponent<ShowGoldenPath_Distraction>();
				if (distractionScript != null){
					distractionScript.PayAttention();
				}
            }

        }
    }

    Vector3 calc_wall_normal(Transform rb, box box1)
    {
        
        if (rb.position.x > box1.xmax)
        {
            if (rb.position.z > box1.zmax)
            {
                if (Mathf.Abs(rb.position.z - box1.zmax) >
                    Mathf.Abs(rb.position.x - box1.xmax))
                {
                    return z_positive; 
                }
                else
                {
                    return x_positive;
                }

            }
            else if (rb.position.z < box1.zmin)
            {
                if (Mathf.Abs(rb.position.z - box1.zmin) >
                    Mathf.Abs(rb.position.x - box1.xmax))
                {
                    return z_negative; 
                }
                else
                {
                    return x_positive;
                }

            }
            else
            { // in between zmin and zmax
                return x_positive;
            }

        }
        else if (rb.position.x < box1.xmin)
        {
            if (rb.position.z > box1.zmax)
            {
                if (Mathf.Abs(rb.position.z - box1.zmax) >
                    Mathf.Abs(rb.position.x - box1.xmin))
                {
                    return z_positive;
                }
                else
                {
                    return x_negative;
                }

            }
            else if (rb.position.z < box1.zmin)
            {
                if (Mathf.Abs(rb.position.z - box1.zmin) >
                    Mathf.Abs(rb.position.x - box1.xmin))
                {
                    return z_negative; 
                }
                else
                {
                    return x_negative;
                }

            }
            else
            { // in between zmin and zmax
                return x_negative;
            }
        }
        else // between xmin and xmax
        {
            if (rb.position.z > box1.zmax)
            {
                return z_positive;
            }
            else if (rb.position.z < box1.zmin)
            {
                return z_negative;
            }
            else
            { // What do we do if the agent is inside the wall?? Lazy Normal
                return x_positive;   // This is not accurate.
            }
        }


    }
   
    Pair<Vector3, Vector3> calcWallPointsFromNormal(box box1, Vector3 normal)
    {
        Pair<Vector3, Vector3> line = new Pair<Vector3, Vector3>();

        if (normal == z_positive)
        { 
            line.find_line(new Vector3(box1.xmin, 0, box1.zmax), new Vector3(box1.xmax,0, box1.zmax));
            return line;
        }
        else if (normal == z_negative)
        {
            line.find_line(new Vector3(box1.xmin, 0, box1.zmin), new Vector3(box1.xmax,0 , box1.zmin));
            return line;
        }
        else if (normal == x_positive)
        {
            line.find_line(new Vector3(box1.xmax, 0, box1.zmin), new Vector3(box1.xmax,0, box1.zmax));
            return line;
        }
        else
        {
            line.find_line(new Vector3(box1.xmin, 0, box1.zmin), new Vector3(box1.xmin,0 , box1.zmax));
            return line;
        }

    }
 
    Vector3 closestPointLineSegment(Vector3 line_start,Vector3 line_end, Vector3 p)
	{
        //    return line_start;

        float dota = (p.x - line_start.x) * (line_end.x - line_start.x) + (p.z - line_start.z) * (line_end.z - line_start.z);
		if (dota <= 0) // point line_start is closest to p
			return line_start;

		float dotb = (p.x - line_end.x) * (line_start.x - line_end.x) + (p.z - line_end.z) * (line_start.z - line_end.z);
		if (dotb <= 0) // point line_end is closest to p
			return line_end;
	
		// find closest point
		float slope = dota / (dota + dotb);

        return line_start + (line_end - line_start)*slope;
	}


    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public void find_line(T a, U b)
        {
            this.First = a;
            this.Second = b;
        }

        public T First { get; set; }
        public U Second { get; set; }
    };


}

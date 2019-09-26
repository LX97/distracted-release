using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent_repulsive : MonoBehaviour
{

    GameObject[] neighbor_agents;
    Rigidbody test;
    Transform test2;
    Rigidbody rb;
    Vector3 force_noise;
    public bool noise = false;

    bool collision_ = false;
    int count = 0;
    //bool noise = false;
    float combinedRadius;
    float t;
    float _cosFov;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        neighbor_agents = GameObject.FindGameObjectsWithTag("Agent");
        combinedRadius = 0.1f + 0.5f + 0.5f; // two agents radius plus personal distance 0.1;
        _cosFov = Mathf.Cos((0.5f * 3.1415892f * 200.0f) / 180.0f);   // agent field of view: 200 degree
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {

        MultiSortedDictionary<float, Vector3> t_pairs
            = new MultiSortedDictionary<float, Vector3>();

        collision_ = false;
        count = 0;


        if (noise== true)
        {
            float rand_number = Random.Range(0.0f, 32767.0f);
            float rand_number2 = Random.Range(0.0f, 32767.0f);
            float angle = 2.0f * 3.141592f * rand_number / 32767.0f;
            float distance = (rand_number2 * 0.001f) / 32767.0f;

            force_noise.x = Mathf.Cos(angle) * distance;
            force_noise.z = Mathf.Sin(angle) * distance;
            force_noise.y = 0;
            rb.AddForce(force_noise);

        }



        foreach (GameObject agent in neighbor_agents)
        {
            test = agent.GetComponent<Rigidbody>();
            test2 = agent.GetComponent<Transform>();
            // ignore own tag and far distance agent
            if ((transform.position - test2.position).magnitude > 0 && (transform.position - test2.position).magnitude < 6)
            {
                Vector3 w = test2.position - transform.position;
                if (w.sqrMagnitude < combinedRadius * combinedRadius)
                {
                    t = 0;             
                }
                else
                {
                    Vector3 relDir = w.normalized;
                    if ((relDir.x * rb.velocity.normalized.x) + (relDir.y * rb.velocity.normalized.y) + (relDir.z * rb.velocity.normalized.z) < _cosFov)
                        continue;
                    else
                    {
                        Vector3 desVel = rb.velocity;
                        float tc = rayIntersectsDisc(transform.position, test2.position, desVel - test.velocity, combinedRadius);
                        t = tc;
                    }
                }

                if (t>=0 && t <4.0f )
                {
                    Vector3 forceDir = transform.position + rb.velocity * t - test2.position - test.velocity * t; // pointing away from collision position
                    collision_ = true;
                    t_pairs.Add(t, forceDir);
                }
        
            }
        }


        if(collision_==true)
        {

            foreach (float t_ in t_pairs.Keys)
            {
                foreach (Vector3 force_Dir in t_pairs[t_])
                {
                    Vector3 force_ = force_Dir;
                    force_.y = 0;
                    float forceDist = force_Dir.magnitude;
                    if (forceDist > 0)
                        force_ /= forceDist;

                    float collisionDist = Mathf.Max(forceDist - 0.5f - 0.5f, .0f); // distance between their cylindrical bodies at the time of collision
                    float D = Mathf.Max(1.3f * t_ + collisionDist, 0.001f); // D = input to evasive force magnitude piecewise function

                    float mag;
                    if (D < 0.5f + 0.1f)
                    {
                        mag = 1.0f * 0.6f / D;
                    }
                    else if (D < 4.0f)
                    {
                        mag = 1.0f;
                    }
                    else if (D < 4.0f * 2.0f)
                    {
                        mag = 1.0f * (4.0f * 2.0f - D) / (4.0f * 2.0f - 4.0f);
                    }
                    else
                    {
                        mag = 0.0f;   // magnitude is zero
                    }

                    mag *=  Mathf.Pow(  (t_==0 ? 1.0f : 0.8f), count++);

					ShowGoldenPath_Distraction distractionScript = GetComponent<ShowGoldenPath_Distraction>();
					if (distractionScript == null || distractionScript.checkDistracted() == false){
						rb.AddForce(mag/2 * force_);
					}
                }
            }

 
        }

  


        
    }


    class MultiSortedDictionary<TKey, TValue>
    {
        public MultiSortedDictionary()
        {
            dic = new SortedDictionary<TKey, List<TValue>>();
        }

        public MultiSortedDictionary(IComparer<TKey> comparer)
        {
            dic = new SortedDictionary<TKey, List<TValue>>(comparer);
        }

        public void Add(TKey key, TValue value)
        {
            List<TValue> list;

            if (dic.TryGetValue(key, out list))
            {
                list.Add(value);
            }
            else
            {
                list = new List<TValue>();
                list.Add(value);

                dic.Add(key, list);
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                return this.dic.Keys;
            }
        }

        public List<TValue> this[TKey key]
        {
            get
            {
                List<TValue> list;

                if (this.dic.TryGetValue(key, out list))
                {
                    return list;
                }
                else
                {
                    return new List<TValue>();
                }
            }
        }

        private SortedDictionary<TKey, List<TValue>> dic = null;
    }

    float rayIntersectsDisc(Vector3 Pa, Vector3 Pb, Vector3 v, float radius)
    {
        float t;
        Vector3 w = Pb - Pa;
        float a = ((v.x * v.x) + (v.y * v.y) + (v.z * v.z));//v*v;
        float b = ((w.x * v.x) + (w.y * v.y) + (w.z * v.z));//w*v;
        float c = ((w.x * w.x) + (w.y * w.y) + (w.z * w.z)) - (radius * radius);//w*w - radius*radius;
        float discr = b * b - a * c;
        if (discr > 0.0f)
        {
            t = (b - Mathf.Sqrt(discr)) / a;
            if (t< 0)
                t = 999999.0f;
        }
        else
            t = 999999.0f;
    
        return t;
    }
    
}

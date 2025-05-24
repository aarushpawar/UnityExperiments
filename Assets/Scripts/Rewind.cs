using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;

public class Rewind : MonoBehaviour
{
    static bool rewindInput = false;
    bool receivedRewind = false;
    ArrayList<Transform> transforms = new System.Collections.ArrayList<Transform>();
    float cooldown = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cooldown -= Time.deltaTime;

        if (cooldown <= 0f)
        {
            transforms.Add(transform);
            cooldown = 1f;

            if (transforms.Count > 10)
            {
                transforms.RemoveAt(0);
            }
        }

        if (rewindInput && !receivedRewind)
        {
            receivedRewind = true;
            Transform newTransform = transforms[0];

            transform.position = newTransform.position;
            transform.rotation = newTransform.rotation;

            transforms.Clear();
            transforms.Add(transform);
        }

        if (!rewindInput && receivedRewind)
        {
            receivedRewind = false;
        }
    }
}

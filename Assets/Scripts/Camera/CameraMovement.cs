﻿using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    private GameObject pod;
    private float smoothSpeed = 200.0f;
    public bool startOfGame = true;
    public float scrollSpeed = 0.05f;
    private Vector3 startPos;
    private float init;
    private float fast;
    private bool explosion;
    float dest;
    private float min;
    private float max;
    void Awake()
    {
        pod = GameObject.FindGameObjectWithTag("Player");
        startPos = pod.transform.position;
        init = transform.position.y;
        //fast = set maxSpeed;
        explosion = false;
        min = -0.21f;
        max = 0.21f;
        if (!startOfGame)
        {
            Vector3 temp = pod.transform.position;
            temp.z = -8.9f;
            transform.position = temp;
        }
    }
    void LateUpdate()
    {
        if (startOfGame)
        {
            pod.transform.position = startPos;
            init += 1.0f;
            dest = Mathf.Lerp(transform.position.y, init, scrollSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, dest, transform.position.z);
            if ((transform.position.y == startPos.y) || (transform.position.y > startPos.y - 1.0f))
            {
                startOfGame = false;
            }
        }
        else if (pod)
        {
            // if ((pod.transform.position.y - 1.0) < transform.position.y)
            if (transform.position.y <= (startPos.y + 10))
            {
                Vector3 newPos;
                if ((explosion))/*pod.speed >= fast) || explosion)*/
                {
                    newPos.x = Mathf.PerlinNoise(transform.position.x * Time.time * min, transform.position.x * Time.time * max);
                    newPos.y = Mathf.PerlinNoise(transform.position.y * Time.time * min, transform.position.y * Time.time * max);
                    newPos.z = transform.position.z;
                    dest = Mathf.Lerp(newPos.y, pod.transform.position.y, smoothSpeed * Time.deltaTime);
                    transform.position = new Vector3(newPos.x, dest - 1.0f, newPos.z);
                }
                else
                {
                    dest = Mathf.Lerp(transform.position.y, pod.transform.position.y, smoothSpeed * Time.deltaTime);
                    transform.position = new Vector3(transform.position.x, dest - 1.0f, transform.position.z);
                }
            }
        }
    }
}
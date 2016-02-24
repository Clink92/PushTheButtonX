﻿using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {

    private ObjectState m_state;

    [SerializeField]
    protected LineRenderer _beam;

	void Awake () {
        m_state = GetComponent<ObjectState>();
	}
	
	void Update () {
        m_state.UpdateState();
	}

    void OnTriggerStay2D(Collider2D col)
    {
        Debug.Log("Colliding with something");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Colliding with something");
    }

}

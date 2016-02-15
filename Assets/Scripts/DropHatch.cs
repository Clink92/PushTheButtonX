﻿using UnityEngine;
using System.Collections;

public class DropHatch : MonoBehaviour 
{
    [SerializeField]
    private Rigidbody2D m_podRigidbody;

    [SerializeField]
    private Vector2 m_ejectForce;

    [SerializeField]
    private ParticleSystem m_motherShipExplosion;
    ParticleSystem.EmissionModule em;

    private Collider2D m_collider2D;

	private void Awake() 
	{
        m_collider2D = GetComponent<Collider2D>();
        em = m_motherShipExplosion.emission;

        em.enabled = false;
        
    }
	
	private void Update() 
	{
        if (!m_collider2D.enabled)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EjectPod();
        }
	}

    private void EjectPod()
    {
        m_collider2D.enabled = false;
        if (m_podRigidbody != null)
        {
            m_podRigidbody.AddForce(m_ejectForce);
            DestroyMotherShip();
        }
    }

    private void DestroyMotherShip()
    {
        m_motherShipExplosion.time = 0.0f;
        em.enabled = true;
        m_motherShipExplosion.loop = false;
        
        transform.root.gameObject.SetActive(false);
    }
}

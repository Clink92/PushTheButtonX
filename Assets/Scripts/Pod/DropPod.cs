﻿using UnityEngine;
using System.Collections;

public class DropPod : MonoBehaviour
{
    [SerializeField]
    private Thruster m_thruster;

    [SerializeField]
    private float m_maxLandingVelocity = 1.0f;

    [SerializeField, Range(0.0f, m_maxFuel)]
    private float m_fuel = m_maxFuel;

    [SerializeField, Range(0.0f, m_maxFuel)]
    private float m_fuelUsePerSecond = 1.0f;

    public const float m_maxFuel = 100.0f;

    [SerializeField]
    private float m_invincibleTime = 1.0f;

    // E-man: Audio skit
    [SerializeField]
    private AudioSource audioFlame;

    [SerializeField]
    private AudioSource audioFireShoot;

    [SerializeField]
    private AudioSource audioThrustStart;

    [SerializeField]
    private AudioSource audioThrustEnd;

    [SerializeField]
    private AudioSource audioExplosion;

    [SerializeField]
    private AudioSource audioHit;

    [SerializeField]
    private AudioSource audioLanding;

    [SerializeField]
    private AudioSource audioFalling;

    [SerializeField]
    private AudioSource audioShieldUp;

    [SerializeField]
    private AudioSource audioFuelEmpty;

    [SerializeField]
    private float rattlingThreshold = -10;

    private AudioSource[] allAudioSources;
    private bool isPaused;

    [Space(10)]

    [SerializeField]
    private AnimationCurve m_gravityCurve;

    private bool m_shield = false;
    
    private float m_startAltitude;

    private Transform m_transform;
    private Rigidbody2D m_rigidbody2D;
    private BoxCollider2D m_boxCollider2D;

    private RayShooter2D m_rayShooter2D;

    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private LayerMask m_groundMask;
    
    // Raycast members
    [SerializeField]
    private int m_totalVerticalRays = 5;
    private float m_verticalRayInterval;
    private float m_rayDistance = 0.5f;
    private float m_skinWidth = 0.1f;
    private Vector2 m_bottomLeft;

    // E-man: Get ThrusterFlame object on awake

    private AudioSource[] audioSources;

    private ObjectState m_objectState;

    private Transform m_meshTransform;
    [SerializeField]
    private float m_shakeAmp = 1.0f;
    [SerializeField]
    private float m_shakeFreq = 1.0f;

    [SerializeField]
    private GameObject m_shieldObj;

    [SerializeField]
    private float ShieldFlickerFrequency;

    // E-man
    

    private bool setToDestroy = false;

    [SerializeField]
    private GameObject podMesh;

    private GameObject grader;
    private Grade grade;

    private Vector3 StartPosition;

    private float fuelIncrease = 10.0f;

    [SerializeField]
    private GameObject thrusterFlame;

    [SerializeField]
    private GameObject thrusterSmoke;

    [SerializeField]
    private GameObject dieExplosion;

    [SerializeField]
    private GameObject shieldShatterParticle;

    private void Awake() 
	{
        m_transform = transform;
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_thruster = GetComponent<Thruster>();
        m_boxCollider2D = GetComponent<BoxCollider2D>();

        m_rayShooter2D = new RayShooter2D();

        m_startAltitude = Altitude();
        
        m_shieldObj.SetActive(false);

        grader = GameObject.Find("Grading");
        grade = grader.GetComponent<Grade>();

        // Make sure that these are set to false
        thrusterFlame.SetActive(false);
        thrusterSmoke.SetActive(false);
        dieExplosion.SetActive(false);
        shieldShatterParticle.SetActive(false);

        // E-man - End

        // Set up raycast collision check
        Vector2 size = m_boxCollider2D.size;
        Vector3 scale = transform.localScale;
        float width = size.x * Mathf.Abs(scale.x) - (2.0f * m_skinWidth);
        m_verticalRayInterval = width / (m_totalVerticalRays - 1);

        audioSources = GetComponents<AudioSource>();
        m_objectState = GetComponent<ObjectState>();

        m_meshTransform = m_transform.FindChild("Ship");

        StartPosition = transform.position;
        /*
        meshRenderer = GameObject.Find("default").GetComponent<MeshRenderer>();
        if (meshRenderer)
        {
            Debug.Log("Yo dude meshrenderer has been created");
        }*/

        allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        isPaused = false;
    }
    
	private void FixedUpdate()
	{
        m_rigidbody2D.gravityScale = CalculateGravityScale();
        
        m_objectState.UpdateState();
    }

    private void Update()
    {
        if(setToDestroy)
        {
            if(!audioExplosion.isPlaying)
            {
                setToDestroy = false;
                m_rigidbody2D.isKinematic = false;
                GameManager.Instance.ChangeState(gameState.LOSE);                
            }
        }
               
        // E-man
        checkForFallingNoise();
        checkForPause();
    }

    public float Altitude()
    {
        return m_transform.position.y;
    }

    public float Velocity()
    {
        return m_rigidbody2D.velocity.y;
    }

    public float Fuel()
    {
        return m_fuel;
    }
    
    public void AddFuel(float amount)
    {
        m_fuel = Mathf.Clamp(m_fuel + amount, 0.0f, m_maxFuel);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        string layerName = LayerMask.LayerToName(other.layer);

        if( string.Equals(layerName, "Ground") )
        {
            bool landed = LandingSequence(collision);
            if(landed)
            {
                audioLanding.time = 2;
                audioLanding.Play();

                LevelComplete();
            }
            else
            {
                GameOver();
            }
        }
        
        if (string.Equals(layerName, "Platform"))
        {
            bool landed = LandingSequence(collision);
            if (!landed || m_fuel <= 0.0f)
            {
                GameOver();
            }
        }

        if (string.Equals(layerName, "Enemy"))
        {
            EnemyHit(); 
        }

        if(string.Equals(layerName, "Obstacle"))
        {
            Debug.Log("Cloud explosion");
            if (m_rigidbody2D.velocity.y > 0)
            {
                GameOver();
                Debug.Log("Cloud explosion 4 real");
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D c)
    {
        GameObject other = c.gameObject;
        string layerName = LayerMask.LayerToName(other.layer);
        if (string.Equals(layerName, "ShieldUp"))
        {
            AddShield();
        }
        if (string.Equals(layerName, "FuelUp"))
        {
            AddFuel(fuelIncrease);
        }
    }
    public bool LandVelocityCheck()
    {
        // If current vertical velocity is within the allowed landing velocity
        return (m_maxLandingVelocity > Mathf.Abs(m_rigidbody2D.velocity.y));
    }

    private void CalculateRayOrigin()
    {
        // Box values
        Vector2 position = (Vector2)transform.position;
        Vector2 scale = (Vector2)transform.localScale;
        Vector2 size = m_boxCollider2D.size;
        Vector2 offset = m_boxCollider2D.offset;
        Vector2 center = position + new Vector2(offset.x * scale.x, offset.y * scale.y);

        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;

        m_bottomLeft = center + Vector2.down * (halfHeight - m_skinWidth) + Vector2.left * (halfWidth - m_skinWidth);
    }

    private bool LandingSequence(Collision2D collision)
    {
        bool land = LandVelocityCheck();

        // If it is a platform, check if we hit from above
        string objLayerName = LayerMask.LayerToName(collision.gameObject.layer);
        if (land && string.Equals(objLayerName, "Platform"))
        {
            CalculateRayOrigin();
            land = LandFromAbove(collision.transform);
        }

        return land;
    }
    
    private bool LandFromAbove(Transform landTarget)
    {
        // Ray origin
        Vector2 rayOrigin = m_bottomLeft;

        // Ray Direction
        Vector2 rayDirection = Vector2.down;

        // Length of ray
        float rayDistance = m_rayDistance + m_skinWidth;

        Vector2 shootFrom = rayOrigin;
        bool fromAbove = false;
        for (int i = 0; i < m_totalVerticalRays; i++)
        {
            // Point to shoot this ray from
            shootFrom.x = rayOrigin.x + m_verticalRayInterval * i;
            shootFrom.y = rayOrigin.y;

            m_rayShooter2D.Shoot(shootFrom, rayDirection, m_groundMask, rayDistance);
            //Debug.DrawRay(shootFrom, rayDirection * rayDistance, Color.red);

            if (!m_rayShooter2D.Hit)
            {
                // No hit, continue to next ray
                continue;
            }
            else if (landTarget == m_rayShooter2D.IntersectedTransform())
            {
                // Landing from above. Stop searching
                fromAbove = true;
                break;
            }
        }
        return fromAbove;
    }
    
    public void EnemyHit()
    {
        StartCoroutine(EnemyHitRoutine());
    }

    private IEnumerator EnemyHitRoutine()
    {
        if (m_shield)
        {
            // E-man: Play hit sound
            if (!audioHit.isPlaying)
            {
                audioHit.Play();
            }

            grade.GotHit();

            float flickerTime = 0;

            // Deactivate the shield mesh
            m_shieldObj.SetActive(false);

            shieldShatterParticle.SetActive(true);
            while (flickerTime < m_invincibleTime)
            { 
                meshRenderer.material.SetColor
                    ("_Color", new Color(0.74f, 0.74f, 0.74f, 0.1f));
                yield return new WaitForSeconds(ShieldFlickerFrequency);

                meshRenderer.material.SetColor 
                    ("_Color", Color.white);
                yield return new WaitForSeconds(ShieldFlickerFrequency);
                flickerTime += 2 * ShieldFlickerFrequency;
            }

            // Set shield to false so we can get damaged
            m_shield = false;
        }
        else
        {
            GameOver();
        }
        yield return null;
    }

    private void GameOver()
    {
        // E-man: Add explosion
        dieExplosion.SetActive(true);
        dieExplosion.transform.SetParent(null);

        // Explosion sound
        audioExplosion.Play();

        setToDestroy = true;
        podMesh.SetActive(false);

        m_boxCollider2D.enabled = false;
        m_rigidbody2D.isKinematic = true;
    }

    private void LevelComplete()
    {
        GameManager.Instance.ChangeState(gameState.WIN);
    }

    private void AddShield()
    {
        // Already have shield. Do nothing
        if (m_shield)
            return;

        m_shield = true;
        m_shieldObj.SetActive(true);

        // E-man 
        audioShieldUp.Play();
    }
    
    public float GravityScale()
    {
        return m_rigidbody2D.gravityScale;
    }

    private float CalculateGravityScale()
    {
        float altitude = Mathf.Min(Altitude(), m_startAltitude);
        float gravityScale = m_gravityCurve.Evaluate(1.0f - (altitude / m_startAltitude));
        return gravityScale;
    }

    public void FireThruster(bool buttonPressed)
    {
        if (buttonPressed && m_fuel > 0.0f)
        {
            if(Altitude() <= StartPosition.y)
            {
                m_rigidbody2D.AddForce(m_thruster.ThrustForce());
            }

            float used = m_fuelUsePerSecond * Time.deltaTime;
            m_fuel -= used;
            grade.FuelUsage(used);
            
            // Enable flame
            thrusterFlame.SetActive(true);

            // Shake mesh
            Vector3 meshPos = m_meshTransform.position;
            meshPos.x = meshPos.x + (m_shakeAmp * Mathf.Sin(m_shakeFreq * Time.time)) * GravityScale();
            m_meshTransform.position = meshPos;            

            if (!audioFlame.isPlaying && !audioFireShoot.isPlaying)
            {
                startThrusterAudio();
            }
        }
        else if(buttonPressed && m_fuel <= 0.0f)
        {
            // Enable flame
            thrusterSmoke.SetActive(true);
            // NOTE: HACK fix this shit
            thrusterFlame.SetActive(false);            

            // Play engine stutter sound
            if (!audioFuelEmpty.isPlaying)
            {
                stopThrusterAudio();
                audioFuelEmpty.Play();
            }
        }   
        else
        {
            // Disable flame
            thrusterFlame.SetActive(false);
            thrusterSmoke.SetActive(false);

            // Reset mesh shake
            Vector3 meshPos = m_meshTransform.position;
            meshPos.x = transform.position.x;
            m_meshTransform.position = meshPos;

            if (audioFlame.isPlaying || audioFireShoot.isPlaying || audioFuelEmpty.isPlaying)
            {
                // Play end thruster sound
                audioThrustEnd.time = 0.2f;
                audioThrustEnd.Play();

                // Stop other sounds
                stopThrusterAudio();
            }
        }     
    }

    public float GetMaxLandingVelocity()
    {
        return m_maxLandingVelocity;
    }

    public bool isThrusting()
    {
        return thrusterFlame.activeSelf;
    }

    // E-man
    private void checkForFallingNoise()
    {
        if(Velocity() <= rattlingThreshold)
        {
            if (!audioFalling.isPlaying)
            {                
                audioFalling.Play();                
            }
        }
        else
        {
            audioFalling.Stop();
        }
    }

    private void checkForPause()
    {
        if (PauseMenu.paused)
        {
            isPaused = true;            

            foreach (AudioSource audioSource in allAudioSources)
            {
                audioSource.Pause();
            }
        }
        else if(!PauseMenu.paused && isPaused)
        {
            isPaused = false;

            foreach (AudioSource audioSource in allAudioSources)
            {
                audioSource.UnPause();
            }
        }
    }

    private void startThrusterAudio()
    {
        audioFlame.Play();
        audioFireShoot.Play();
        audioThrustStart.Play();
    }

    private void stopThrusterAudio()
    {
        audioFlame.Stop();
        audioFireShoot.Stop();
        audioThrustStart.Stop();
        audioFuelEmpty.Stop();
        audioFuelEmpty.Stop();
    }
}

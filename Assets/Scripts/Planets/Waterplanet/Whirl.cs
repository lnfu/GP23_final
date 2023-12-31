using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirl : MonoBehaviour
{
    /// <summary>  </summary>
    public float GRAVITY_PULL    = 40.0f;
    /// <summary>  </summary>
    private float  SWIRLSTRENGTH   = 2.0f;

    // ------------------------------------------------
    /// <summary>  </summary>
    private float               _gravityRadius  = 1.0f;    
    /// <summary>  </summary>
    private List<Rigidbody2D>   _rigidBodies    = new List<Rigidbody2D>();

    public GameObject waterplanet;
    private float randomGeneratetime=3.0f;
    // ------------------------------------------------

#if UNITY_EDITOR     



    void Start()
    {
        transform.position = Random.insideUnitCircle * 55 + new Vector2(waterplanet.transform.position.x, waterplanet.transform.position.y);
    }


    void Update()
    {
        if( Application.isPlaying == false )
        {
            _gravityRadius = GetComponent<CircleCollider2D>().radius;
        }
        randomGeneratetime-=Time.deltaTime;

        if(randomGeneratetime<0)
        {
            transform.position = Random.insideUnitCircle * 55 + new Vector2(waterplanet.transform.position.x, waterplanet.transform.position.y);
            randomGeneratetime=3f;
        }

    }
#endif 

    private void LateUpdate()
    {    
        UpdateBlackHole();
    }


    void OnTriggerEnter2D(Collider2D in_other)
    {
        if ( in_other.attachedRigidbody != null && _rigidBodies != null && in_other.name=="Player")
        {                
            //to get them nice and swirly, use the perpendicular to the direction to the vortex
            Vector3 direction = transform.position - in_other.attachedRigidbody.transform.position;
            var tangent = Vector3.Cross(direction, Vector3.forward).normalized * SWIRLSTRENGTH;

            in_other.attachedRigidbody.velocity = tangent;            
            _rigidBodies.Add( in_other.attachedRigidbody );
        }
    }
    void OnTriggerExit2D(Collider2D in_other)
    {
        if ( in_other.attachedRigidbody != null && _rigidBodies != null && in_other.name=="Player")
        {                
            //to get them nice and swirly, use the perpendicular to the direction to the vortex
                  
            _rigidBodies.Remove( in_other.attachedRigidbody );
        }
    }

    private void UpdateBlackHole()
    {
        if( _rigidBodies != null )
        {
            for (int i = 0; i < _rigidBodies.Count; i++)
            {
                if( _rigidBodies[i] != null )
                {
                    CalculateMovement( _rigidBodies[i] );
                }
            }
        }
    }

    

    private void CalculateMovement(Rigidbody2D in_rb)
    {
        float distance = Vector3.Distance(transform.position,in_rb.transform.position); 
        float gravityIntensity =distance/ _gravityRadius;

        in_rb.AddForce((transform.position - in_rb.transform.position)*gravityIntensity * in_rb.mass* GRAVITY_PULL* Time.deltaTime);
        
        in_rb.drag += 0.0001f;
        
        Debug.DrawRay(in_rb.transform.position, transform.position - in_rb.transform.position);


        if( distance <= 0.1f )
        {
            _rigidBodies.Remove( in_rb );

            Destroy( in_rb.gameObject );
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]


public class BlackholeOuterfield : MonoBehaviour
{
    /// <summary>  </summary>
    public float GRAVITY_PULL    = 40.0f;
    /// <summary>  </summary>
    public float  SWIRLSTRENGTH   = 2.0f;

    // ------------------------------------------------
    /// <summary>  </summary>
    private float               _gravityRadius  = 150.0f;    
    /// <summary>  </summary>
    private List<Rigidbody2D>   _rigidBodiesblackhole    = new List<Rigidbody2D>();
    // ------------------------------------------------

#if UNITY_EDITOR     
    void Update()
    {
        if( Application.isPlaying == false )
        {
            _gravityRadius = GetComponent<CircleCollider2D>().radius;
        }
    }
#endif 

    private void LateUpdate()
    {    
        UpdateBlackHole();
    }


    void OnTriggerEnter2D(Collider2D in_other)
    {
        if ( in_other.attachedRigidbody != null && _rigidBodiesblackhole != null && in_other.name=="Dragon" )
        {                
            //to get them nice and swirly, use the perpendicular to the direction to the vortex
            Vector3 direction = transform.position - in_other.attachedRigidbody.transform.position;
            var tangent = Vector3.Cross(direction, Vector3.forward).normalized * SWIRLSTRENGTH;

            in_other.attachedRigidbody.velocity = tangent;            
            _rigidBodiesblackhole.Add( in_other.attachedRigidbody );
        }
    }
    void OnTriggerExit2D(Collider2D in_other)
    {
        if ( in_other.attachedRigidbody != null && _rigidBodiesblackhole != null )
        {                
            //to get them nice and swirly, use the perpendicular to the direction to the vortex
                  
            _rigidBodiesblackhole.Remove( in_other.attachedRigidbody );
        }
    }

    private void UpdateBlackHole()
    {
        if( _rigidBodiesblackhole != null )
        {
            for (int i = 0; i < _rigidBodiesblackhole.Count; i++)
            {
                if( _rigidBodiesblackhole[i] != null )
                {
                    CalculateMovement( _rigidBodiesblackhole[i] );
                }
            }
        }
    }

    

    private void CalculateMovement(Rigidbody2D in_rb)
    {
        float distance = Vector3.Distance(transform.position,in_rb.transform.position); 
        float gravityIntensity = _gravityRadius/(distance*distance) ;
        //print(distance);
        if(gravityIntensity>1)
            gravityIntensity=1;
        in_rb.AddForce((transform.position - in_rb.transform.position)*gravityIntensity * in_rb.mass* GRAVITY_PULL* Time.deltaTime);
        
        in_rb.drag += 0.0001f;
        
        Debug.DrawRay(in_rb.transform.position, transform.position - in_rb.transform.position);


        if( distance <= 0.1f )
        {
            _rigidBodiesblackhole.Remove( in_rb );

            Destroy( in_rb.gameObject );
        }
    }
}
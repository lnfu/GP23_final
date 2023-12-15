using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.Playables;

public enum Location
{
    Planet,
    Space,
}

public enum PlayerState
{
    OnPlanet,
    Transform,
    Untransform,
    Launch,
    InSpace
}


public class PlayerController_new : MonoBehaviour
{

    [Header("鎖定")]
    [DisplayOnly] public bool isLocked;

    [Header("燃料")]
    [DisplayOnly] public float fuel;            // 油量/電量（0 - 100）
    public float fuelDelta;


    [Header("星球上移動跳躍")]
    public float walkSpeed;
    public float jumpHeight;
    public float launchAcceleration;

    [Header("狀態")]
    [DisplayOnly] public PlayerState playerState;

    [Header("所在星球")]
    [DisplayOnly] public GameObject planet;


    [Header("變身時間")]
    [DisplayOnly] public float transformTimer;
    public float transformTime;
    public float untransformTime;


    [Header("阻力")]
    public float linearDragOnPlanet;
    public float angularDragOnPlanet;
    public float linearDragInSpace;
    public float angularDragInSpace;

    [Header("太空移動")]
    public float driveAcceleration;
    public float turnAcceleration;


    private Animator _animator;
    private UIManager _uIManager;
    private float _gravity;
    private Rigidbody2D _rb;
    private float horizontal;
    private bool up;
    private bool isGrounded;

    public void Lock()
    {
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        fuel = 100f;
        transformTimer = transformTime;
        _animator = GetComponent<Animator>();
        isGrounded = false;
        playerState = PlayerState.Untransform;
        _uIManager = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
        if (!_uIManager)
        {
            print("測試模式");
        }

    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        up = Input.GetKey(KeyCode.W);

        _gravity = planet.GetComponent<PlanetGravity>().gravity;

    }


    void FixedUpdate()
    {

        WalkOrNot();

        JumpOrNot();

        LaunchOrNot();

    }


    private void WalkOrNot()
    {
        if (playerState == PlayerState.OnPlanet)
        {
            if (horizontal != 0)
            {
                int param = (horizontal > 0f) ? 1 : -1;
                transform.localScale = new Vector3(param, 1f, 1f); // 角色左右轉向 
                // _rb.velocity = param * walkSpeed * transform.right.normalized;
                _rb.velocity = Vector2.Dot(_rb.velocity, ((Vector2)transform.up).normalized) * ((Vector2)transform.up).normalized
                                + param * walkSpeed * ((Vector2)transform.right).normalized;

                _animator.SetBool("walk", true);
            }
            else
            {
                _animator.SetBool("walk", false);
            }
        }


    }

    private void JumpOrNot()
    {
        if (playerState == PlayerState.OnPlanet && isGrounded && up)
        {
            // 先歸零垂直方向的速度, 然後加入往上初速度
            _rb.velocity = Vector2.Dot(_rb.velocity, ((Vector2)transform.right).normalized) * ((Vector2)transform.right).normalized;
            _rb.velocity += Mathf.Sqrt(2f * jumpHeight * _gravity) * (Vector2)transform.up.normalized;
            isGrounded = false;
        }
    }

    private void LaunchOrNot()
    {
        if (playerState == PlayerState.OnPlanet)
        {
            _rb.drag = linearDragOnPlanet;
            _rb.angularDrag = angularDragOnPlanet;

            _rb.constraints = RigidbodyConstraints2D.None;

            if (!isGrounded && Mathf.Abs(Vector2.Dot(_rb.velocity, ((Vector2)transform.up).normalized)) < 0.1f && up)
            {
                playerState = PlayerState.Transform;
                transformTimer = transformTime;
                _animator.ResetTrigger("planet");
                _animator.SetTrigger("transform");
            }

        }

        if (playerState == PlayerState.Transform)
        {
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            transformTimer -= Time.fixedDeltaTime;


            if (transformTimer <= 0f)
            {
                if (!up)
                {
                    playerState = PlayerState.Untransform;
                    transformTimer = untransformTime;
                    _animator.ResetTrigger("transform");
                    _animator.SetTrigger("untransform");
                }

                playerState = PlayerState.Launch;
                _animator.ResetTrigger("transform");
                _animator.SetTrigger("spaceship_idle");
            }
        }

        if (playerState == PlayerState.Untransform)
        {
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            transformTimer -= Time.fixedDeltaTime;

            int param = (horizontal > 0f) ? 1 : -1;
            transform.localScale = new Vector3(param, 1f, 1f); // 角色左右轉向 

            if (transformTimer <= 0f)
            {
                playerState = PlayerState.OnPlanet;
                _animator.ResetTrigger("untransform");
                _animator.SetTrigger("planet");
            }
        }

        if (playerState == PlayerState.Launch)
        {
            _rb.drag = 0;
            _rb.angularDrag = 0;

            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            _rb.AddForce((_gravity + launchAcceleration) * _rb.mass * transform.up); // F = m a

            if (!up)
            {
                playerState = PlayerState.Untransform;
                transformTimer = untransformTime;
                _animator.ResetTrigger("spaceship_idle");
                _animator.SetTrigger("untransform");
            }
        }

        if (playerState == PlayerState.InSpace)
        {
            _rb.constraints = RigidbodyConstraints2D.None;

            if (up)
            {
                _rb.AddForce(driveAcceleration * _rb.mass * transform.up);
                fuel -= fuelDelta;
                if (_uIManager && fuel <= 0)
                {
                    _uIManager.LoadPlayScene();
                }
            }
            if (horizontal != 0)
            {
                _rb.AddTorque(-horizontal * turnAcceleration * _rb.mass * .5f); // .5 是力臂

            }
        }



    }




    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            transformTimer = transformTime;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Field")
        {
            playerState = PlayerState.Launch;
        }

    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.name == "Field")
        {
            playerState = PlayerState.InSpace;
            _rb.drag = linearDragInSpace;
            _rb.angularDrag = angularDragInSpace;
        }

    }


}
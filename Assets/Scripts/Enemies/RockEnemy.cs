using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RockEnemy : EnemyController
{
    [Header("Cat Enemy Scripts Parameters")]
    private Animator anim;
    public Animator animCA;
    private AnimatorStateInfo animState;
    private UIManager _uIManager;

    [DisplayOnly] public bool boolForIdleAnim = false;
    [DisplayOnly] public bool boolForWandering = true;
    [DisplayOnly] public bool inAttackRange = false;
    [DisplayOnly] public bool Stop = false;
    [DisplayOnly] public bool WallDetected = false;
    [DisplayOnly] public bool playerDetected = false;

    private int idleState;
    private int walkState;
    private int runState;
    float _changeDirectionCooldown = 1.0f;
    float _idletime = 3.0f;
    float attackRange = 8.0f;
    float _loadscenetime = 3.0f;

    //Random rnd = new Random();
    // Start is called before the first frame update
    void Start()
    {
        stage = "OnPlanet";
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        direction = 1;
        //canJump = true;

        idleState = Animator.StringToHash("Base Layer.Rock_idle");
        walkState = Animator.StringToHash("Base Layer.Rock_walk");
        runState = Animator.StringToHash("Base Layer.Rock_run");
        //jumpState = Animator.StringToHash("Base Layer.Cat_jump");

        _uIManager = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();

    }

    // Update is called once per frame
    void Update()
    {
        if (stage == "OnPlanet")
        {
            gravity = planet.GetComponent<PlanetGravity>().gravity;
        }

        DetectPlayer();
        if (Stop)
        {
            direction = 0;
            boolForIdleAnim = true;

        }
        if (player.GetComponent<PlayerController_new>().isHurt == true)
        {
               _loadscenetime-= Time.deltaTime;
                print(_loadscenetime);
                if(_loadscenetime<0) {
                    // _uIManager.LoadPlayScene();
                    _uIManager.PlayerDead((int)DeadString.rock);
                }
        }


        anim.SetBool("isPlayerInRange", isPlayerInEnemyRange);
        anim.SetBool("IsWandering", boolForWandering);
        anim.SetBool("idle", boolForIdleAnim);
        anim.SetBool("inAttackRange", inAttackRange);
        // anim.SetBool("jump", boolForJumpAnim);
        // if (boolForJumpAnim) {
        //     boolForJumpAnim = !boolForJumpAnim;
        // }
    }

    void FixedUpdate()
    {
        GroundCheck();

        if (stage == "OnPlanet")
        {
            Walk();
            Run();
            //print(isGrounded);
        }
    }
    private void HandleRandomDirectionChange()
    {
        _changeDirectionCooldown -= Time.deltaTime;

        if (_changeDirectionCooldown <= 0)
        {
            boolForIdleAnim = true;
            _idletime -= Time.deltaTime;
            direction = 0;
            if (_idletime <= 0)
            {
                boolForIdleAnim = false;
                _changeDirectionCooldown = Random.Range(1f, 5f);

                do
                {
                    direction = Random.Range(-2, 2);
                }
                while (direction == 0);
                _idletime = 3.0f;
            }
        }
    }

    // private void Idle()
    // {
    //    _idletime-= Time.deltaTime;
    // }
    protected override void Walk()
    {
        // player.GetComponent<CapsuleCollider2D>().enabled==false
        if ((isGrounded && boolForWandering))
        {
            HandleRandomDirectionChange();
            //print(_changeDirectionCooldown);
            //print(direction);
            if (direction == 0)
            {
                // 只保留垂直方向的速度
                rb.velocity = Vector2.Dot(rb.velocity, ((Vector2)transform.up).normalized) * ((Vector2)transform.up).normalized;
            }
            else
            {
                transform.localScale = (direction > 0) ? new Vector3(0.06666667f, 0.06666667f, 1) : new Vector3(-0.06666667f, 0.06666667f, 1);
                Vector2 horizontalVelocity = Vector2.Dot(rb.velocity, ((Vector2)transform.right).normalized) * ((Vector2)transform.right).normalized;
                if (horizontalVelocity.magnitude < maxWalkSpeed)
                {
                    // 水平加速
                    rb.AddForce(direction * WalkAcceleration * rb.mass * ((Vector2)transform.right).normalized);
                }
            }
        }
        //print(WallDetected);
        if (WallDetected == true)
        {
            direction *= -1;
        }
    }

    protected void Run()
    {
        if (isGrounded)
        {
            if (direction == 0)
            {
                // 只保留垂直方向的速度
                rb.velocity = Vector2.Dot(rb.velocity, ((Vector2)transform.up).normalized) * ((Vector2)transform.up).normalized;
            }
            else
            {
                transform.localScale = (direction > 0) ? new Vector3(0.06666667f, 0.06666667f, 1) : new Vector3(-0.06666667f, 0.06666667f, 1);
                Vector2 horizontalVelocity = Vector2.Dot(rb.velocity, ((Vector2)transform.right).normalized) * ((Vector2)transform.right).normalized;
                if (horizontalVelocity.magnitude < maxWalkSpeed * 2)
                {
                    // 水平加速
                    rb.AddForce(direction * WalkAcceleration * rb.mass * ((Vector2)transform.right).normalized * 2);
                }
            }
        }
    }
    protected override void DetectPlayer()
    {
        if (player.GetComponent<PlayerController_new>().isHurt == true)
            isPlayerInEnemyRange = false;

        if (playerDetected && player.GetComponent<PlayerController_new>().isHurt == false)
        //if (IsPlayerInRange(detectRange))
        {
            //print("In detect range");
            boolForWandering = false;
            isPlayerInEnemyRange = true;
            //animCA.SetBool("playerdetect", true);
            //print(isPlayerInEnemyRange);
            CaculateDirection();

            if (IsPlayerInRange(attackRange))
            {
                //print(isPlayerInEnemyRange);

                inAttackRange = true;

            }

        }
        else
        {
            //direction = 0;
            boolForWandering = true;
            inAttackRange = false;
            isPlayerInEnemyRange = false;
            //animCA.SetBool("playerdetect", false);
        }
        //print(isPlayerInEnemyRange);
    }

    protected override bool IsPlayerInRange(float rangeToDetect)
    {
        Vector3 playerPos = player.transform.position;
        float distance = (playerPos - transform.position).magnitude;

        if (distance <= rangeToDetect / 2)
        {
            //isPlayerInEnemyRange = true;
            return true;
        }
        else
        {
            //isPlayerInEnemyRange = false;
            return false;
        }
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.name == "Player")
        {
            // kill player
            player.GetComponent<PlayerController_new>().isHurt = true;
            // _uIManager.LoadPlayScene();
            _uIManager.PlayerDead((int)DeadString.rock);
        }
        // if (_uIManager)
        //     {
        //         _uIManager.LoadPlayScene();
        //     }
    }

   void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Player")
        {
            animCA.SetBool("playerdetect", true);
            playerDetected=true;
        }
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other.name == "Player")
        {
            animCA.SetBool("playerdetect", false);
            playerDetected=false;
        }
    }
}

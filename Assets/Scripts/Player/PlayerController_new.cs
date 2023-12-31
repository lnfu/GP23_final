using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.Playables;
using System;
using UnityEngine.Rendering.Universal;


public enum Location
{
    Planet,
    Space,
}

public enum PlayerState
{
    OnPlanet = 0,
    Transform = 1,
    Untransform = 2,
    Launch = 3,
    InSpace = 4,
    Landing = 5,
}


public class PlayerController_new : MonoBehaviour
{
    [Header("粒子特效")]
    public ParticleSystem fireParticleSystem;
    public ParticleSystem speedupParticleSystem;
    public Light2D  playerLight;

    [Header("鎖定")]
    public bool canMove = false;
    public bool canJump = false;
    public bool canLaunch = false;
    [DisplayOnly] public bool isLocked;
    [DisplayOnly] public bool isFreezed;

    [Header("燃料")]
    [DisplayOnly] public float fuel;            // 油量/電量（0 - 100）
    [DisplayOnly] public float oxygennumber;    // 
    public float fuelDecrement;
    public float fuelIncrement;


    [Header("星球上移動跳躍")]
    public float walkSpeed;
    public float jumpHeight;
    public float launchAcceleration;

    [Header("狀態")]
    [DisplayOnly] public PlayerState playerState;
    [DisplayOnly] public bool isGrounded;
    [DisplayOnly] public bool isHurt;

    [Header("所在星球")]
    public GameObject planet;
    public GameObject mazePlanet;
    public GameObject dragonPlanet;
    public LayerMask groundLayer;


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
    public float lowestimpactforce=500f;
    public Vector2 collisionForce;

    [Header("鍵盤輸入")]
    [DisplayOnly] public float horizontal;
    [DisplayOnly] public bool up;

    [Header("重生點")]
    public Transform rezPlanetO;
    public Transform rezMaze;
    public Transform rezDragon;

    public Animator collideanimator;
    private Animator _animator;
    private UIManager _uIManager;
    private StageManager _stageManager;
    private float _gravity;
    private Rigidbody2D _rb;
    Vector2 v2Velocity;
    private bool _isLoading; // used when dead
    private float _landingClock;

    [Header("Task")]
    public UITask task;
    public float learningMoveDelay = 5f;
    public float learningJumpDelay = 5f;
    public float LearningLaunchDelay = 5f;

    public bool inWaterPlanet;

    public void Lock()
    {
        up = false;
        horizontal = 0f;
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
    }

    public void Freeze()
    {
        isFreezed = true;
    }

    public void Unfreeze()
    {
        isFreezed = false;
    }

    void Awake()
    {
        fuel = 100f;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        transformTimer = transformTime;
        _animator = GetComponent<Animator>();
        isGrounded = false;
        playerState = PlayerState.OnPlanet;
        CollideCinemachineshake.iscollided=false;

        // UIManager
        GameObject m = GameObject.FindWithTag("UIManager");

        _uIManager = m.GetComponent<UIManager>();
        _stageManager = m.GetComponent<StageManager>();
        if (_stageManager == null ||  _stageManager.stage > Stage.LearningLaunch) {
            canMove = true;
            canJump = true;
            canLaunch = true;
        }

        isHurt = false;
        _isLoading = false;
        _uIManager.isPlayerDead = false;
        // _animator.SetBool("ishurt", isHurt);

        ResetPosition();

        if (fireParticleSystem.isPlaying)
        {
            fireParticleSystem.Stop();
        }
        if (speedupParticleSystem.isPlaying)
        {
            speedupParticleSystem.Stop();
        }

        task.AfterShow += afterShowTask;
    }

    void afterShowTask() {
        if (_stageManager.stage == Stage.LearningMove) {
            canMove = true;
        }
        if (_stageManager.stage == Stage.LearningJump) {
            canJump = true;
        }
        if (_stageManager.stage == Stage.LearningLaunch) {
            canLaunch = true;
        }
    }

    void ResetPosition()
    {

        if (_stageManager)
        {
            switch (_stageManager.stage)
            {
                case Stage.LearningMove:
                case Stage.LearningJump:
                case Stage.OnOriginPlanet:
                case Stage.LearningLaunch:
                case Stage.ToCatPlanet:
                case Stage.ToWaterPlanet:
                case Stage.Water:
                case Stage.ToMazePlanet:
                    transform.position = rezPlanetO.position;
                    break;
                case Stage.Maze:
                case Stage.ToDragonPlanet:
                    transform.position = rezMaze.position;
                    break;
                case Stage.Dragon:
                    transform.position = rezDragon.position;
                    break;
            }
        }
    }

    public void Transform()
    {
        playerState = PlayerState.Transform;
        transformTimer = transformTime;
        _animator.SetTrigger("transform");
        up = true;
    }
    public void ImmediateLaunch() {
        _rb.velocity = Vector3.zero; 
        playerState = PlayerState.InSpace;
        _animator.SetTrigger("transform");
	    Lock();
	    Invoke("ImmediateLaunchDelay", 1);
    }
    void ImmediateLaunchDelay() {
	    Unlock();
        _animator.ResetTrigger("transform");
    }

    void HandleTask() {
        // DELETE: 測試 Task 功能
        // if (Input.GetKeyDown(KeyCode.T)) {
        //     if (!task.IsShowed()) {
        //         task.ChangeTitle("Test");
        //         task.ChangeContent("This is just a simple test for the function of task UI.");
        //         task.Show();
        //     }
        //     else {
        //         task.Hide();
        //     }
        // }

        if (_stageManager.stage == Stage.LearningMove) {
            
            if (learningMoveDelay <= 0f) {
                task.ChangeTitle("MOVE AROUND");
                task.ChangeContent("Use the A and D keys to move left and right.");

                task.Show();
            }
            else {
                learningMoveDelay -= Time.deltaTime;
            }
        }

        if (_stageManager.stage == Stage.LearningJump) {

            if (learningJumpDelay <= 0f) {
                task.ChangeTitle("JUMP");
                task.ChangeContent("Quickly tap the 'W' key to jump.");

                task.Show();
            }
            else {
                learningJumpDelay -= Time.deltaTime;
            }
        }

        if (_stageManager.stage == Stage.OnOriginPlanet || _stageManager.stage == Stage.Stele) {
            task.Hide();
        }

        if (_stageManager.stage == Stage.LearningLaunch) {
            if (LearningLaunchDelay <= 0f) {
                task.ChangeTitle("LAUNCH");
                task.ChangeContent("Press and hold 'W' to initiate launch.");
                task.Show();
            }
            else {
                LearningLaunchDelay -= Time.deltaTime;
            }
        }

        if (_stageManager.stage == Stage.ToCatPlanet) {
            task.Hide();
        }
        if (_stageManager.stage == Stage.ToWaterPlanet)
        {
            task.Hide();
        }

        if (_stageManager.stage == Stage.CollectWaterItem)
        {
            task.ChangeTitle("WAKW UP THE CATFISH");
            task.ChangeContent("Collect the red stone taken by the whales.");
            task.Show();
        }
        if (_stageManager.stage == Stage.Water)
        {
            task.Hide();
        }

        if (_stageManager.stage == Stage.ToMazePlanet) {
            task.ChangeTitle("ENTER THE ABYSS");
            task.ChangeContent("Embark on a journey through the Maze Planet.");

            task.Show();
        }

        if (_stageManager.stage == Stage.Maze) {
            task.Hide();
        }

        if (_stageManager.stage == Stage.Kitten) {
            task.ChangeTitle("PLAY, KITTY!");
            task.ChangeContent("Play with kittens to summon Cat-111.");

            task.Show();
        }

        if (_stageManager.stage == Stage.Cat) {
            task.Hide();
        }


    }

    void Update()
    {
        if (_stageManager != null)
        {
            HandleTask();
        }



        if (!isHurt && !isLocked)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            up = Input.GetKey(KeyCode.W);
        }
        if (planet)
        {
            _gravity = planet.GetComponent<PlanetGravity>().gravity;
        }
        // _animator.SetBool("ishurt", isHurt);

        if (isHurt)
        {
            this.GetComponent<CapsuleCollider2D>().enabled = false;
        }

        // if (_uIManager && oxygennumber <= 0 && (!_isLoading))
        // {
        //     _uIManager.LoadPlayScene();
        //     _isLoading = true;
        // }
        // _animator.SetBool("ground", isGrounded);
        v2Velocity=_rb.velocity; 
        //collideanimator.ResetTrigger("Collided");
        //print(v2Velocity.magnitude);
    }


    void FixedUpdate()
    {

        if (canMove) WalkOrNot();

        if (canJump) JumpOrNot();

        if (canLaunch) LaunchOrNot();

        _animator.SetInteger("state", (int)playerState);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, 10f, groundLayer);
        _animator.SetFloat("height", hit.distance);

    }



    private void WalkOrNot()
    {
        if (playerState == PlayerState.OnPlanet)
        {
            if (horizontal != 0)
            {
                int param = (horizontal > 0f) ? 1 : -1;
                transform.localScale = new Vector3(param, 1f, 1f); // 角色左右轉向 
                _rb.velocity = Vector2.Dot(_rb.velocity, ((Vector2)transform.up).normalized) * ((Vector2)transform.up).normalized
                                + param * walkSpeed * ((Vector2)transform.right).normalized;

                _animator.SetBool("walk", true);


                if (_stageManager && _stageManager.stage == Stage.LearningMove) {
                    task.Hide();
                    _stageManager.UpdateStage();
                }
            }
            else
            {
                _rb.velocity = Vector2.Dot(_rb.velocity, ((Vector2)transform.up).normalized) * ((Vector2)transform.up).normalized;
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

            if (_stageManager && _stageManager.stage == Stage.LearningJump) {
                task.Hide();
                _stageManager.UpdateStage();
            }

        }


        float vertical = Vector2.Dot(_rb.velocity, ((Vector2)transform.up).normalized);
        if (vertical < 1.6f && vertical > -1.6f) {
            _animator.SetInteger("vertical", 0);
        }
        else if (vertical > 1.6f) {
            _animator.SetInteger("vertical", 1);
        }
        else if (vertical < -1.6f) {
            _animator.SetInteger("vertical", -1);
        }
    }

    private void LaunchOrNot()
    {
        // _animator.ResetTrigger("transform");
        // _animator.ResetTrigger("untransform");

        if (playerState == PlayerState.OnPlanet)
        {
            _animator.ResetTrigger("untransform");

            _rb.drag = linearDragOnPlanet;
            _rb.angularDrag = angularDragOnPlanet;

            if (isFreezed)
            {
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else
            {
                _rb.constraints = RigidbodyConstraints2D.None;
            }




            // TODO
            // LayerMask mask = ~groundLayer;
            // int mask = (1 << 3);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, 10f, groundLayer);
            if (hit.collider != null)
            {
                // print(hit.distance);
                // Debug.DrawRay(transform.position, -transform.up * hit.distance, Color.yellow);
                // Debug.Log("Did Hit");

                if (hit.distance > 3.5f && up)
                {
                    playerState = PlayerState.Transform;
                    transformTimer = transformTime;
                    _animator.SetTrigger("transform");
                }

                
            }
            // else
            // {
            //     Debug.DrawRay(transform.position, -transform.up * 1000, Color.white);
            //     Debug.Log("Did not Hit");
            // }

            // if ((!isGrounded) && (Mathf.Abs(Vector2.Dot(_rb.velocity, ((Vector2)transform.up).normalized)) < 0.18f) && up)
            // {
            //     playerState = PlayerState.Transform;
            //     transformTimer = transformTime;
            //     _animator.SetTrigger("transform");
            // }
            if (fireParticleSystem.isPlaying)
            {
                fireParticleSystem.Stop();
            }
            if (speedupParticleSystem.isPlaying)
            {
                speedupParticleSystem.Stop();
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
                    _animator.SetTrigger("untransform");
                }

                playerState = PlayerState.Launch;
            }
        }

        if (playerState == PlayerState.Untransform)
        {
            _animator.ResetTrigger("transform");

            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            transformTimer -= Time.fixedDeltaTime;

            int param = (horizontal > 0f) ? 1 : -1;
            transform.localScale = new Vector3(param, 1f, 1f); // 角色左右轉向 

            if (transformTimer <= 0f)
            {
                playerState = PlayerState.OnPlanet;
            }

            if (fireParticleSystem.isPlaying)
            {
                fireParticleSystem.Stop();
            }
            if (speedupParticleSystem.isPlaying)
            {
                speedupParticleSystem.Stop();
            }

        }

        if (playerState == PlayerState.Launch)
        {
            _animator.ResetTrigger("transform");

            if (_stageManager && _stageManager.stage == Stage.LearningLaunch) {
                _stageManager.UpdateStage();
            }


            _rb.drag = 0;
            _rb.angularDrag = 0;

            if (isFreezed)
            {
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else
            {
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            }

            _rb.AddForce((_gravity + launchAcceleration) * _rb.mass * transform.up); // F = m a
            if (!fireParticleSystem.isPlaying)
            {
                fireParticleSystem.Play();
            }
            if (!speedupParticleSystem.isPlaying)
            {
                speedupParticleSystem.Play();
            }

            if (!up)
            {
                playerState = PlayerState.Untransform;
                transformTimer = untransformTime;
                _animator.SetTrigger("untransform");
            }
        }

        if (playerState == PlayerState.InSpace)
        {

            if (isFreezed)
            {
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else
            {
                _rb.constraints = RigidbodyConstraints2D.None;
            }

            if (up)
            {
                _animator.SetBool("accelerate", true);
                if (!fireParticleSystem.isPlaying)
                {
                    fireParticleSystem.Play();
                }
                if (!speedupParticleSystem.isPlaying)
                {
                    speedupParticleSystem.Play();
                }
                _rb.AddForce(driveAcceleration * _rb.mass * transform.up);

                if (fuel >= 20f)
                {
                    fuel -= fuelDecrement;
                }
                else
                {
                    fuel -= fuelDecrement;
                }

                if (_uIManager && fuel <= 0 && (!_isLoading))
                {
                    // _uIManager.LoadPlayScene();
                    _uIManager.PlayerDead((int)DeadString.fuel);
                    _isLoading = true;
                }
            }
            else
            {
                _animator.SetBool("accelerate", false);
                if (fireParticleSystem.isPlaying)
                {
                    fireParticleSystem.Stop();
                }
                if (speedupParticleSystem.isPlaying)
                {
                    speedupParticleSystem.Stop();
                }
            }
            if (horizontal != 0)
            {
                _rb.AddTorque(-horizontal * turnAcceleration * _rb.mass * .5f); // .5 是力臂

            }
            playerLight.intensity = Mathf.Lerp(3, 7,1);
        }


        if (playerState == PlayerState.Landing)
        {
            _landingClock -= Time.fixedDeltaTime;

            _rb.velocity = Vector2.Dot(_rb.velocity, ((Vector2)transform.up).normalized) * ((Vector2)transform.up).normalized;

            if (_landingClock <= 0f)
            {
                playerState = PlayerState.Launch;
            }
            playerLight.intensity = Mathf.Lerp(7, 3, 1);
        }


    }




    void OnCollisionEnter2D(Collision2D other)
    {

        if ((1 << other.gameObject.layer & groundLayer) == 1 << other.gameObject.layer)
        {
            isGrounded = true;
            transformTimer = transformTime;
            if (planet == mazePlanet && _stageManager && _stageManager.stage == Stage.ToMazePlanet)
            {
                _stageManager.UpdateStage();
            }
            if (planet == dragonPlanet && _stageManager && _stageManager.stage == Stage.Maze)
            {
                _stageManager.UpdateStage();
            }
        }
        
        if(other.gameObject.CompareTag("Obstacle"))
        {
            //animation["Collide"].wrapMode = WrapMode.Once;
            //animation.Play("Collide");
            
            collisionForce=ComputeTotalImpulse(other);
            //print(collisionForce);
            //Vector2 collisionForce = other.impulse / Time.deltaTime;
            if(collisionForce.magnitude>lowestimpactforce&&(fuel-(collisionForce.magnitude-lowestimpactforce)*0.1f>0))
            {
                fuel-=(collisionForce.magnitude-lowestimpactforce)*0.1f;
                collideanimator.SetTrigger("Collided");
                
                CollideCinemachineshake.iscollided=true;
                //print(CinemachineShake.iscollided);
            }
            else if(collisionForce.magnitude>lowestimpactforce&&(fuel-(collisionForce.magnitude-lowestimpactforce)*0.1f<0))
            {
                fuel=0.1f;
                collideanimator.SetTrigger("Collided");
                CollideCinemachineshake.iscollided=true;
                //print("explode");
            }
            //collideanimator.ResetTrigger("Collided");
        }
        // if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        // {
        // }

    }

    void OnCollisionExit2D(Collision2D other)
    {
        if ((1 << other.gameObject.layer & groundLayer) == 1 << other.gameObject.layer)
        {
            isGrounded = false;
        }
        // if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        // {
        //     isGrounded = false;
        // }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Field" && playerState == PlayerState.InSpace)
        {
            playerState = PlayerState.Landing;
            _landingClock = 0.5f;



        }

        if (other.name == "CatPlanetField" && playerState == PlayerState.InSpace)
        {
            // 先複製上面的這樣寫, 雖然很醜, 之後應該改用 tag 表示 field
            // 並且是 field 讓玩家套用不同星球的紀錄
            playerState = PlayerState.Landing;
            _landingClock = 0.5f;

            if (_stageManager && _stageManager.stage == Stage.ToCatPlanet)
            {
                _stageManager.UpdateStage();
            }
        }

        if (other.name=="Dragon")
        {
            if (_uIManager && (!_isLoading))
            {
                //_uIManager.BackStage();
                // _uIManager.LoadPlayScene();
                _uIManager.PlayerDead((int)DeadString.dragon);
                _isLoading = true;
             }
        }
        

        if (other.CompareTag("Fruit") && fuel < 100f)
        {
        //     Tree tree = other.GetComponentInParent<Tree>();
        //     if (tree != null)
        //     {
        //         tree.FruitEaten();
        //         Destroy(other.gameObject);
        //     }
             fuel += fuelIncrement;
             if (fuel > 100f) fuel = 100f;
        }

        if (other.CompareTag("FruitOnBroken") && fuel < 100f)
        {
            //For broken planet
            Treebroken tree = other.GetComponentInParent<Treebroken>();
            if (tree != null)
            {
                tree.FruitEaten();
                Destroy(other.gameObject);
            }
            fuel += fuelIncrement;
            if (fuel > 100f) fuel = 100f;
        }
        if(other.name=="Water Planet")
        {
            inWaterPlanet=true;
        }

    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other.name == "Field" || other.name == "CatPlanetField")
        {
            playerState = PlayerState.InSpace;
            _rb.drag = linearDragInSpace;
            _rb.angularDrag = angularDragInSpace;
        }

        if(other.name=="Water Planet")
        {
            inWaterPlanet=false;
        }
    }

    public bool inWater()
    {
        return inWaterPlanet;
    }

    static Vector2 ComputeTotalImpulse(Collision2D collision) 
    {
        Vector2 impulse = Vector2.zero;

        int contactCount = collision.contactCount;
        for(int i = 0; i < contactCount; i++) 
        {
            var contact = collision.GetContact(i);
            impulse += contact.normal * contact.normalImpulse;
            impulse.x += contact.tangentImpulse * contact.normal.y;
            impulse.y -= contact.tangentImpulse * contact.normal.x;
        }
        return impulse;
    }
}

internal class DisplayOnlyAttribute : Attribute
{
}

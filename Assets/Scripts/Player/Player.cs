using System.Collections;
using System.ComponentModel;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Player : MonoBehaviour, IDamageable
{

    public static Player instance;

    //private references 
    Camera cam;

    //coroutine references for ensuring no duplicates
    Coroutine iFramesRoutine = null;

    [Header("Manually Assigned References")]
    public Rigidbody rb;

    //for now we'll just use a ragdoll that's
    //pre loaded so we don't instantiate one every time
    //but we'll have to remember to copy the colors and to
    //remake the ragdoll so it isn't super glitchy.
    //and we'll only do ragdoll on death. 
    public Transform ragdollParentTransform;

    //the chest rigidbody of the ragdoll
    //we use this to apply forces on death
    //so the ragdoll maintains the velocity
    //we were moving at when we die.
    public Rigidbody ragdollChest;

    //the 3D model game object of the animated character.
    //we disable this on death and replace with a ragdoll.
    public GameObject animatedModel; 

    public PlayerInput playerInput;

    public Weapon curWeapon;

    public ParticleSystem dashParticles;

    #region health vars
    [Header("Health Variables")]
    public int _maxHealth = 3;

    public int maxHealth
    {
        get
        {
            //Calc the health when we access it.
            //Also make sure that if we 
            //leveled up and got more health
            //that we reset the player's health.
            //if (_maxHealth != CalcHealth(level))
            //{
            //    _maxHealth = CalcHealth(level);
            //    /*                if (_health < 9)
            //                    {
            //                        _health = 10;
            //                    }*/
            //    curHealth = CalcHealth(level);
            //}
            return _maxHealth;
        }
    }

    private int _curHealth = 3;

    public int curHealth
    {
        get
        {
            return _curHealth;
        }

        set
        {
            _curHealth = Mathf.Clamp(value, 0, maxHealth);
            if (curHealth == 0)
            {
                Die();
            }
        }
    }

    #endregion

    [Header("Damage Variables")]
    //the force at which we bounce off of the object that damaged us. 
    public float bounceForce = 5f;

    public bool invincible = false;

    //terraria uses this number for iframes as do most games.
    public float iFrameTime = 0.67f;

    [Header("Movement Variables")]
    
    public float groundCheckScale = 0.8f;

    public bool accelerating = false;
    public float minSpeed = 0f; // Minimum speed, will always be 0.
    public float maxSpeed = 20f;
    public float timeToMaxSpeed = 10f; //The time it takes to reach roughly 20m/s which I think is good to be 10 seconds.
    public float currentInputMoveTime = 0f; //The amount of time the player has been holding any of the WASD keys. 
    public AnimationCurve speedCurve; // Custom speed curve
    public AnimationCurve accelerationCurve;
    public float maxAcceleration = 10f;
    public float maxDeceleration = 20f;

    public float slowDownMultiplier = 0.05f;

    public float currentSpeed = 0f;

    [SerializeField]
    private float moveSpeed = 5.0f;

    public float walkSpeed = 10f;
    
    public float sprintSpeed = 20f;


    public float rotationSpeed = 5f;

    private Coroutine slowToStopCoroutine;

    //Variables for movement
    Vector2 moveInput = Vector3.zero;

    public Vector3 lastMoveVector = Vector3.zero;
    Vector3 moveVector;

    [Header("Dash Parameters")]
    public bool doDash = false;
    public float dashSpeed = 10f;
    public int dashTotal = 1;
    private int dashCount = 1;
    public bool dashing = false;
    public float dashDist = 10f;

    [Header("Jump Parameters")]
    public float groundCheckDist = 0.1f;
    [SerializeField] private int jumpCount = 1;
    public int jumpTotal = 1;
    [SerializeField] private bool jumpCanceled;
    [SerializeField] private bool jumping;
    public float jumpHeight = 5f;
    [SerializeField] private float buttonTime;
    [SerializeField] private float jumpTime;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float multiplier = 100f;
    public float timeToApex = 0.01f;
    public float timeToFall = 0.5f;

    //The gravity we return to 
    //after modifying gravity.
    float baseGravity = 9.81f;
    float gravity = 9.81f;
    float fallGravity = 9.81f;

    public bool doJump;

    /* private Coroutine jumpCoroutine;

     public bool jumpPressed = false;

     public bool isJumping = false;*/

    public Collider playerCollider;

    public bool isGrounded = false;

    public bool inAir => !jumping && !isGrounded;

    public Vector3 desiredMoveDirection;

    public Vector2 accumulatedVelocity = Vector2.zero;

    bool isOnWall = false;

    bool isSprinting = false;

    //Input actions
    InputAction moveAction;
    InputAction jumpAction;
    InputAction attackAction;
    InputAction dashAction;

    //Raycast vars

    LayerMask playerMask;

    //set our static instance so it's easier to find the player.
    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //DISABLE GRAVITY SO WE CAN USE OUR OWN.
        rb.useGravity = false;

        //get player mask
        playerMask = LayerMask.GetMask("Player", "Ignore Raycast"); //Assign our layer mask to player
        playerMask = ~playerMask; //Invert the layermask value so instead of being just the player it becomes every layer but the mask

        //get cam
        cam = Camera.main;

        //get move action
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        attackAction = playerInput.actions["Attack"];
        dashAction = playerInput.actions["Dash"];

        //disable the ragdoll after
        //it is done initializing the joints.
        //if it is disabled before the start method is called
        //then the joints don't work.
        ragdollParentTransform.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        #region isGroundedCheck
        //isGrounded = Physics.BoxCast(transform.position, this.GetComponent<Collider>().bounds.size, -transform.up, Quaternion.identity, groundCheckDist, playerMask);
        //isGrounded = Physics.Raycast(transform.position, -transform.up, this.GetComponent<Collider>().bounds.extents.y + groundCheckDist, playerMask);
        Collider[] colliders = Physics.OverlapBox(transform.position + (-transform.up * this.GetComponent<Collider>().bounds.size.y / 2) + (-transform.up * groundCheckDist), new Vector3(GetComponent<Collider>().bounds.size.x * groundCheckScale, 0.1f, GetComponent<Collider>().bounds.size.z * groundCheckScale), transform.rotation, playerMask);
        if (colliders.Length > 0)
        {
            isGrounded = true;
            //Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red, 1f);
        }
        else
        {
            isGrounded = false;
        }
        /*isGrounded = Physics.BoxCast(transform.position, this.GetComponent<Collider>().bounds.size, -transform.up, out RaycastHit hitInfo, Quaternion.identity, this.GetComponent<Collider>().bounds.extents.y + groundCheckDist, playerMask);*/

        //Ray ray = new Ray(transform.position, -transform.up);
        //isGrounded = GetComponent<Collider>().Raycast(ray, out RaycastHit hitinfo, groundCheckDist);
        #endregion

        //get the movement direction.
        moveInput = moveAction.ReadValue<Vector2>();

        //TODO: Remove isSprinting because we don't want sprint anymore.
        moveSpeed = isSprinting ? sprintSpeed : walkSpeed;

        //Get the forward vector using player up and the camera right vector
        //so it's a forward vector on the plane created by the up axis of the player
        //and the right axis of the camera. 
        Vector3 forwardProjectedOnPlane = Vector3.Cross(cam.transform.right, transform.up);

        Debug.DrawRay(transform.position, forwardProjectedOnPlane.normalized * 10f, Color.yellow);

        //project controls to the camera's rotation so left and right are always the left and right sides of the camera.
        desiredMoveDirection = cam.transform.right * moveInput.normalized.x + forwardProjectedOnPlane * moveInput.normalized.y;
        moveVector = cam.transform.right * moveInput.normalized.x + forwardProjectedOnPlane * moveInput.normalized.y;
        moveVector = moveVector.normalized * moveSpeed;

        //Getting sqrMagnitude is more efficient than normal magnitude.
        //Is the player trying to move right now?
        if (moveInput.sqrMagnitude > 0)
        {
            accelerating = true;
        }
        else
        {
            accelerating = false;
        }

        if (accelerating)
        {
            //increment the time we've been moving
            currentInputMoveTime += Time.deltaTime;
            Mathf.Clamp(currentInputMoveTime, 0f, timeToMaxSpeed);
        }
        else
        {
            currentInputMoveTime = 0;
        }

        //jumpPressed |= jumpAction.WasPressedThisFrame();

        doJump |= (jumpAction.WasPressedThisFrame() && jumpCount > 0 && !jumping);

        if (isGrounded)
        {
            //reset jump count and jump canceled, and gravity
            //when not jumping and grounded.
            if (!jumping)
            {
                jumpCount = jumpTotal;
                jumpCanceled = false;
                //set gravity back to base.
                gravity = baseGravity;
                Debug.Log("BACK TO BASE".Color("Green"));
                //animator.SetTrigger("landing");
            }
            //reset dash count when grounded, and not dashing.
            if (!dashing)
            {
                dashCount = dashTotal;
            }
        }

        //increase jump time while jumping
        if (jumping)
        {
            jumpTime += Time.deltaTime;
        }

        if (jumping && !jumpCanceled)
        {
            if (!jumpAction.IsPressed()) //If we stop giving input for jump cancel jump so we can have a variable jump.
            {
                jumpCanceled = true;
                //gravity = fallGravity;
            }

            if (jumpTime >= buttonTime) //When we reach our projected time stop jumping and begin falling.
            {
                Debug.Log("JUMP CANCELED BY BUTTON TIME".Color("Green"));
                //pause the editor
                //Debug.Break();
                jumpCanceled = true;

                //set gravity back to fall gravity
                gravity = fallGravity;
                //gravity = baseGravity;


                //jumpDist = Vector2.Distance(transform.position, ogJump); //Not needed, just calculates distance from where we started jumping to our highest point in the jump.
                //jumpDist = transform.position.y - ogJump.y;
            }
        }

        if (jumpCanceled)
        {
            jumping = false;
        }

        doDash |= dashAction.WasPressedThisFrame() && dashCount > 0 && !dashing;

        #region attacking

        if (attackAction.WasPressedThisFrame())
        {
            curWeapon.Attack();
        }

        #endregion

        #region wall check
        Collider[] colliders1 = Physics.OverlapBox(transform.position + (-transform.right * this.GetComponent<Collider>().bounds.size.x / 2), new Vector3(0.1f, GetComponent<Collider>().bounds.size.y * 2f / 3f, GetComponent<Collider>().bounds.size.z), transform.rotation, playerMask);

        Collider[] colliders2 = Physics.OverlapBox(transform.position + (transform.right * this.GetComponent<Collider>().bounds.size.x / 2), new Vector3(0.1f, GetComponent<Collider>().bounds.size.y * 2f / 3f, GetComponent<Collider>().bounds.size.z), transform.rotation, playerMask);

        if (colliders1.Length > 0 || colliders2.Length > 0)
        {
            isOnWall = true;
        }
        else
        {
            isOnWall = false;
        }

        #endregion
    }

    private void FixedUpdate()
    {
        //Vector3 prevVel = rb.linearVelocity;

        

        //project controls to the camera's rotation so left and right are always the left and right sides of the camera.
        //moveVector = cam.transform.right * moveInput.x + cam.transform.forward * moveInput.y;

       
        //set the speed using move speed and the normalized movement direction vector.
        //rb.linearVelocity = moveVector.normalized * moveSpeed;

        
        HandleRbRotation();

        HandleDashing();
        HandleMovement();
        HandleJumping();
        
    }

    public void HandleRbRotation()
    {
        //when dashing we want to rotate to face the dash direction
        if (dashing)
        {
            return;
            //rb.MoveRotation(Quaternion.LookRotation(new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z), transform.up));
        }
        else //otherwise we want to face the input direction.
        {
            //rotate towards the velocity direction but don't rotate upwards.
            if (moveVector != Vector3.zero)
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(new Vector3(moveVector.x, 0, moveVector.z), transform.up), rotationSpeed));
        }

        

        
    }

    public void HandleMovement()
    {
        if (dashing)
        {
            return;
        }

        //We need to store the accumulated velocity and just take that and use vector projection on the normalized input to 
        //get some sort of accumulation going where we can just have the actual speed be moving in the direction of our input.
        //moveVector = (transform.forward * moveInput.y * lastVel.z) + (transform.right * moveInput.x * lastVel.x);

        //remove the player's addtion from the current velocity. We want the player's input to be constant, not a part of the force.
        //this means we have to remove it before doing anything else. 
        //if (moveInput.sqrMagnitude > 0) //only decrease if we want to move again. 
        //rb.linearVelocity -= lastMoveVector;

        //if (moveInput.sqrMagnitude > 0) //only decrease if we want to move again. 
        //Set last Move Vector so we can use it later. 
        lastMoveVector = moveVector;

        //rb.linearVelocity += moveVector;

        #region constant movement
        //Store yVelocity
        float yVel = rb.linearVelocity.y;
        rb.linearVelocity = moveVector;
        //Restore yVelocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, yVel, rb.linearVelocity.z);
        #endregion

        ////The dot product check is for when we are
        ////turning on a dime and our velocity is the opposite direction
        ////of our desired velocity. 
        //if (Vector3.Dot(desiredMoveDirection, rb.linearVelocity) < 0 || moveInput.magnitude == 0 && /*!grappling.IsGrappling() &&*/ !isJumping && isGrounded && /*!dashPressed &&*/ !jumpPressed)
        //{
        //    //rb.linearVelocity *= 0;
        //    //Slow down very quickly but still make it look like it was gradual.
        //    if (slowToStopCoroutine == null)
        //    {
        //        slowToStopCoroutine = StartCoroutine(slowToStop());
        //    }
        //}
        ////Instant velocity changes depending on speed.
        ////When we start moving we want to immediately go 
        ////to our base walking speed so velocity change.
        ////Then from there on out we slowly approach running.
        //else if (isGrounded && /*!grappling.IsGrappling() &&*/ !isJumping && !jumpPressed)
        //{
        //    // Calculate normalized time for acceleration and deceleration
        //    // float accelerationTime = currentSpeed / maxSpeed;
        //    // float decelerationTime = 1f - accelerationTime;
        //    float accelerationTime = currentInputMoveTime / timeToMaxSpeed;
        //    float decelerationTime = 1f - accelerationTime;

        //    // Apply custom acceleration curve
        //    currentSpeed = accelerationCurve.Evaluate(rb.linearVelocity.magnitude/*accelerationTime*/);

        //    // Apply custom speed curve
        //    // currentSpeed = speedCurve.Evaluate(accelerationTime);

        //    Debug.DrawRay(transform.position, transform.TransformDirection(rb.linearVelocity), Color.blue);

        //    //Vector3 targetVelocity = desiredMoveDirection.normalized * maxSpeed;

        //    // Evaluate speed using animation curve, they highest value in the curve is 1 so 1 * maxspeed = maxspeed. 
        //    // This is how we gradually approach our maxSpeed.
        //    //float targetSpeed = speedCurve.Evaluate(rb.linearVelocity.magnitude / maxSpeed) * maxSpeed;
        //    Vector3 targetVelocity = desiredMoveDirection.normalized * currentSpeed;
        //    //Vector3 targetVelocity = desiredMoveDirection.normalized * maxSpeed;

        //    //AccelerateToward(targetVelocity);
        //    // Determine current acceleration based on current speed
        //    //float acceleration = Mathf.Lerp(initialAcceleration, maxAcceleration, rb.linearVelocity.magnitude / maxSpeed);

        //    // Calculate current velocity in desired direction
        //    //Vector3 currentVelocity = Vector3.Project(rb.linearVelocity, new Vector3(moveInput.x, 0f, moveInput.y));

        //    //CounterVelocity();
        //    //Doing targetVelocity - rb.linearVelocity * rb.mass / Time.fixedDeltaTime gives us an acceleration of sorts.
        //    //This is what makes it so no matter how fast you turn the velocity isn't decreased.
        //    //The LateUpdate() call is what sets the direction of velocity to always face the direction we are wanting
        //    //to move. 

        //    //The only problem here is doing targetVelocity - rb.linearVelocity basically gets rid of any speed generated
        //    //by doing a grapple or dash. We want to perserve it somehow.
        //    //targetVelocity = targetVelocity.normalized * (targetVelocity.magnitude + (rb.linearVelocity.magnitude));
        //    Vector3 force = (targetVelocity/* - rb.linearVelocity*/)/* * rb.mass / Time.fixedDeltaTime*/;
        //    //force *= acceleration;
        //    //force = Vector3.ClampMagnitude(force, maxSpeed);
        //    //rb.AddForce(force, ForceMode.VelocityChange);
        //    if (slowToStopCoroutine == null)
        //        rb.AddForce(force, ForceMode.Force);



        //    //rb.AddForce(movement, ForceMode.VelocityChange);
        //    //rb.AddForce(moveVector * movementMultiplier, ForceMode.Force);
        //    //rb.AddForce(AccumulatedVelocity, ForceMode.VelocityChange);
        //}
        //else
        //{
        //    if (slowToStopCoroutine == null)
        //        rb.AddForce(moveVector);
        //}

        //rb.AddForce(moveVector - GetComponent<Rigidbody>().velocity, ForceMode.VelocityChange);
        //rb.AddForce(moveVector);

        //rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);


    }

    public void HandleJumping()
    {
        if (dashing)
        {
            return;
        }

        if (doJump)
        {
            //I did the work out and 2 * h / t = gravity so I'm going to do that.
            gravity = 2 * jumpHeight / timeToApex;
            fallGravity = 2 * jumpHeight / timeToFall;

            float projectedHeight = timeToApex * gravity / 2f;
            Debug.Log(timeToApex + " " + projectedHeight + " " + gravity);
            Debug.Log(("Projected Height " + projectedHeight).ToString().Color("Cyan"));

            doJump = false;
            jumpCount--;
            float jumpForce;

            jumpForce = Mathf.Sqrt(2f * gravity * jumpHeight) * rb.mass; //multiply by mass at the
            //end so that it reaches the height regardless of weight.

            //divide by 2 so we get the amount of time to reach the apex of the jump.
            buttonTime = (jumpForce / (rb.mass * gravity));
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            jumpTime = 0;
            jumping = true;
            jumpCanceled = false;

            
        }

        //Where I learned this https://www.youtube.com/watch?v=7KiK0Aqtmzc
        //This is what gives us consistent fall velocity so that jumping has the correct arc.
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

        if (localVel.y < 0 && inAir) //If we are in the air and at the top of the arc then apply our fall speed to make falling more game-like
        {
            //animator.SetBool("falling", true);
            //we don't multiply by mass because forceMode2D.Force includes that in it's calculation.
            //set gravity to be fallGravity.
            gravity = fallGravity;
            Vector3 jumpVec = -transform.up * (fallMultiplier - 1)/* * 100f * Time.deltaTime*/;
            rb.AddForce(jumpVec, ForceMode.Force);
        }
    }

    public void HandleDashing()
    {
        if (doDash)
        {
            doDash = false;
            //rb.AddForce(transform.forward.normalized * dashSpeed, ForceMode.Impulse);
            //StartCoroutine(MoveToPosition(transform.forward.normalized * dashDist, dashSpeed, 0.1f));
            dashing = true;
            dashCount--;
            StartCoroutine(DashCoroutine());
           
        }
    }

    //LD Montello
    //dash given speed
    //and distance.
    public IEnumerator DashCoroutine()
    {
        //set the dash particles to play
        dashParticles.Play();

        //cancel out all momentum accept for jumping.
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        Vector3 startPos = transform.position;


        //acceleration = (finalVelocity^2 - initialVelocity^2) / (2 * distance)
        float acceleration = (dashSpeed * dashSpeed - 0 * 0) / (2 * dashDist);
       

        //Time = distance / speed
        float dashTime = dashDist / dashSpeed;

        //rb.AddForce(transform.forward * dashSpeed, ForceMode.Impulse);

        //move at the dashSpeed
        //until we have reached the full
        //calculated dash time.
        while (dashTime > 0)
        {
            //rb.AddForce(transform.forward * dashSpeed, ForceMode.Force);
            dashTime -= Time.deltaTime;
            rb.linearVelocity = transform.forward.normalized * dashSpeed;

            //we wait for fixed update as yielding null here will cause
            //movements to be inconsistent. All rigidbody velocity modifications
            //should always occur in the fixed (also known as physics) step.
            yield return new WaitForFixedUpdate();
        }


        //set the dash particles to stop
        dashParticles.Stop();

        //reset the x and z velocity but don't reset y velocity in case they jumped and dashed.
        //this simulates an instantaneous stop.
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        //Debug the total distance to make sure it is consistent
        //Debug.LogWarning("Final Distance = " + (Vector3.Distance(transform.position, startPos)));

        //say we are no longer
        //dashing
        dashing = false;

        
    }

    

    void LateUpdate()
    {
        ApplyFinalMovements();
    }

    /// <summary>
    /// Called in late update, should only contain applications that occur after we are done calculating physics. 
    /// I.E. if we ended up doing custom gravity, call it here.
    /// </summary>
    public void ApplyFinalMovements()
    {
        //We need to check that the desiredMoveDirection vector isn't zero because otherwise it can zero out our velocity.
        if (isGrounded && !dashing && !jumping && /*!grappling.IsGrappling() && */desiredMoveDirection.normalized.sqrMagnitude > 0)
        {
            // Set the velocity directly to match the desired direction
            // Don't clamp the speed anymore as there isn't a good reason to do so.
            // Don't override the Y velocity.
            
            //store the current y velocity
            float tempY = rb.linearVelocity.y;
            //remove the Y value from velocity before we apply that to the forward momentum so we don't "steal" values from the vertical
            //momentum and add them to the forward momentum.
            Vector3 velWithoutY = rb.linearVelocity - new Vector3(0f, tempY, 0f);

            //Clamp without y to not prevent jump speed from slowing down,
            //just clamp xz plane movement.
            velWithoutY = Vector3.ClampMagnitude(velWithoutY, maxSpeed);

            //when turning on a dime instantly stop moving before continuing.
            if (Vector3.Dot(desiredMoveDirection, rb.linearVelocity) < 0)
            {
                rb.linearVelocity = new Vector3(0f, tempY, 0f);
            }
            else
            {
                rb.linearVelocity = desiredMoveDirection.normalized * velWithoutY.magnitude/*Mathf.Clamp(rb.linearVelocity.magnitude, 0f, maxSpeed)*/;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, tempY, rb.linearVelocity.z);
            }

           

            //if (Vector3.Dot(desiredMoveDirection, rb.linearVelocity) < 0)
            //{
            //    //currentSpeed = accelerationCurve.Evaluate(0f);//rb.linearVelocity.magnitude/*accelerationTime*/);
            //    //rb.linearVelocity -= rb.linearVelocity + desiredMoveDirection * 0.1f;

            //    /*rb.linearVelocity -= velWithoutY;
            //    rb.linearVelocity += velWithoutY * 0.01f;*/

            //    //Slow down very quickly but still make it look like it was gradual.
            //    if (slowToStopCoroutine == null)
            //    {
            //        slowToStopCoroutine = StartCoroutine(slowToStop());
            //    }

            //}

        }


        //Apply gravity, because gravity is not affected by mass and 
        //we can't use ForceMode.acceleration with 2D just multiply
        //by mass at the end. It's basically the same.
        //In unity it factors in mass for this calculation so 
        //multiplying by mass cancels out mass entirely.
        rb.AddForce(-transform.up * gravity * rb.mass);
    }

    private IEnumerator slowToStop()
    {
        Debug.LogWarning("Start Slowing To Stop!");
        //we check both if the player has stopped giving input
        //or if they just turned on a dime using dot product
        //to determine if we need to keep slowing down,
        //this allows us to slow down before moving in the 
        //opposite direction when turning on a dime,
        //while still maintaining the original slow down
        //conditions from momentum mori.
        while (rb.linearVelocity.magnitude > 0 && (moveInput.magnitude == 0 || Vector3.Dot(desiredMoveDirection, rb.linearVelocity) < 0) && isGrounded /*&& !dashPressed*/)
        {
            Debug.LogWarning("Slowing To Stop!");
            //AccumulatedVelocity -= 0.05f * AccumulatedVelocity;

            //store the current y velocity
            float tempY = rb.linearVelocity.y;
            //remove the Y value from velocity before we apply that to the forward momentum so we don't destroy values from the vertical
            //momentum in an attempt to slow down horizontal momentum
            Vector3 velWithoutY = rb.linearVelocity - new Vector3(0f, tempY, 0f);

            rb.linearVelocity -= slowDownMultiplier * velWithoutY;
            yield return new WaitForFixedUpdate();
        }
        slowToStopCoroutine = null;
    }

    #region unchanged by momentum mori code

    public void Die()
    {

        for (int i = 0; i < ragdollParentTransform.transform.childCount; i++)
        {
            ragdollParentTransform.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("LowRes");

            for (int j = 0; j < ragdollParentTransform.transform.GetChild(i).transform.childCount; j++)
            {
                ragdollParentTransform.transform.GetChild(i).transform.GetChild(j).gameObject.layer = LayerMask.NameToLayer("LowRes");
            }
        }

        //set to be low resolution
        ragdollParentTransform.gameObject.layer = LayerMask.NameToLayer("LowRes");

        ragdollParentTransform.gameObject.SetActive(true);
        //set ragdoll parent position and then unparent the limbs
        ragdollParentTransform.position = transform.position;
        ragdollParentTransform.DetachChildren();

        //Copy current velocity to the ragdoll chest
        ragdollChest.linearVelocity = rb.linearVelocity;
        ragdollChest.angularVelocity = rb.angularVelocity;

        //TODO:
        //copy the animation rotation and such of the model's limbs
        //to the ragdoll.



        //disable the visual model for the character.
        animatedModel.SetActive(false);
    }

    public void TakeDamage(int d, GameObject other)
    {
        //if we're invincible, 
        //then exit this method.
        if (invincible)
        {
            return;
        }

        curHealth -= d;

        //Set to be low resolution for a small amount of time.
        StartLowResRoutine();

        //Start iFrames here.
        StartIFrames();

        //TODO:
        //change the layer of the visual model for the player to be
        //the lowres layer,
        //have the player get bounced away from the damaging object,
        //and then also give them invincibility frames where
        //they do the blinking in and out of existance thing.
        rb.linearVelocity += (transform.position - other.transform.position).normalized * bounceForce;

        //print out data about the player taking damage.
        Debug.Log("Player Took: ".Color("Blue") + d.ToString().Color("Red") + " from " + other.transform.root.name.Color("Orange"));
    }


    public void StartIFrames()
    {


        //if we're already invincible and
        //the iframes coroutine is currently
        //going, stop it, and create a new one.
        //Debug an error that this should never occur.
        if (invincible == true && iFramesRoutine != null)
        {
            StopCoroutine(iFramesRoutine);
            invincible = false;
            Debug.LogError("Player was damaged when in I-Frames, please check that enemies obey the rules of damage and only deal damage by calling TakeDamage.");
        }
        
        //start iframes coroutine
        iFramesRoutine = StartCoroutine(IFramesCoroutine()); 
     
    }

    public IEnumerator IFramesCoroutine()
    {
        invincible = true;
        //wait for 0.67 seconds while invincible.
        yield return new WaitForSeconds(iFrameTime);
        //after 0.67 seconds become hittable again.
        invincible = false;

        //set iframes routine to null 
        //to indicate we have finished
        //as this will not happen automatically.
        iFramesRoutine = null;

        //exit coroutine.
        yield break;
    }

    bool isLowRes = false;

    Coroutine lowResRoutine = null;

    public void StartLowResRoutine()
    {
        if (isLowRes && lowResRoutine != null)
        {
            return;
        }

        lowResRoutine = StartCoroutine(LowResCoroutine());
    }

    public IEnumerator LowResCoroutine()
    {
        //say we are in low resolution
        isLowRes = true;

        //store previous layer
        int prevLayer = animatedModel.layer;

        for (int i = 0; i < animatedModel.transform.childCount; i++)
        {
            animatedModel.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("LowRes");
        }

        //set to be low resolution
        animatedModel.layer = LayerMask.NameToLayer("LowRes");

        //wait for .25 seconds
        yield return new WaitForSeconds(0.25f);

        //after waiting return to original layer
        animatedModel.layer = prevLayer;

        for (int i = 0; i < animatedModel.transform.childCount; i++)
        {
            animatedModel.transform.GetChild(i).gameObject.layer = prevLayer;
        }

        //say we are no longer low res.
        isLowRes = false;

        //set back to null
        //to indicate the coroutine has ended.
        lowResRoutine = null;

        yield return null;
    }

    #endregion


    private void OnDrawGizmos()
    {
        // cache previous Gizmos settings
        Color prevColor = Gizmos.color;
        Matrix4x4 prevMatrix = Gizmos.matrix;


        //Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, transform.rotation, transform.lossyScale);
        //Gizmos.matrix = rotationMatrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(/*transform.position + */(-transform.up * this.GetComponent<Collider>().bounds.size.y / 2) + (-transform.up * groundCheckDist), new Vector3(GetComponent<Collider>().bounds.size.x * groundCheckScale, 0.1f, GetComponent<Collider>().bounds.size.z * groundCheckScale));
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(/*transform.position + */(-transform.right * this.GetComponent<Collider>().bounds.size.x / 2), new Vector3(0.1f, GetComponent<Collider>().bounds.size.y * 2f / 3f, GetComponent<Collider>().bounds.size.z));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(/*transform.position + */(transform.right * this.GetComponent<Collider>().bounds.size.x / 2), new Vector3(0.1f, GetComponent<Collider>().bounds.size.y * 2f / 3f, GetComponent<Collider>().bounds.size.z));
        //Physics.BoxCast(transform.position, this.GetComponent<Collider>().bounds.size, -transform.up, Quaternion.identity, groundCheckDist, playerMask);

        // restore previous Gizmos settings
        Gizmos.color = prevColor;
        Gizmos.matrix = prevMatrix;
    }

    //draw health
    //for debug.
    void OnGUI()
    {
        string text = curHealth.ToString();
        int oldFontSize = GUI.skin.label.fontSize;
        GUI.skin.label.fontSize = 30;
        Vector3 position = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(position.x, Screen.height - position.y, textSize.x, textSize.y), text);
        GUI.skin.label.fontSize = oldFontSize;
    }
}

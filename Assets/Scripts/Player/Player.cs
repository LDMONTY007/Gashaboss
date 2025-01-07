using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Player : MonoBehaviour, IDamageable
{
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

    public bool invinicible = false;

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

    public float currentSpeed = 0f;

    public float moveSpeed = 5.0f;

    private Coroutine slowToStopCoroutine;

    //Variables for movement
    Vector2 moveInput = Vector3.zero;

    public Vector3 lastMoveVector = Vector3.zero;
    Vector3 moveVector;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public float jumpHeight = 5f;
    public float fallMultiplier = 9f;
    public float groundCheckDist = 0.1f;

    private Coroutine jumpCoroutine;

    public bool jumpPressed = false;

    public bool isJumping = false;

    public Collider playerCollider;

    public bool isGrounded = false;

    public Vector3 desiredMoveDirection;

    public Vector2 accumulatedVelocity = Vector2.zero;

    bool isOnWall = false;

    //Input actions
    InputAction moveAction;
    InputAction jumpAction;

    //Raycast vars

    LayerMask playerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get player mask
        playerMask = LayerMask.GetMask("Player", "Ignore Raycast"); //Assign our layer mask to player
        playerMask = ~playerMask; //Invert the layermask value so instead of being just the player it becomes every layer but the mask

        //get cam
        cam = Camera.main;

        //get move action
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

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

        //project controls to the camera's rotation so left and right are always the left and right sides of the camera.
        desiredMoveDirection = cam.transform.right * moveInput.x + cam.transform.forward * moveInput.y;
        moveVector = cam.transform.right * moveInput.x + cam.transform.forward * moveInput.y;
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

        jumpPressed |= jumpAction.IsPressed();

        if (isGrounded)
        {
            isJumping = false;
        }

        //dashPressed |= Input.GetKeyDown(KeyCode.LeftShift);

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

        HandleMovement();
        HandleJumping();
    }

    public void HandleRbRotation()
    {
        //rotate towards the velocity direction but don't rotate upwards.
        if (rb.linearVelocity != Vector3.zero)
            rb.MoveRotation(Quaternion.LookRotation(moveVector, transform.up));
    }

    public void HandleMovement()
    {


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
        /*        //Store yVelocity
                float yVel = rb.linearVelocity.y;
                rb.linearVelocity = moveVector;
                //Restore yVelocity
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, yVel, rb.linearVelocity.z);*/
        #endregion


        if (moveInput.magnitude == 0 && /*!grappling.IsGrappling() &&*/ !isJumping && isGrounded && /*!dashPressed &&*/ !jumpPressed)
        {
            //rb.linearVelocity *= 0;
            //Slow down very quickly but still make it look like it was gradual.
            if (slowToStopCoroutine == null)
            {
                slowToStopCoroutine = StartCoroutine(slowToStop());
            }
        }
        //Instant velocity changes depending on speed.
        //When we start moving we want to immediately go 
        //to our base walking speed so velocity change.
        //Then from there on out we slowly approach running.
        else if (isGrounded && /*!grappling.IsGrappling() &&*/ !isJumping && !jumpPressed)
        {
            // Calculate normalized time for acceleration and deceleration
            // float accelerationTime = currentSpeed / maxSpeed;
            // float decelerationTime = 1f - accelerationTime;
            float accelerationTime = currentInputMoveTime / timeToMaxSpeed;
            float decelerationTime = 1f - accelerationTime;

            // Apply custom acceleration curve
            currentSpeed = accelerationCurve.Evaluate(rb.linearVelocity.magnitude/*accelerationTime*/);

            // Apply custom speed curve
            // currentSpeed = speedCurve.Evaluate(accelerationTime);

            Debug.DrawRay(transform.position, transform.TransformDirection(rb.linearVelocity), Color.blue);

            //Vector3 targetVelocity = desiredMoveDirection.normalized * maxSpeed;

            // Evaluate speed using animation curve, they highest value in the curve is 1 so 1 * maxspeed = maxspeed. 
            // This is how we gradually approach our maxSpeed.
            //float targetSpeed = speedCurve.Evaluate(rb.linearVelocity.magnitude / maxSpeed) * maxSpeed;
            Vector3 targetVelocity = desiredMoveDirection.normalized * currentSpeed;
            //Vector3 targetVelocity = desiredMoveDirection.normalized * maxSpeed;

            //AccelerateToward(targetVelocity);
            // Determine current acceleration based on current speed
            //float acceleration = Mathf.Lerp(initialAcceleration, maxAcceleration, rb.linearVelocity.magnitude / maxSpeed);

            // Calculate current velocity in desired direction
            //Vector3 currentVelocity = Vector3.Project(rb.linearVelocity, new Vector3(moveInput.x, 0f, moveInput.y));

            //CounterVelocity();
            //Doing targetVelocity - rb.linearVelocity * rb.mass / Time.fixedDeltaTime gives us an acceleration of sorts.
            //This is what makes it so no matter how fast you turn the velocity isn't decreased.
            //The LateUpdate() call is what sets the direction of velocity to always face the direction we are wanting
            //to move. 

            //The only problem here is doing targetVelocity - rb.linearVelocity basically gets rid of any speed generated
            //by doing a grapple or dash. We want to perserve it somehow.
            //targetVelocity = targetVelocity.normalized * (targetVelocity.magnitude + (rb.linearVelocity.magnitude));
            Vector3 force = (targetVelocity/* - rb.linearVelocity*/)/* * rb.mass / Time.fixedDeltaTime*/;
            //force *= acceleration;
            //force = Vector3.ClampMagnitude(force, maxSpeed);
            //rb.AddForce(force, ForceMode.VelocityChange);
            rb.AddForce(force, ForceMode.Force);



            //rb.AddForce(movement, ForceMode.VelocityChange);
            //rb.AddForce(moveVector * movementMultiplier, ForceMode.Force);
            //rb.AddForce(AccumulatedVelocity, ForceMode.VelocityChange);
        }
        else
        {
            rb.AddForce(moveVector);
        }

        //rb.AddForce(moveVector - GetComponent<Rigidbody>().velocity, ForceMode.VelocityChange);
        //rb.AddForce(moveVector);

        //rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);


    }

    public void HandleJumping()
    {
        if (jumpPressed && isGrounded)
        {
            /*float*/
            jumpForce = Mathf.Sqrt(2f * -Physics.gravity.y * jumpHeight/*modJumpHeight*/) * rb.mass;
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
            isJumping = true;
        }

        if (isJumping)
        {

            //Where I learned this https://www.youtube.com/watch?v=7KiK0Aqtmzc
            //This is what gives us consistent fall velocity so that jumping has the correct arc.
            Vector2 localVel = transform.InverseTransformDirection(rb.linearVelocity);

            if (localVel.y < 0) //If we are in the air and at the top of the arc then apply our fall speed to make falling more game-like
            {
                //animator.SetBool("falling", true);
                //we don't multiply by mass because forceMode2D.Force includes that in it's calculation.
                //set gravity to be fallGravity.
                //gravity = fallGravity;
                Vector2 jumpVec = -transform.up * (fallMultiplier - 1) * 100f * Time.deltaTime;
                rb.AddForce(jumpVec, ForceMode.Force);
            }
        }
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
        if (isGrounded && !isJumping && /*!grappling.IsGrappling() && */desiredMoveDirection.normalized.sqrMagnitude > 0)
        {
            // Set the velocity directly to match the desired direction
            // Don't clamp the speed anymore as there isn't a good reason to do so.
            // Don't override the Y velocity.
            Debug.Log(desiredMoveDirection.normalized);
            //store the current y velocity
            float tempY = rb.linearVelocity.y;
            //remove the Y value from velocity before we apply that to the forward momentum so we don't "steal" values from the vertical
            //momentum and add them to the forward momentum.
            Vector3 velWithoutY = rb.linearVelocity - new Vector3(0f, tempY, 0f);
            rb.linearVelocity = desiredMoveDirection.normalized * velWithoutY.magnitude/*Mathf.Clamp(rb.linearVelocity.magnitude, 0f, maxSpeed)*/;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, tempY, rb.linearVelocity.z);
        }
    }

    private IEnumerator slowToStop()
    {
        Debug.LogWarning("Start Slowing To Stop!");
        while (rb.linearVelocity.magnitude > 0 && moveInput.magnitude == 0 && isGrounded /*&& !dashPressed*/)
        {
            Debug.LogWarning("Slowing To Stop!");
            //AccumulatedVelocity -= 0.05f * AccumulatedVelocity;
            rb.linearVelocity -= 0.05f * rb.linearVelocity;
            yield return new WaitForFixedUpdate();
        }
        slowToStopCoroutine = null;
    }

    #region unchanged by momentum mori code

    public void Die()
    {

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
        if (invinicible)
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
        if (invinicible == true && iFramesRoutine != null)
        {
            StopCoroutine(iFramesRoutine);
            invinicible = false;
            Debug.LogError("Player was damaged when in I-Frames, please check that enemies obey the rules of damage and only deal damage by calling TakeDamage.");
        }
        
        //start iframes coroutine
        iFramesRoutine = StartCoroutine(IFramesCoroutine()); 
     
    }

    public IEnumerator IFramesCoroutine()
    {
        invinicible = true;
        //wait for 0.67 seconds while invincible.
        yield return new WaitForSeconds(iFrameTime);
        //after 0.67 seconds become hittable again.
        invinicible = false;

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
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEditor.PlayerSettings;

public class BossController : MonoBehaviour, IDamageable, ICollectable
{
    [Header("Boss Specific Info")]
    public string bossName = "BaseBoss";
    public int coinsRewarded = 3;
    public int capsRewarded = 1;

    [Header("Manually assigned variables")]
    public TextMeshPro debugInfoTextMesh;

    public Collider bossCollider;

    public Animator animator;

    public ParticleSystem movementParticles;

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

            //LD Montello
            //Update the current health in the UI for the boss.
            UIManager.Instance.bossUIManager.UpdateHealthBar(_curHealth, maxHealth);

            if (curHealth == 0)
            {
                Die();
            }
        }
    }

    public void Die()
    {




        //TODO:
        //copy the animation rotation and such of the model's limbs
        //to the ragdoll.



        //disable the visual model for the character.
        //animatedModel.SetActive(false);

        //TODO:
        //Play some sort of death effect
        //where it falls to the ground then fades away

        isDead = true;

        //set color to be red when dead.
        //bossRenderer.material.color = Color.red;
        //TODO:
        //Play a death animation here.


        rb.constraints = RigidbodyConstraints.None;

        //TODO:
        //play the death animation for the boss.


        //turn off the boss UI.
        UIManager.Instance.SetBossUI(false);

        //reward the player with the loot
        //from this boss.
        playerObject.GetComponent<Player>().caps += capsRewarded;
        playerObject.GetComponent<Player>().curHealth += coinsRewarded;

        //Destroy the boss object after stopping all coroutines on this object
        StopAllCoroutines();
        Destroy(gameObject);
    }

    

    [HideInInspector]
    public bool isDead = false;

    #endregion

    //LD Montello
    //time to idle before making a decision
    //by default this is zero but will change 
    //depending on certain conditions.
    public float curIdleTime = 0f;

    public bool doStateMachine = false;

    [Header("Attack Parameters")]
    public float attackCheckRadius = 10f;


    [HideInInspector]
    public Rigidbody rb;

    [Header("Move Parameters")]
    public float moveSpeed = 5f;

    public float rotationSpeed = 20f;

    [Header("Collision Avoidance Parameters")]
    public float avoidanceForceMultiplier = 50f;
    public float avoidanceDistance = 30f;
    public float avoidanceAngle = 75f;
    
    bool isMoving = false;

    bool isShaking = false;

    public enum BossState
    {
        idle,
        attack,
        move,
        stun,
    }

    private BossState _curState = BossState.idle;

    //I turned current state into a getter setter
    //using a private property so that I can print
    //whenever it changes just to make sure that it
    //changes only when appropriate.
    //it also lets me change the debug info text to display the state.
    public BossState curState { 
        get { 
            return _curState; 
        } 
        set { 
            Debug.LogWarning("State switched from " + _curState + " to " + value);  
            _curState = value; 
            debugInfoTextMesh.text = _curState.ToString(); 

            //last resort for stopping the getCloseForAttackCoroutine,
            //which should be able to stop itself, and doesn't
            //because another coroutine starts which ends up changing the
            //state to stun and then from stun to move before getCloseForAttackCoroutine
            //gets control again, so it never gets a chance to check if it should stop.
            /*if (getCloseForAttackCoroutine != null && _curState == BossState.stun)
            {
                StopCoroutine(getCloseForAttackCoroutine);
                getCloseForAttackCoroutine = null;
            }*/
        } 
    }

    public GameObject animatedModel;

    public Weapon weapon;

    private MeshRenderer bossRenderer;

    public Transform lookTransform;
    private bool invincible;

    public GameObject playerObject;

    bool IsPlayerInAttackRange()
    {
        Collider[] objs = Physics.OverlapSphere(transform.position, attackCheckRadius);

        //if the player is in the attack range
        //return true. 
        if (objs.Any<Collider>(c => c.GetComponent<Player>() != null))
        {
            
            return true;
        }

        return false;
    }

    //LD Montello
    //called on start
    //to initialize all the base data in the UI.
    public void StartUI()
    {
        //turn on the boss UI.
        UIManager.Instance.SetBossUI(true);

        //LD Montello
        //Update the current health in the UI for the boss.
        UIManager.Instance.bossUIManager.UpdateHealthBar(curHealth, maxHealth);

        UIManager.Instance.bossUIManager.SetBossName(bossName);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartUI();

        playerObject = Player.instance.gameObject;

        bossRenderer = animatedModel.GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
        OnCollect();
    }

    // Update is called once per frame
    void Update()
    {
        if (doStateMachine)
        {
            HandleStateMachine();

            
        }

        //Handle the particle FX
        HandleParticles();

        //Handle the animations.
        HandleAnimation();
    }

    private void FixedUpdate()
    {
        //LD Montello
        //rotate towards the direction
        //we are moving in.
        HandleRbRotation();
    }

    public void SwitchToIdle(float idleTime)
    {
        //the time we should sit in the idle
        //state before we make a decision.
        curIdleTime = idleTime;
        curState = BossState.idle;
    }

    public virtual void HandleStateMachine()
    {
        #region boss state switching

        //old code to go straight to attacking,
        //but this doesn't make sense to do because
        //we want to allow the state to execute fully first.
        /*if (curState != BossState.attack)
        { 
            if (IsPlayerInAttackRange())
            {
                curState = BossState.attack;
            }
        }*/

        #endregion

        #region handling individual states

        switch (curState)
        {
            case BossState.idle:
                bossRenderer.material.color = Color.white;
                HandleIdle();
                break;
            case BossState.attack:
                bossRenderer.material.color = Color.red;
                HandleAttack();
                break;
            case BossState.move:
                bossRenderer.material.color = Color.blue;
                HandleMove();
                break;
            case BossState.stun:
                bossRenderer.material.color = Color.yellow;
                break;
        }

        #endregion
    }

    //LD Montello
    public virtual void HandleAnimation()
    {
        //This is where you'd check if rb.velocity is greater than some value and if it is, set the 
        //boss to walk animation.
    }

    public virtual void HandleParticles()
    {
        if (movementParticles != null)
        {
            if (!movementParticles.isPlaying && (isMoving || curState == BossState.move))
            {
                movementParticles.Play();
            }
            else if (movementParticles.isPlaying && !isMoving && curState != BossState.move)
            {
                movementParticles.Stop();
            }
        }
    }

    //Here we decide if we want to attack,
    //or if we want to try and get closer to the player.
    public void HandleIdle()
    {

        //LD Montello
        //if we're supposed to idle,
        //then decrement idle time and
        //return before making any decisions.
        if (curIdleTime > 0)
        {
            Debug.LogWarning("IDLE " + curIdleTime);
            curIdleTime -= Time.deltaTime;
            return;
        }
        else
        {
            Debug.LogWarning("EXIT IDLE " + curIdleTime);
        }


        if (IsPlayerInAttackRange())
        {
            Debug.Log("Boss is in attack range!".Color("orange"));
            curState = BossState.attack;
        }
        else
        {
            curState = BossState.move;
        }
    }


    MeleeAttack meleeAttack = new MeleeAttack();

    public void HandleAttack()
    {


        //TODO:
        //choose the attack based on our pattern
        //and execute it.
        //attacks should be a seperate script to make modular bosses and boss design easier.


        //if the boss isn't already in a melee attack,
        //then start one.
        if (!meleeAttack.active)
        {
            Debug.Log("Boss wants to attack here!".Color("Red"));
            StartCoroutine(meleeAttack.ActionCoroutine(this, 1f));
        }
        

    }

    private Coroutine getCloseForAttackCoroutine = null;

    public void HandleMove()
    {
        if (!isMoving)
        {
            //if the coroutine already ended
            //(isMoving has to be false to reach this conclusion)
            //then we need to kill the old coroutine
            //and start the new one.
            if (getCloseForAttackCoroutine != null)
            {
                StopCoroutine(getCloseForAttackCoroutine);
                getCloseForAttackCoroutine = null;
            }
            //Debug.Log("Boss is moving to player!".Color("Red"));
            //for now just move towards the player
            //StartCoroutine(MoveToPosition(playerObject.transform.position));
            getCloseForAttackCoroutine = StartCoroutine(GetCloseForAttack(moveSpeed));
        }
    }

    //LD Montello
    public void HandleRbRotation()
    {
        //rotate towards the velocity direction but don't rotate upwards.
        if (new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) != Vector3.zero)
        {
            //LD Montello
            //Rotation is locked on our rigidbody settings
            //so only code can rotate the object.
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z), transform.up), rotationSpeed));
        }



    }

    private void OnDrawGizmos()
    {
        //Store gizmos color.
        Color prevColor = Gizmos.color;

        //Draw a red sphere to show within
        //the radius the player must be for
        //our boss to initiate an attack.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackCheckRadius);

        //Set gizmos color back to the original color.
        Gizmos.color = prevColor;
    }

    //draw health for debug.
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


    /// <summary>
    /// LD Montello
    /// calculates the initial steering
    /// vector used for our steering algorithm.
    /// </summary>
    /// <param name="targetPos">position to move towards</param>
    /// <param name="speed">speed to move at</param>
    /// <returns></returns>
    public Vector3 CalculateSteering(Vector3 targetPos, float speed)
    {
        //calculate desired velocity
        Vector3 desiredVelocity = (targetPos - transform.position).normalized * speed;

        Vector3 steering = desiredVelocity - rb.linearVelocity;

        //remove y component.
        steering.y = 0;

        return steering;
    }

    //LD Montello
    //Calculates the collision
    //avoidance force rule
    //which is used in our steering algorithm.
    public Vector3 CollisionAvoidance()
    {
        Vector3 collisionAvoidanceForce = Vector3.zero;

        #region collision avoidance

        Vector3 ahead = rb.linearVelocity.normalized * avoidanceDistance;
        ahead.y = 0;
        Vector3 aheadWorld = transform.position + ahead;

        List<GameObject> hitObjs = new List<GameObject>();

        //we want to do the entire vertical size of the boss collider
        for (int i = 0; i < bossCollider.bounds.size.y; i++)
        {
            Vector3 rayOrigin = transform.position + new Vector3(0, i - bossCollider.bounds.size.y / 2, 0);

            Debug.DrawRay(rayOrigin, ahead, Color.red);

            //number of rays to shoot out in our cone like shape
            int rayCount = 10;

            for (int j = 0; j < rayCount; j++)
            {
                //we want the left most
                //angle to start
                //on the left side
                //of the up vector
                //and end on the ride side,
                //so we need to do (180 - avoidanceAngle) / 2
                //to get our offset in the 2 quadrant range
                //where we want to shoot rays.
                //then we add that offset
                //to our character's current rotation
                //angle to get a start angle.
                //then we use our start angle
                //plus our current angle along
                //the FOV to generate the desired
                //direction vector for the ray.
                float startAngle = transform.rotation.eulerAngles.y + 180f + avoidanceAngle / 2f;
                float angle = (avoidanceAngle) * j / rayCount;
                Vector2 dir2 = LDUtil.AngleToDir2D(-1 * (startAngle + angle)).normalized;
                Vector3 dir3 = new Vector3(dir2.x, 0f, dir2.y);
                //Debug.DrawRay(rayOrigin, dir3 * avoidanceDistance * 2, Color.green, 1f);

                //ContactFilter2D contactFilter = new ContactFilter2D();

                //List<RaycastHit2D> results = new List<RaycastHit2D>();

                //Raycast while ignoring
                //Player and Ignore Raycast layers.
                //bossCollider.Raycast(dir3, contactFilter, results, distance * 2);
                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, dir3, avoidanceDistance, ~LayerMask.GetMask("Player", "Ignore Raycast"));

                //loop through all objects that were hit by the raycastAll
                for (int k = 0; k < hits.Length; k++)
                {
                    //if we hit ourselves ignore for collision avoidance. 
                    if (hits[k].collider.gameObject == gameObject)
                    { continue; }


                    //Draw a ray that matches the distance
                    //of what we hit.
                    Debug.DrawRay(transform.position, dir3 * hits[k].distance, Color.green);

                    //add to the list of hit objects.
                    hitObjs.Add(hits[k].collider.gameObject);

                    //avoidance force is the 
                    //vector from the end of the ray hit for the current direction,
                    //to the position of the hit object
                    //and this will give us a direction to push away from the object.
                    Vector3 avoidanceForce = (rayOrigin + dir3) - hits[k].collider.gameObject.transform.position;
                    avoidanceForce = avoidanceForce.normalized * avoidanceForceMultiplier;
                    //remove y component.
                    avoidanceForce.y = 0;
                    Debug.DrawRay(rayOrigin, avoidanceForce, Color.cyan);

                    //add avoidance force to our steering.
                    collisionAvoidanceForce += avoidanceForce;
                }
            }
        }



        #endregion

        //remove y component.
        collisionAvoidanceForce.y = 0;

        return collisionAvoidanceForce;
    }

    //LD Montello
    /// <summary>
    /// Moves from current position to target position given a speed value. 
    /// </summary>
    /// <param name="targetPos">Position to move to.</param>
    /// <param name="speed">Speed to move at. Defaults to 10, can be overridden</param>
    /// <param name="targetAccuracy">Range we must be within before stopping movement.</param>
    /// <returns></returns>
    public IEnumerator MoveToPosition(Vector3 targetPos, float speed = 10f, float targetAccuracy = 5f, float idleTime = 0f)
    {
        isMoving = true;
        
        curState = BossState.move;


        float moveTime = 0f;

        //Shake to telegraph
        //the attack so the player knows it's happening
        //then after the whole coroutine executes
        //we can charge.
        //yield return ShakeCoroutine();
        ShakeModel();

        while (isShaking)
        {
            //if we're not in the move state
            //anymore than stop moving.
            if (curState != BossState.move)
            {
                isMoving = false;
                yield break;
            }
            moveTime += Time.deltaTime;
            yield return null;
        }



        //Loop till we reach our position
        //and only stop if we reach it or we hit an object.
        while ((targetPos - transform.position).magnitude > targetAccuracy && curState != BossState.stun)
        {
            //if we're not in the move state
            //anymore than stop moving.
            if (curState != BossState.move)
            {
                //Stop moving.
                rb.linearVelocity = Vector3.zero;
                isMoving = false;
                yield break;
            }
            moveTime += Time.deltaTime;

            //calculate initial steering
            Vector3 steering = CalculateSteering(targetPos, speed);

            //Add the collision avoidance rule
            steering += CollisionAvoidance();

            //add steering to current velocity.
            rb.linearVelocity += steering;
            //clamp linear velocity to the current given speed.
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, speed);

            //LD Montello,
            //for movement to be consistent we need to wait
            //for fixed update to move.
            yield return new WaitForFixedUpdate();
        }


        //if we reach our position or we are stunned
        //then stop moving.
        rb.linearVelocity = Vector3.zero;

        //if we weren't 
        //stunned then go to the default
        //state so the attack pattern can
        //reset.
        /*        if (curState != BossState.Stun)
                {
                    curState = BossState.None;
                    //Set the position to match our target
                    //because we were close enough.
                    //transform.position = targetPos;
                }*/

        isMoving = false;

        Debug.LogWarning(moveTime);

        //say we are no longer moving
        //and need to make a decision.
        SwitchToIdle(idleTime);
    }


    //continously moves towards player until 
    //the boss is in attack range.
    public IEnumerator GetCloseForAttack(float speed)
    {
        curState = BossState.move;
        isMoving = true;

        //while the player isn't
        //close enough to be attacked,
        //walk closer to them.
        while (!IsPlayerInAttackRange() || Vector3.Distance(playerObject.transform.position, transform.position) >= weapon.attackDistance)
        {
            //if we're not in the move state
            //anymore than stop moving.
            //this is usually caused when a player
            //hits us and we end up entering stun.
            if (curState != BossState.move)
            {
                //Stop moving.
                rb.linearVelocity = Vector3.zero;
                isMoving = false;
                yield break;
            }

            //calculate initial steering
            Vector3 steering = CalculateSteering(playerObject.transform.position, speed);
            
            //Add the collision avoidance rule
            steering += CollisionAvoidance();

            //add steering to current velocity.
            rb.linearVelocity += steering;
            //clamp linear velocity to the current given speed.
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, speed);

            yield return new WaitForFixedUpdate();
        }

        //TODO:
        //Look at the player after moving
        //and then switch to attacking 
        //so we guaranteed hit them.

        //Stop moving.
        rb.linearVelocity = Vector3.zero;
        isMoving = false;

        Debug.Log("Move to attack finished");

        //when they are in the attack range,
        //switch to the attack state.
        curState = BossState.attack;
        //Debug.Break();
    }

    //Just a small shake 
    //animation for the sprite object
    //so I can use this
    //when they get hit.
    public void ShakeModel()
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine());
        }
    }

    private IEnumerator ShakeCoroutine()
    {
        isShaking = true;

        float total = 0.3f;
        float currentTime = 0f;

        //Sleep for 50 ms.
        float sleepTime = 0.05f;

        //Cache the current position of the model
        //incase it isn't Vector3.zero
        Vector3 modelPos = animatedModel.transform.localPosition;

        int sign = 1;
        float frequency = 5f;

        //Set startTime
        float startTime = Time.timeSinceLevelLoad;

        while (currentTime < total)
        {
            //Add the time since start time
            currentTime += Time.timeSinceLevelLoad - startTime;

            //offset the model one direction this frame
            //the opposite direction the next frame,
            //so it looks like it's shaking
            //left and right.
            animatedModel.transform.localPosition = modelPos + lookTransform.right.normalized * sign * frequency * Time.deltaTime;

            //invert direction to immitate shaking
            sign *= -1;


            //Set start time before we wait for 
            //sleepTime so that when control returns
            //to us the total amount of time we spend shaking
            //will still be equivalent to the total time given.
            startTime = Time.timeSinceLevelLoad;
            yield return new WaitForSeconds(sleepTime);
        }



        //Set back to original position.
        animatedModel.transform.localPosition = modelPos;

        isShaking = false;

        yield break;
    }

    public void TakeDamage(int d, GameObject other)
    {
        //if we're invincible, 
        //then exit this method.
        //or if we're dead don't
        //take damage
        if (invincible || isDead)
        {
            return;
        }

        curHealth -= d;

        //idle for a few moments before moving again.
        //SwitchToIdle(1f);


        //print out data about the boss taking damage.
        Debug.Log("Boss Took: ".Color("Orange") + d.ToString().Color("Red") + " from " + other.transform.root.name.Color("Blue"));

        //When the boss takes damage,
        //put them in the stun state
        //to cancel any movements.
        curState = BossState.stun;


        //Start the shaking animation
        ShakeModel();

        //Set to be low resolution for a small amount of time.
        StartLowResRoutine();


        if (curIFramesRoutine == null)
        {
            //LD Montello
            //Stun the boss temporarily.
            curIFramesRoutine = StartCoroutine(IFramesCoroutine(1f));
        }
        else
        {
            Debug.LogError("PREVIOUS IFRAMES HAVEN'T FINISHED");
        }

        
    }

    Coroutine curIFramesRoutine = null;

    //This is where we do the iframes code
    public IEnumerator IFramesCoroutine(float iFrameTime)
    {
        //curState = BossState.stun;

        //set to be invincible so they
        //can't be damaged during iframes
        invincible = true;

        //After being in iframes just dash away from the player.

        //LD Montello
        //dash away from the player.
        DashAwayMove dashAwayMove = new DashAwayMove();
        /*        dashAwayMove.Execute(this, 1f);*/

        //we need to wait for 
        //other code to check state changes
        //since the state changed to stun earlier.
        //This is because we return the dash move
        //which changes back to the move state,
        //so if we don't give control back to other
        //coroutines and update then they never
        //get a chance to see we are in stun
        //and need to stop execution.
        yield return new WaitForFixedUpdate();

        yield return dashAwayMove.ActionCoroutine(this, 1f);

        float total = iFrameTime;
        float curTime = 0f;

        //cooldown for the sprite flickering.
        float flickerCooldown = 0.2f;


        //wait for the total iFrame time before
        //leaving invincibility.
        //Also flicker the 3D model while we do this.
        while (curTime < total)
        {
            curTime += Time.deltaTime;

            //used to directly turn off the renderer
            //but now we'll turn off the animated model gameobject itself.
            //bossRenderer.enabled = !bossRenderer.enabled;
            animatedModel.gameObject.SetActive(!animatedModel.gameObject.activeSelf);


            //wait until the cooldown to do the sprite flicker again.
            yield return new WaitForSeconds(flickerCooldown);
            //add the flicker cooldown to account for the time
            //we waited.
            curTime += flickerCooldown;

            /*//If we died, stop blinking.
            if (isDead)
            {
                break;
            }*/
            
            Debug.LogWarning("FLICKER: " + curTime);

            //wait until the cooldown to do the sprite flicker again.
            yield return null;
        }

        //set the animated model to be visible again.
        animatedModel.gameObject.SetActive(true);






        /*        //while we are dashing away, 
                //wait before leaving i-frames.
                while (isMoving)
                {
                    yield return null;
                }*/

        //after the stun the boss is no longer
        //invincible. 
        //while invincible display I-frames.
        invincible = false;

        curIFramesRoutine = null;
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
    public void OnCollect(){
        
    }
}

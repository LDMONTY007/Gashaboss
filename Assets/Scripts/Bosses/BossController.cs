using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;

public class BossController : Collectible, IDamageable
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

    public ParticleSystem stunParticles;

    public GachaMachine parentMachine;

    public Transform launchTransform; // used for bosses that need projectiles spawned from a specific point

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
        OnCollect(); // add to collections before destroying

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

        if (parentMachine != null)
            //the boss was defeated so tell the parent machine this.
            parentMachine.OnBossDefeated();

        // Check if the player has a discount voucher and refresh it after boss defeat
        VoucherEffect voucher = playerObject.GetComponent<Player>().GetComponent<VoucherEffect>();
        if (voucher != null)
        {
            // Reset the discount after boss defeat
            voucher.hasDiscount = true;
            Debug.Log("Caps discount voucher refreshed after boss defeat!");
        }

        // Check if player has GachaGambit effect active and expire it after boss defeat
        GachaGambitEffect gambitEffect = playerObject.GetComponent<Player>().GetComponent<GachaGambitEffect>();
        if (gambitEffect != null)
        {
            gambitEffect.OnBossDefeated();
        }

        // Destroy the boss object after stopping all coroutines on this object
        StopAllCoroutines();
        Destroy(gameObject);
    }



    [HideInInspector]
    public bool isDead = false;

    #endregion

    public float bounceForce = 20f;

    public float stunTime = 0.5f;

    // Variables for stun immunity
    private bool isStunImmune = false;

    [Tooltip("Time between stuns where boss can't be stunned again (seconds)")]
    public float stunImmunityTime = 3f; // Adjust this value as needed

    //LD Montello
    //time to idle before making a decision
    //by default this is zero but will change 
    //depending on certain conditions.
    public float curIdleTime = 0f;

    public bool doStateMachine = false;

    [Header("Attack Parameters")]
    private float attackCheckRadius = 10f;



    [HideInInspector]
    public Rigidbody rb;

    [Header("Move Parameters")]
    public float moveSpeed = 5f;
    public float minStopDist = 1f;

    [Header("Collision Avoidance Parameters")]
    public float avoidanceForceMultiplier = 50f;
    public float avoidanceDistance = 30f;
    public float avoidanceAngle = 75f;

    // incoming dmg multiplier - Chris Li (for items)
    public float incomingDamageMultiplier = 1.0f;

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

            if (value == BossState.move)
            {
                //only freeze rotation when moving, not position.
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
            else
            {
                //otherwise freeze position and rotation.
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

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

    public BossWeapon weapon;

    private MeshRenderer bossRenderer;

    public Transform lookTransform;
    private bool invincible;

    public GameObject playerObject;

    // 1. Fix player visibility check in IsPlayerInAttackRange():
    bool IsPlayerInAttackRange()
    {
        // If player is invisible, they can't be detected
        if (!Player.instance.isVisible)
            return false;

        Collider[] objs = Physics.OverlapSphere(transform.position, attackCheckRadius);

        // If the player is in the attack range, return true
        foreach (Collider c in objs)
        {
            if (c.GetComponent<Player>() != null)
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
        //start with the max health for this boss.
        curHealth = maxHealth;

        StartUI();

        playerObject = Player.instance.gameObject;

        bossRenderer = animatedModel.GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
        SwitchToIdle(2f);
        StartCoroutine(IFramesCoroutine(1.5f));
    }

    public Vector3 GetPathablePoint(Vector3 desiredPoint, float searchRadius)
    {
        Vector3 foundPoint = Vector3.zero;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(desiredPoint, out hit, searchRadius, NavMesh.AllAreas))
        {
            foundPoint = hit.position;
            Debug.Log("Pathable Point Found: " + foundPoint);
        }
        else
        {
            Debug.Log("No pathable point found within radius.");
        }

        return foundPoint;
    }

    public float groundCheckDist = 0.97f;
    public float groundCheckScale = 0.4f;
    public bool isGrounded = false;

    private bool didLand = true;

    // Update is called once per frame
    void Update()
    {
        #region grounded check
        Collider[] colliders = Physics.OverlapBox(transform.position + (-transform.up * this.GetComponent<Collider>().bounds.size.y / 2) + (-transform.up * groundCheckDist), new Vector3(GetComponent<Collider>().bounds.size.x * groundCheckScale, 0.1f, GetComponent<Collider>().bounds.size.z * groundCheckScale), transform.rotation);
        if (colliders.Length > 0 && (colliders.Contains(GetComponent<Collider>()) && colliders.Length != 1))
        {
            //if we were jumping or in the air,
            //then we landed.
            if (!didLand)
            {
                didLand = true;
                OnLanded();
            }

            isGrounded = true;
            //Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red, 1f);

            //Call on landed.

        }
        else
        {
            //when we are no longer grounded,
            //say that we didn't land.
            if (didLand == true)
            {
                didLand = false;
            }

            isGrounded = false;
        }
        #endregion

        if (doStateMachine)
        {
            //Debug.LogWarning("Next: " + nextAttack.didExecute + " , " + nextAttack.active);

            //if the next attack has already ended then 
            //set it to null so we can get a new next attack
            //in the next Idle state.
            if (nextAttack != null && nextAttack.didExecute)
            {
                //reset the attack's didExecute flag.
                nextAttack.didExecute = false;
                nextAttack = null;
            }

            HandleStateMachine();
        }

        //Handle the particle FX
        HandleParticles();

        //Handle the animations.
        HandleAnimation();
    }

    public void OnLanded()
    {
        //TODO: Play landing particles here.
    }

    private void FixedUpdate()
    {
        //LD Montello
        //rotate towards the direction
        //we are moving in.
        HandleRbRotation();
    }

    void LateUpdate()
    {
        ApplyFinalMovements();
    }



    /// <summary>
    /// Handles applying final data to the rigidbody.
    /// For example if the boss is in the air make sure we don't
    /// freeze them on the Z axis and let gravity effect them.
    /// </summary>
    public virtual void ApplyFinalMovements()
    {
        

        //when we aren't doing some kind of move, 
        //the boss can't fall unless we check here and allow them to fall.
        //we freeze the position otherwise. 
        //we just allow gravity to take over
        //so that it can fall back to the ground.
        if (!manualRotation && curState != BossState.stun && curState != BossState.move && !isGrounded)
        {
            //freeze all rotation and only allow movement on the y axis.
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        }
    }

    public void SwitchToIdle(float idleTime)
    {
        //the time we should sit in the idle
        //state before we make a decision.
        curIdleTime = idleTime;
        curState = BossState.idle;
    }

    // 3. Fix HandleStateMachine to ensure bosses can recover when needed:
    public virtual void HandleStateMachine()
    {
        // Add periodic check to force recovery if boss seems stuck
        if (Time.frameCount % 300 == 0) // Every ~5 seconds at 60fps
        {
            // Check if we've been in the same state for too long
            // Implementation depends on how you want to track state duration

            // For now, just ensure nextAttack is properly cleared if it's stuck
            if (nextAttack != null && nextAttack.didExecute)
            {
                nextAttack.didExecute = false;
                nextAttack = null;
            }
        }

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

        if (stunParticles != null)
        {
            if (!stunParticles.isPlaying && curState == BossState.stun)
            {
                stunParticles.Play();
            }
            else if (stunParticles.isPlaying && curState != BossState.stun)
            {
                stunParticles.Stop();
                //make sure all the particles are deleted from the system.
                stunParticles.Clear();
                Debug.LogWarning("STOPPED");
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

        //no leaving the stun state early.
        if (curState != BossState.stun)
        {
            //if  the next attack is null,
            //get the next attack before checking the attack range
            //as different attacks will change the attack range.
            if (nextAttack == null)
            {
                GetNextAttack();
            }

            //Then check the attack range
            //to see if the next attack is 
            //ready to be used.
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

    }


    MeleeAttack meleeAttack = new MeleeAttack();
    AltAttack altAttack = new AltAttack();
    SpecialAttack specialAttack = new SpecialAttack();

    private BossAction _nextAttack;

    public BossAction nextAttack {  get { return _nextAttack; } set { _nextAttack = value; if (value != null) Debug.Log("Set Next Attack: " + value.ToString()); } }

    public void GetNextAttack()
    {
        //if the boss isn't already attacking,
        //then start one.
        if (!meleeAttack.active && !altAttack.active && !specialAttack.active)
        {
            // Temp logic for attack handling, for now we'll just pick a random attack out of the three options
            // Weapons will just default to melee attacks, if a alt or special attack isn't available anyway, so this should operate fine
            int randAttack = UnityEngine.Random.Range(0, 3);
            Debug.Log("Boss wants to attack here!".Color("Red"));
            switch (randAttack)
            {
                case 0:
                    nextAttack = meleeAttack;
                    //Set the attack check radius to be that of the normal attack.
                    attackCheckRadius = weapon.atkCheckRadius;
                    break;
                case 1:
                    // if boss doesn't have an alt, use a reg attack instead
                    if (!weapon.hasAlt)
                    {
                        nextAttack = meleeAttack;
                        //Set the attack check radius to be that of the normal attack.
                        attackCheckRadius = weapon.atkCheckRadius;
                        break;
                    }
                    nextAttack = altAttack;
                    //Set the attack check radius to be that of the alt attack.
                    attackCheckRadius = weapon.altAtkCheckRadius;
                    break;
                case 2:
                    // if boss doesn't have an alt or a special, use a reg attack instead
                    if (!weapon.hasAlt && !weapon.hasSpecial)
                    {
                        nextAttack = meleeAttack;
                        //Set the attack check radius to be that of the normal attack.
                        attackCheckRadius = weapon.atkCheckRadius;
                        break;
                    }
                    // if boss doesn't have a special, use an alt attack instead
                    if (!weapon.hasSpecial)
                    {
                        nextAttack = altAttack;
                        //Set the attack check radius to be that of the alt attack.
                        attackCheckRadius = weapon.altAtkCheckRadius;
                        break;
                    }
                    nextAttack = specialAttack;
                    //Set the attack check radius to be that of the special attack.
                    attackCheckRadius = weapon.specialAtkCheckRadius;
                    break;
            }
        }
    }

    public void HandleAttack()
    {
       //if we have our next attack and aren't attacking, execute it.
       if (nextAttack != null && !meleeAttack.active && !altAttack.active && !specialAttack.active)
       {
            StartCoroutine(nextAttack.ActionCoroutine(this, 1f));
       }
    }

    private Coroutine getCloseForAttackCoroutine = null;
    public Vector3 GetFeetPosition()
    {
        return this.GetComponent<Collider>().bounds.center + (-transform.up * this.GetComponent<Collider>().bounds.size.y / 2);
    }
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

    public bool manualRotation = false;

    //LD Montello
    public void HandleRbRotation()
    {
        //rotate towards the velocity direction but don't rotate upwards.
        if (!manualRotation && new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) != Vector3.zero)
        {
            //LD Montello
            //Rotation is locked on our rigidbody settings
            //so only code can rotate the object.
            rb.MoveRotation(Quaternion.LookRotation(new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z), transform.up));
        }

        

    }

    public void LookAtPlayer()
    {
        RigidbodyConstraints prevConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        Vector3 rotDir = (new Vector3(Player.instance.transform.position.x, 0, Player.instance.transform.position.z) - transform.position).normalized;
        rotDir.y = 0;

        //LD Montello
        //Rotation is locked on our rigidbody settings
        //so only code can rotate the object.
        rb.MoveRotation(Quaternion.LookRotation(rotDir, transform.up));

        rb.constraints = prevConstraints;

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

    //old, do not use --LD Montello.
    //draw health for debug.
/*    void OnGUI()
    {
        string text = curHealth.ToString();
        int oldFontSize = GUI.skin.label.fontSize;
        GUI.skin.label.fontSize = 30;
        Vector3 position = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(position.x, Screen.height - position.y, textSize.x, textSize.y), text);
        GUI.skin.label.fontSize = oldFontSize;
    }*/


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
    //finds a position 
    //that the boss can path to 
    //from their current position
    //from the current angle
    public bool GetPathablePosition(float desiredDist, ref Vector3 foundPos)
    {

        float angleIncrement = 10f;

        float leftAngle = 0f;
        float rightAngle = 0f;

        bool checkLeftAngle = false;

        while (leftAngle < 180 && rightAngle < 180)
        {

            Vector2 tempVec = Vector2.zero;

            //switch between which angle we are checking
            //based off of the current side we check.
            //so we're alternating the side we check on.
            if (checkLeftAngle)
            {
                //we subtract the left angle
                //from the current rotation to find 
                //the world rotation.
                //LD Note: we subtract 90 from the y angle 
                //because the boss's forward looking angle is 180 
                tempVec = LDUtil.AngleToDir2D((transform.rotation.eulerAngles.y - 90) - leftAngle);
                //increment search angle.
                leftAngle += angleIncrement;
            }
            else
            {
                //we add the right angle
                //from the current rotation to find 
                //the world rotation.
                tempVec = LDUtil.AngleToDir2D((transform.rotation.eulerAngles.y - 90) + rightAngle);
                //increment search angle.
                rightAngle += angleIncrement;
            }


            //get the angle as a direction vector.
            Vector3 dir = new Vector3(tempVec.x, 0f, tempVec.y);
            //get the point relative to the boss's position 
            //at the distance to check if there is anything colliding there.
            //Vector3 pointToCheck = transform.position + dir.normalized * desiredDist;

            RaycastHit[] hits = Physics.SphereCastAll(transform.position, (bossCollider as CapsuleCollider).radius, dir, desiredDist);

            if (hits.Length > 0)
            {
                //if we hit ourself and nothing else, then
                //we consider this a pathable position.
                if (hits.Length == 1 && hits[0].collider.gameObject == gameObject)
                {
                    //set the found position
                    foundPos = transform.position + dir.normalized * desiredDist;

                    //Draw debug ray
                    Debug.DrawLine(transform.position, foundPos, Color.green);

                    //say we found a position successfully. 
                    return true;
                }
                Debug.DrawLine(transform.position, hits[0].point, checkLeftAngle ? Color.red : Color.blue);
            }
            //We didn't hit anything so this point is valid for pathing.
            else
            {
                //set the found position
                foundPos = transform.position + dir.normalized * desiredDist;

                //Draw debug ray
                Debug.DrawLine(transform.position, foundPos, Color.green);

                //say we found a position successfully. 
                return true;
            }

            checkLeftAngle = !checkLeftAngle;
        }

        //return false when we haven't
        //found a proper position.
        foundPos = Vector3.zero;
        return false;
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
            //this is usually a loop that bosses can get stuck in, if that's the case we should have
            //a fall back where they stop doing this movement and restart pathing to the player.

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
    // 2. Fix GetCloseForAttack to handle edge cases better:
    public IEnumerator GetCloseForAttack(float speed)
    {
        curState = BossState.move;
        isMoving = true;

        // Add timeout to prevent infinite loop
        float timeout = 15.0f; // 15 seconds max
        float timeElapsed = 0f;

        // While the player isn't close enough to be attacked or is invisible,
        // walk closer to them (if visible) or just wait (if invisible)
        while ((!IsPlayerInAttackRange() ||
                Vector3.Distance(playerObject.transform.position, transform.position) >= weapon.attackDistance) &&
               timeElapsed < timeout)
        {
            timeElapsed += Time.deltaTime;

            // If we've been chasing too long, reset state to idle to make a new decision
            if (timeElapsed >= timeout)
            {
                Debug.LogWarning($"{bossName} chase timeout - resetting state");
                rb.linearVelocity = Vector3.zero;
                isMoving = false;
                SwitchToIdle(1f);
                yield break;
            }

            // If player is invisible, don't chase - go back to idle
            if (!Player.instance.isVisible)
            {
                rb.linearVelocity = Vector3.zero;
                isMoving = false;
                SwitchToIdle(1f);
                yield break;
            }

            // If we're not in the move state anymore, stop moving
            if (curState != BossState.move)
            {
                rb.linearVelocity = Vector3.zero;
                isMoving = false;
                yield break;
            }

            // Check if we need to find a new path (in case we're stuck)
            if (rb.linearVelocity.magnitude < 0.1f)
            {
                // Try to find an alternative path
                Vector3 newPosition = Vector3.zero;
                if (GetPathablePosition(5f, ref newPosition))
                {
                    // Move to this intermediate position first
                    yield return StartCoroutine(MoveToPosition(newPosition, speed, 1f, 0.2f));
                    continue;
                }
            }

            // Calculate initial steering
            Vector3 steering = CalculateSteering(playerObject.transform.position, speed);

            // Add the collision avoidance rule
            steering += CollisionAvoidance();

            // Add steering to current velocity
            rb.linearVelocity += steering;

            // Clamp linear velocity to the current given speed
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, speed);

            yield return new WaitForFixedUpdate();
        }

        // Stop moving
        rb.linearVelocity = Vector3.zero;
        isMoving = false;

        Debug.Log("Move to attack finished");

        // When they are in the attack range, switch to the attack state
        if (curState == BossState.move) // Only switch if we're still in move state
        {
            curState = BossState.attack;
        }
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

        //curHealth -= d; adjusted takeDamage code for items multipliers
        int modifiedDamage = Mathf.RoundToInt(d * incomingDamageMultiplier);
        curHealth -= modifiedDamage;

        // Check if this is DOT damage - DOT damage doesn't cause stun
        bool isDotDamage = other != null && other.GetComponent<DotDamageMarker>() != null;

        //idle for a few moments before moving again.
        //SwitchToIdle(1f);


        //print out data about the boss taking damage. (updated debug code)
        //Debug.Log("Boss Took: ".Color("Orange") + d.ToString().Color("Red") + " from " + other.transform.root.name.Color("Blue"));
        Debug.Log("Boss Took: ".Color("Orange") + modifiedDamage.ToString().Color("Red") + " from " + other.transform.root.name.Color("Blue"));

        //When the boss takes damage,
        //put them in the stun state if not immune
        //to cancel any movements.
        //When the boss takes damage, put them in stun state if not immune
        if (!isDotDamage && !isStunImmune)
        {
            //When the boss takes damage,
            //put them in the stun state
            //to cancel any movements.
            curState = BossState.stun;

            //Start the shaking animation
            ShakeModel();

            //Set to be low resolution for a small amount of time.
            StartLowResRoutine();

            //rotate -1 degrees away from the player so we get bounced at an angle away from it.
            Vector3 vectorAngle = LDUtil.RotateVectorAroundAxis((transform.position - other.transform.position).normalized, other.transform.right, -5);

            //bounce the boss away from the player.
            //this is how we simulate knockback.
            StartCoroutine(BounceCoroutine(vectorAngle, bounceForce));

            //LD Montello
            //Stun the boss for stunTime
            StartCoroutine(StunCoroutine(stunTime));
        }
        else
        {
            Debug.Log($"{bossName} is currently immune to stun!");
        }
    }

    public IEnumerator BounceCoroutine(Vector3 direction, float force)
    {
        Vector3 startForce = direction * force;

        //stop the boss
        rb.linearVelocity = Vector3.zero;

        float bounceTime = 0.5f;
        float curTime = 0f;

        if (curState == BossState.stun)
        {
            //don't freeze position anymore
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }


        while (curTime <= bounceTime)
        {
            //we need to wait for fixed update so that this can
            //properly be applied to the boss.
            yield return new WaitForFixedUpdate();

            rb.linearVelocity = startForce;

            curTime += Time.deltaTime;
        }


        rb.linearVelocity = Vector3.zero;

        if (curState == BossState.stun)
        {
            //go back to freezing position
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }


        //this only works while the boss is stunned,
        //they can escape being knocked back much quicker if they
        //have less i-frame time.
        //rb.AddForce(direction * force, ForceMode.Impulse);
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
        // Moth
        // Let's not dash away at the start of the Iframe, we aint making no punk ass bosses
        //DashAwayMove dashAwayMove = new DashAwayMove();
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

        //yield return dashAwayMove.ActionCoroutine(this, 1f);

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

    // Modified StunCoroutine with immunity cooldown
    public IEnumerator StunCoroutine(float stunTime)
    {
        isStunImmune = true; // Boss is immune during stun and cooldown

        //We are already in stun,
        //so wait some amount of time before exiting stun.
        yield return new WaitForSeconds(stunTime);

        // Exit stun state
        curState = BossState.idle;

        Debug.Log($"{bossName} recovered from stun. Immune to stun for {stunImmunityTime} seconds.");

        // Wait for immunity cooldown
        yield return new WaitForSeconds(stunImmunityTime);

        // End immunity
        isStunImmune = false;
        Debug.Log($"{bossName} is now vulnerable to stun again.");
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
    public override void OnCollect(){
        CollectionManager.instance.AddToCollection(this);
    }

    // WaitForSEcondsRoutine Method
    private IEnumerator WaitForSecondsRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}

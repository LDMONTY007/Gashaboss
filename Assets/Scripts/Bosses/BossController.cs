using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class BossController : MonoBehaviour, IDamageable
{
    [Header("Manually assigned variables")]
    public TextMeshPro debugInfoTextMesh;


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
        bossRenderer.material.color = Color.red;
        rb.constraints = RigidbodyConstraints.None;

        //TODO:
        //play the death animation for the boss.
        
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



    Rigidbody rb;

    [Header("Move Parameters")]
    public float moveSpeed = 5f;

    public float rotationSpeed = 20f;

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
    public BossState curState { get { return _curState; } set { Debug.LogWarning("State switched from " + _curState + " to " + value);  _curState = value; debugInfoTextMesh.text = _curState.ToString(); } }

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerObject = Player.instance.gameObject;

        bossRenderer = animatedModel.GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (doStateMachine)
        {
            HandleStateMachine();

            
        }
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

    public void HandleStateMachine()
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

    public void HandleAttack()
    {
        Debug.Log("Boss wants to attack here!".Color("Red"));

        //TODO:
        //choose the attack based on our pattern
        //and execute it.
        //attacks should be a seperate script to make modular bosses and boss design easier.
        


        MeleeAttack meleeAttack = new MeleeAttack();
        StartCoroutine(meleeAttack.ActionCoroutine(this, 1f));

        //at the end of an attack coroutine
        //always set the state back to idle
        //curState = BossState.idle;

    }

    public void HandleMove()
    {
        if (!isMoving)
        {
            //Debug.Log("Boss is moving to player!".Color("Red"));
            //for now just move towards the player
            //StartCoroutine(MoveToPosition(playerObject.transform.position));
            StartCoroutine(GetCloseForAttack(moveSpeed));
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
        curState = BossState.move;

        isMoving = true;

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
                isMoving = false;
                yield break;
            }
            moveTime += Time.deltaTime;
            rb.linearVelocity = (targetPos - transform.position).normalized * speed;

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

        //if we're not in the move state
        //anymore than stop moving.
        //this prevents the following
        //"SwitchToIdle" call from breaking
        //an ongoing "SwitchToIdle" call
        //which is meant to cancel this movement.
        if (curState != BossState.move)
        {
            isMoving = false;
            yield break;
        }

        //say we are no longer moving
        //and need to make a decision.
        SwitchToIdle(idleTime);
    }


    public IEnumerator GetCloseForAttack(float speed)
    {
        curState = BossState.move;
        isMoving = true;

        //while the player isn't
        //close enough to be attacked,
        //walk closer to them.
        while (!IsPlayerInAttackRange() || Vector3.Distance(playerObject.transform.position, transform.position) >= weapon.attackDistance)
        {
            rb.linearVelocity = rb.linearVelocity = (playerObject.transform.position - transform.position).normalized * speed;
            yield return new WaitForFixedUpdate();
        }

        //TODO:
        //Look at the player after moving
        //and then switch to attacking 
        //so we guaranteed hit them.

        //Stop moving.
        rb.linearVelocity = Vector3.zero;
        isMoving = false;

        //when they are in the attack range,
        //switch to the attack state.
        curState = BossState.attack;
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

            bossRenderer.enabled = !bossRenderer.enabled;
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

        bossRenderer.enabled = true;


        

        

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
}

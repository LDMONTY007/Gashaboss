using System.Collections;
using System.Linq;
using UnityEngine;

public class BossController : MonoBehaviour, IDamageable
{

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

    public BossState curState = BossState.idle;

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
                bossRenderer.material.color = Color.yellow;
                HandleAttack();
                break;
            case BossState.move:
                bossRenderer.material.color = Color.blue;
                HandleMove();
                break;
        }

        #endregion
    }

    //Here we decide if we want to attack,
    //or if we want to try and get closer to the player.
    public void HandleIdle()
    {
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
        meleeAttack.Execute(this, 1f);

        //at the end of an attack coroutine
        //always set the state back to idle
        //curState = BossState.idle;

    }

    public void HandleMove()
    {
        if (!isMoving)
        {
            Debug.Log("Boss is moving to player!".Color("Red"));
            //for now just move towards the player
            StartCoroutine(MoveToPosition(playerObject.transform.position));
        }
    }

    //LD Montello
    public void HandleRbRotation()
    {
        //rotate towards the velocity direction but don't rotate upwards.
        if (rb.linearVelocity != Vector3.zero)
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
    public IEnumerator MoveToPosition(Vector3 targetPos, float speed = 10f, float targetAccuracy = 5f)
    {
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
            moveTime += Time.deltaTime;
            yield return null;
        }

        //Loop till we reach our position
        //and only stop if we reach it or we hit an object.
        while ((targetPos - transform.position).magnitude > targetAccuracy && curState != BossState.stun)
        {
            moveTime += Time.deltaTime;
            rb.linearVelocity = (targetPos - transform.position).normalized * speed;
            yield return null;
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
        curState = BossState.idle;
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

        //Start the shaking animation
        ShakeModel();

        //Set to be low resolution for a small amount of time.
        StartLowResRoutine();
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

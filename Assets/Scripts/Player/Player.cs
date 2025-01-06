using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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
    public float moveSpeed = 5f;

    //Variables for movement
    Vector3 moveVector = Vector3.zero;

    //Input actions
    InputAction moveAction;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get cam
        cam = Camera.main;

        //get move action
        moveAction = playerInput.actions["Move"];


        //disable the ragdoll after
        //it is done initializing the joints.
        //if it is disabled before the start method is called
        //then the joints don't work.
        ragdollParentTransform.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //get the movement direction.
        moveVector = moveAction.ReadValue<Vector2>();


        //Testing dying.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Die();
        }
    }

    private void FixedUpdate()
    {
        Vector3 prevVel = rb.linearVelocity;

        

        //project controls to the camera's rotation so left and right are always the left and right sides of the camera.
        moveVector = cam.transform.right * moveVector.x + cam.transform.forward * moveVector.y;

       
        //set the speed using move speed and the normalized movement direction vector.
        rb.linearVelocity = moveVector.normalized * moveSpeed;

        //rotate towards the velocity direction but don't rotate upwards.
        if (rb.linearVelocity != Vector3.zero)
        rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity, transform.up));
        
    }

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
        iFramesRoutine = StartCoroutine(iFramesCoroutine()); 
     
    }

    public IEnumerator iFramesCoroutine()
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
}

using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //private references 
    Camera cam;

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

        Debug.Log(moveVector.y);

        //project controls to the camera's rotation so left and right are always the left and right sides of the camera.
        moveVector = cam.transform.right * moveVector.x + cam.transform.forward * moveVector.y;

       
        //set the speed using move speed and the normalized movement direction vector.
        rb.linearVelocity = moveVector.normalized * moveSpeed;

        //rotate towards the velocity direction but don't rotate upwards.
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
}

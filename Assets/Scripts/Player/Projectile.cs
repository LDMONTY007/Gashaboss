using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject hitParticles;

    public bool doesSplashDamage = false;

    public float splashDamageRadius = 15f;

    public bool destroyOnCollision = true;

    public int damage = 1;

    public bool launchOnStart = true;

    //set by the projectile weapon when we are spawned.
    //use this to launch when we start our code.
    public float launchSpeed = 5f;

    public float gravity = 9.81f;

    [HideInInspector]
    public Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        //we don't want to use gravity as we'll be applying our own depending on the situation 
        //and projectile type.
        rb.useGravity = false;

        if (launchOnStart)
        rb.AddForce(transform.forward.normalized * launchSpeed, ForceMode.Impulse);
    }

    private void LateUpdate()
    {
        //we apply our final movements in late update
        //to ensure it does not get overwritten.
        ApplyFinalMovements();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (doesSplashDamage)
        {
            DealSplashDamage();
        }
        else
        {
            //do damage to the object we collided with (or attempt to)
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DealDamage(damageable);
            }
        }

        

        if (destroyOnCollision)
        {
            Destroy(gameObject);
        }

    }

    private void OnDestroy()
    {
        //create the hit particles when we are destroyed.
        Instantiate(hitParticles, transform.position, transform.rotation);
    }

    public void DealSplashDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, splashDamageRadius);
        for (int i = 0; i < colliders.Length; i++)
        {
            //try to do damage to a damageable if it exists.
            IDamageable damageable = colliders[i].gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DealDamage(damageable);
            }
        }
    }

    private void OnDrawGizmos()
    {
        //draw the splash damage radius if we do splash damage.
        //this is so we can make sure the particles played are the right radius.
        if (doesSplashDamage)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, splashDamageRadius);
            Gizmos.color = prevColor;
        }
        
    }

    public virtual void DealDamage(IDamageable damageable)
    {
        if (damageable != null)
        {

            //make the damageable take damage.
            //and tell it we gave it damage.
            damageable.TakeDamage(damage, gameObject);
        }
    }

    public virtual void ApplyFinalMovements()
    {
        //Apply gravity, because gravity is not affected by mass and 
        //we can't use ForceMode.acceleration with 2D just multiply
        //by mass at the end. It's basically the same.
        //In unity it factors in mass for this calculation so 
        //multiplying by mass cancels out mass entirely.

        //we also use vector3.up because we're going to assume gravity
        //is currently going up or down relative to world space.
        rb.AddForce(-Vector3.up * gravity * rb.mass);
    }
}


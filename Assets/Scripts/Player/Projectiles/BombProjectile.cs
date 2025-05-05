using UnityEngine;

public class BombProjectile : Projectile
{

    public int bouncesBeforeExplosion = 2;

    private void Update()
    {
        Debug.DrawRay(transform.position, rb.linearVelocity.normalized * 10f, Color.blue);
    }

    private void ReflectBounce(Collision col)
    {
        //reflect the current velocity to create a perfect parabola bounce.
        //rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, col.contacts[0].normal);
        Debug.DrawRay(transform.position, col.GetContact(0).normal.normalized * 10f, Color.green);
        Debug.DrawRay(transform.position, col.relativeVelocity * 15f, Color.yellow);
        
        //Set the current velocity to be the negative relative velocity reflected over the normal of the surface
        //to create a perfect cartoony bounce.
        rb.linearVelocity = Vector3.Reflect(-col.relativeVelocity, col.GetContact(0).normal);
        Debug.DrawRay(transform.position, Vector3.Reflect(-col.relativeVelocity, col.GetContact(0).normal), Color.red);
        
    }

    //hide the inherited member so we can have an explosion
    //on the 3rd collision.
    public new void OnCollisionEnter(Collision collision)
    {

        

        //if the object we hit is damageable, immediately explode.
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        //Ignore the player so they don't get damaged by the bomb.
        if (Player.instance.gameObject != collision.gameObject && damageable != null)
        {
            DealDamage(damageable);
        }
        else
        {
            //if we haven't bounced enough we'll
            //continue allowing the bomb to bounce.
            if (bouncesBeforeExplosion > 0)
            {
                bouncesBeforeExplosion--;

                //bounce.
                ReflectBounce(collision);
                return;
            }
            //once we've bounced enough the bomb will explode.
            else if (doesSplashDamage)
            {
                DealSplashDamage();
            }
        }


        //destroy the gameobject if it should
        //be destroyed on collision.
        if (destroyOnCollision)
        {
            Destroy(gameObject);
        }
    }

    //this is used to forcefully detonate the bomb when we want through code.
    public void Detonate()
    {
        //Deal the splash damage
        DealSplashDamage();

        //Destroy the bomb.
        Destroy(gameObject);
    }
}

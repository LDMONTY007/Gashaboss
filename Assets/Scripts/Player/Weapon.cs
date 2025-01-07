using System.Collections;
using UnityEngine;

//LD Montello
//Note: Make sure this
//object's collider excludes
//the player layer.
public class Weapon : MonoBehaviour
{
    //the weapon should handle animations within itself
    //cus it'll make scaling easier. 

    public ParticleSystem hitParticles;

    //public Collider weaponCollider;

    public bool hasCooldown = false;

    public float cooldownTime = 1f;

    public bool canAttack = true;

    public float attackRadius = 10f;

    public float attackDistance = 1f;

    LayerMask playerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //weaponCollider.enabled = false;

        //get player mask
        playerMask = LayerMask.GetMask("Player", "Ignore Raycast"); //Assign our layer mask to player
        playerMask = ~playerMask; //Invert the layermask value so instead of being just the player it becomes every layer but the mask

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack()
    {
        //if we can't attack,
        //print a message in case something
        //breaks because of this.
        if (!canAttack)
        {
            Debug.LogWarning("Cannot attack during cooldown");
            return;
        }

        //Start AttackCoroutine
        StartCoroutine(AttackCoroutine());


    }

    public IEnumerator AttackCoroutine()
    {
        //don't allow other attacks during our current attack.
        canAttack = false;

        //TODO:
        //implement an animation for attacking,
        //but for now enable the collider for an attack.
        //weaponCollider.enabled = true;

        //RaycastHit hits = Physics.SphereCastAll(transform.position, attackRadius, transform.forward, attackDistance, playerMask);
        RaycastHit sphereHit;
        
        if (Physics.SphereCast(transform.position, attackRadius, transform.forward * attackDistance, out sphereHit, attackDistance, playerMask))
        {
            //if this hit is in front of us, check for damageables
            if (Mathf.Abs(Vector3.Dot(transform.position, sphereHit.point)) > 0)
            {
                IDamageable damageable = sphereHit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    //make the damageable take damage.
                    //and tell it we gave it damage.
                    damageable.TakeDamage(1, gameObject);

                    //TODO:
                    //Spawn a particle system burst that destroys itself
                    //when we hit a damageable so that the player can see where they hit.
                    //maybe give each weapon an individualized particle effect.
                    //spawn a particle effect facing outward from the normal
                    //at the position that was hit.
                    Debug.Log(sphereHit.point);
                    Instantiate(hitParticles, sphereHit.point, Quaternion.LookRotation(sphereHit.normal));
                }
            }


        }

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
        yield return new WaitForSeconds(cooldownTime);

        //allow us to attack again.
        canAttack = true;

        yield break;
    }

    //You didn't code this,
    //you got it from here:
    //https://discussions.unity.com/t/draw-spherecasts-code-review/872716/2
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, attackRadius, transform.forward * attackDistance, out hit, attackDistance, playerMask))
        {
            Gizmos.color = Color.green;
            Vector3 sphereCastMidpoint = transform.position + (transform.forward * hit.distance);
            Gizmos.DrawWireSphere(sphereCastMidpoint, attackRadius);
            Gizmos.DrawSphere(hit.point, 0.1f);
            Debug.DrawLine(transform.position, sphereCastMidpoint, Color.green);
        }
        else
        {
            Gizmos.color = Color.red;
            Vector3 sphereCastMidpoint = transform.position + (transform.forward * (attackDistance - attackRadius));
            Gizmos.DrawWireSphere(sphereCastMidpoint, attackRadius);
            Debug.DrawLine(transform.position, sphereCastMidpoint, Color.red);
        }
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            //make the damageable take damage.
            //and tell it we gave it damage.
            damageable.TakeDamage(1, gameObject);

            //TODO:
            //Spawn a particle system burst that destroys itself
            //when we hit a damageable so that the player can see where they hit.
            //maybe give each weapon an individualized particle effect.
            //spawn a particle effect facing outward from the normal
            //at the position that was hit.
            Instantiate(hitParticles, collision.GetContact(0).point, Quaternion.LookRotation(collision.GetContact(0).normal));
        }
    }*/

    //Cooldown coroutine.
    public IEnumerator CooldownCoroutine()
    {
        canAttack = false;

        yield return new WaitForSeconds(cooldownTime);

        canAttack = true;
    }
}

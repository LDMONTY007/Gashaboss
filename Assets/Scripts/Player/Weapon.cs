using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

//LD Montello
//Note: Make sure this
//object's collider excludes
//the player layer.
public class Weapon : Collectible
{
    //the weapon should handle animations within itself
    //cus it'll make scaling easier. 
    public ParticleSystem hitParticles;

    public GameObject slamParticles;

    public CollisionSensor collisionSensor;

    // Transform component that tells the player where to hold the weapon
    public Transform handleTransform;

    //public Collider weaponCollider;

    public bool hasCooldown = false;

    public float cooldownTime = 1f;

    public float altCooldownTime = 1f;

    public float specialCooldownTime = 1f;

    //used to know when the weapon is executing an attack.
    public bool isAttacking = false;

    public bool canAttack = true;

    public float attackDistance = 1f;

    public Player player;

    public Animator animator;

    //colors used when debugging the collision sensor mesh
    protected Color ogMeshColor;
    //blue with 100 alpha.
    protected Color cooldownMeshColor = new Color(0, 0, 1, 100f / 255f);

    //used to prevent destroying the weapon we pick up.
    //sometimes that would happen and this is a really messy solution for that.
    public bool isEquipped;

    bool swordOnLeft = false;

    //when unity engine
    //does the "Loading" prompt
    //this method gets called.
    //this also happens whenever
    //the attackDistance is modified
    //in the inspector
    //which means that in the editor
    //the radius of the trigger
    //will also be modified without
    //the game running. 
    //this is the ideal behavior.
    private void OnValidate()
    {
        //set the collision sensor 
        //collider radius to be
        //the same as our attack distance
        collisionSensor.triggerCollider.radius = attackDistance / 2;
    }

    private void OnDestroy()
    {
        //TODO:
        //Add the animation here where we destroy the 
        //weapon.
        Debug.Log(gameObject.name + " was destroyed!");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = Player.instance;

        //set the collision sensor 
        //collider radius to be
        //the same as our attack distance
        collisionSensor.triggerCollider.radius = attackDistance / 2;

        //get original mesh color
        ogMeshColor = collisionSensor.sensorColor;
        //weaponCollider.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //used for the special spin attack 
    //to check if there was a successful hit.
    bool specialDamageHitSuccess = false;

    public IEnumerator DealDamage(int damage)
    {

        List<GameObject> objs = collisionSensor.ScanForObjects();

        if (objs.Count > 0)
        {
            for (int i = 0; i < objs.Count; i++)
            {
                if (objs[i] != null)
                {
                    IDamageable damageable = objs[i].GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        //if the damageable is a boss, and is dead, skip this object in the loop.
                        BossController boss = objs[i].GetComponent<BossController>();
                        if (boss != null && boss.isDead)
                        {
                            continue;
                        }

                        //if the damageable isn't enabled then we skip it.
                        if ((damageable as MonoBehaviour).enabled == false)
                        {
                            continue;
                        }

                        //make the damageable take damage.
                        //and tell it we gave it damage.
                        damageable.TakeDamage(damage, gameObject);

                        //tell the special attack that the special damage hit was a success.
                        specialDamageHitSuccess = true;


                        //TODO:
                        //Spawn a particle system burst that destroys itself
                        //when we hit a damageable so that the player can see where they hit.
                        //maybe give each weapon an individualized particle effect.
                        //spawn a particle effect facing outward from the normal
                        //at the position that was hit.
                        //TODO:
                        //in the future replace this with
                        //some code that checks the direction
                        //the attack is coming from, does a box 
                        //cast and uses the normal and hit point from
                        //that box cast to calculate where the damage
                        //particle effect should spawn. 
                        Collider c = objs[i].GetComponent<Collider>();


                        Vector3 closestPoint = c.ClosestPoint(Camera.main.transform.position);


                        Instantiate(hitParticles, closestPoint + (-Camera.main.transform.forward.normalized * 0.25f), Quaternion.LookRotation(Camera.main.transform.position));

                        //Instantiate(hitParticles, c.transform.position, Quaternion.LookRotation(Camera.main.transform.position));

                        //wait for fixedupdate before launching player.
                        //if we didn't wait we'd have inconsistent physics.
                        yield return new WaitForFixedUpdate();

                        //player.LaunchPlayer(player.transform.up, 30f, 1f, 2f);
                    }
                }
                else
                {
                    Debug.LogWarning("Object was null while checking if it is damageable".Color("Orange"));
                }

            }
        }
    }

    public IEnumerator DealDamageAndLaunch(int damage, Vector3 direction, float height = 30f, float timeToApex = 1, float timeToFall = 2)
    {

        List<GameObject> objs = collisionSensor.ScanForObjects();

        if (objs.Count > 0)
        {
            for (int i = 0; i < objs.Count; i++)
            {
                if (objs[i] != null)
                {
                    IDamageable damageable = objs[i].GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        //if the damageable is a boss, and is dead, skip this object in the loop.
                        BossController boss = objs[i].GetComponent<BossController>();
                        if (boss != null && boss.isDead)
                        {
                            continue;
                        }


                        //if the damageable isn't enabled then we skip it.
                        if ((damageable as MonoBehaviour).enabled == false)
                        {
                            continue;
                        }

                        //make the damageable take damage.
                        //and tell it we gave it damage.
                        damageable.TakeDamage(damage, gameObject);

                        
                        //TODO:
                        //Spawn a particle system burst that destroys itself
                        //when we hit a damageable so that the player can see where they hit.
                        //maybe give each weapon an individualized particle effect.
                        //spawn a particle effect facing outward from the normal
                        //at the position that was hit.
                        //TODO:
                        //in the future replace this with
                        //some code that checks the direction
                        //the attack is coming from, does a box 
                        //cast and uses the normal and hit point from
                        //that box cast to calculate where the damage
                        //particle effect should spawn. 
                        Collider c = objs[i].GetComponent<Collider>();


                        Vector3 closestPoint = c.ClosestPoint(Camera.main.transform.position);


                        Instantiate(hitParticles, closestPoint + (-Camera.main.transform.forward.normalized * 0.25f), Quaternion.LookRotation(Camera.main.transform.position));

                        //wait for fixedupdate before launching player.
                        //if we didn't wait we'd have inconsistent physics.
                        yield return new WaitForFixedUpdate();

                        player.LaunchPlayer(direction, height, timeToApex, timeToFall);
                    }
                }
                else
                {
                    Debug.LogWarning("Object was null while checking if it is damageable".Color("Orange"));
                }

            }
        }
    }

    public virtual void Attack()
    {
        //if we can't attack,
        //print a message in case something
        //breaks because of this.
        if (!canAttack)
        {
            //Debug.LogWarning("Cannot attack during cooldown");
            return;
        }

        //Start AttackCoroutine
        StartCoroutine(AttackCoroutine());

        //flip if the sword is on the left or not so we know for the special attack.
        swordOnLeft = !swordOnLeft;
    }

    public virtual void AltAttack()
    {
        if (!canAttack)
        {
            //Debug.LogWarning("Cannot attack during cooldown");
            return;
        }


        //TODO: 
        //code a different attack that occurs when in the air. this way the player can have a quick downward smash attack that
        //works only if they are in the air and do their alt attack with the sword.
        if (player.inAir)
        {
            StartCoroutine(AirAltAttackCoroutine());
        }
        else
        {

            //flip if the sword is on the left or not so we know for the special attack.
            swordOnLeft = !swordOnLeft;

            //Start AltAttackCoroutine
            StartCoroutine(AltAttackCoroutine());
        }

        
    }

    public virtual void SpecialAttack()
    {
        //if we can't attack,
        //print a message in case something
        //breaks because of this.
        if (!canAttack)
        {
            //Debug.LogWarning("Cannot attack during cooldown");
            return;
        }

        //Start specialAttack
        StartCoroutine(SpecialAttackCoroutine());
    }

    public virtual IEnumerator AttackCoroutine()
    {
        //say we are currently attacking
        isAttacking = true;

        //start the attack animation
        if (animator != null)
        animator.SetTrigger("attack");

        //don't allow other attacks during our current attack.
        canAttack = false;

        //TODO:
        //implement an animation for attacking,
        //but for now enable the collider for an attack.
        //weaponCollider.enabled = true;

        //Execute the coroutine for dealing damage using our collision sensor.
        yield return DealDamage(1);

        //set color of debug mesh to show we are in cooldown
        collisionSensor.sensorColor = cooldownMeshColor;

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
        yield return new WaitForSeconds(cooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

        //wait for the animator to finish the attack animation before continuing.
        //yield return LDUtil.WaitForAnimationFinish(animator);

        //allow us to attack again.
        canAttack = true;

        //say we are no longer attacking
        isAttacking = false;

        yield break;
    }


    public virtual IEnumerator AltAttackCoroutine()
    {
        //say we are currently attacking
        isAttacking = true;

        //start the attack animation
        if (animator != null)
            animator.SetTrigger("altAttack");

        //don't allow other attacks during our current attack.
        canAttack = false;

        //Execute the coroutine for dealing damage using our collision sensor.
        //and launch the player upwards.
        yield return DealDamageAndLaunch(1, player.transform.up, 30f, 1f, 2f);

        //set color of debug mesh to show we are in cooldown
        collisionSensor.sensorColor = cooldownMeshColor;

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
            yield return new WaitForSeconds(altCooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        //say we are no longer attacking
        isAttacking = false;

        yield break;
    }

    //this does 2 damage.
    public virtual IEnumerator AirAltAttackCoroutine()
    {
        //say we are currently attacking
        isAttacking = true;

        //start the attack animation
        if (animator != null)
        {
            //tell the animator we are in the air.
            animator.SetBool("inAir", true);

            //do the alt attack animation.
            animator.SetTrigger("altAttack");
        }

        //don't allow other attacks during our current attack.
        canAttack = false;

        //wait for fixedupdate before launching player.
        //if we didn't wait we'd have inconsistent physics.
        yield return new WaitForFixedUpdate();

        //launch the player downwards.
        player.LaunchPlayer(-player.transform.up, 30f, 1f, 2f);


        //we need to wait for the player to land on the ground again
        //before continuing.
        while (!player.isGrounded)
        {
            yield return null;
        }

        //Say we are no longer in the air so that we do the ground landing
        //sword animation
        if (animator != null)
        {
            animator.SetBool("inAir", false);
        }

        //Spawn the slam attack landing ring
        //particle effect
        //at the player's feet
        if (slamParticles != null)
        Instantiate(slamParticles, player.GetFeetPosition(), slamParticles.transform.rotation);

        //Execute the coroutine for dealing damage using our collision sensor.
        //Deal 2 damage.
        yield return DealDamage(2);


        //set color of debug mesh to show we are in cooldown
        collisionSensor.sensorColor = cooldownMeshColor;

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
            yield return new WaitForSeconds(altCooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        //say we are no longer attacking
        isAttacking = false;

        yield break;
    }



    //TODO:
    //I want to make it so when you get an attack
    //to land, any attack, you don't start falling yet,
    //because then you can combo against the boss.
    public virtual IEnumerator SpecialAttackCoroutine()
    {
        //say we are currently attacking
        isAttacking = true;

        //start the attack animation
        if (animator != null)
            animator.SetTrigger("specialAttack");


        //don't allow other attacks during our current attack.
        canAttack = false;


        
        if (animator != null)
        {
            //Tell the player not to use gravity.
            player.useGravity = false;

            //don't let the player jump.
            player.canJump = false;

            //Tell the player to stop jumping.
            player.StopJumping();

            player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, 0.1f, player.rb.linearVelocity.z);

            //Wait until the animation is done
            //yield return LDUtil.WaitForAnimationFinishIgnoreTransition(animator);
            //I actually need to run the attack check code every so often so that 
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
            {
                yield return null;
            }

            float prevAngle = collisionSensor.angle;

            collisionSensor.angle = 45;

            //max hits for the spin attack is 3.
            int maxHits = 3;
            int hits = 0;

            //Wait until the animation is done
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            {
                //if the sword is on the left spin collider clockwise
                if (!swordOnLeft)
                {
                    //spin the collision sensor 360 degrees based on the normalized time of the spin animation.
                    collisionSensor.transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(360, 0, animator.GetCurrentAnimatorStateInfo(0).normalizedTime), 0f);
                }
                //else sword is on the right spin collider counter-clockwise 
                else
                {
                    //spin the collision sensor 360 degrees based on the normalized time of the spin animation.
                    collisionSensor.transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(0, 360, animator.GetCurrentAnimatorStateInfo(0).normalizedTime), 0f);
                }

                if (hits < maxHits)
                {
                    //Check and deal damage.
                    //Execute the coroutine for dealing damage using our collision sensor.
                    yield return DealDamage(1);
                }
                else
                {
                    Debug.Log("MAXED OUT HITS FOR SPIN ATTACK".Color("Orange"));
                }


                //if we did hit something, we need to wait 0.33 seconds
                //before registering another hit in this spin attack.
                if (specialDamageHitSuccess)
                {
                    //reset the flag.
                    specialDamageHitSuccess = false;
                    //increment hit count.
                    hits++;
                    yield return new WaitForSeconds(0.33f);
                }
                else
                    yield return null;
            }

            collisionSensor.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            collisionSensor.angle = prevAngle;

            //turn gravity back on and
            //let the player jump again.
            player.useGravity = true;
            player.canJump = true;

            //player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, player.rb., player.rb.linearVelocity.z);
        }


        //set color of debug mesh to show we are in cooldown
        collisionSensor.sensorColor = cooldownMeshColor;

        //this special attack has no cooldown other than it's animation time.
        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown)
            yield return new WaitForSeconds(specialCooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        //say we are no longer attacking
        isAttacking = false;

        yield break;
    }
    
    //Cooldown coroutine.
    public IEnumerator CooldownCoroutine()
    {
        canAttack = false;

        yield return new WaitForSeconds(cooldownTime);

        canAttack = true;
    }



    public override void OnCollect(){

        //we should not be equipped more than once.
        if (isEquipped)
        {
            return;
        }

        isEquipped = true;

        if (animator != null)
        {
            animator.SetBool("isDrop", false);
        }

        CollectionManager.instance.AddToCollection(this);
        //swap the weapon on the player for ourselves.
        Player.instance.SwapCurrentWeapon(this);
    }
}

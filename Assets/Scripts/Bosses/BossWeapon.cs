using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

public class BossWeapon: Weapon{
    [SerializeField] public BossActionMaterials materials; // Holds the reference to pass to boss actions
    private List<BossAction> bossActions = new List<BossAction>();// Holds actions for attacks

    // These vars are to modify the collision sensor for attack radius's before boss attacks
    #region Attack vars
    [SerializeField] private float atkAngle = 30;
    [SerializeField] private float atkHeight = 1.0f;
    public float atkCheckRadius = 15;
    #endregion
    #region AltAttack vars
    [SerializeField] public bool hasAlt = false;
    [SerializeField] private float altAtkRadius;
    [SerializeField] private float altAtkAngle;
    [SerializeField] private float altAtkHeight;
    public float altAtkCheckRadius = 15;
    [SerializeField] private int altAction = -1; // holds the action index to be called as part of the alt
    public bool animateAlt = false; // set this to true if animator controls alt action
    #endregion
    #region SpecialAttack vars
    [SerializeField] public bool hasSpecial = false;
    [SerializeField] private float specialAtkRadius;
    [SerializeField] private float specialAtkAngle;
    [SerializeField] private float specialAtkHeight;
    public float specialAtkCheckRadius = 15;
    [SerializeField] private int specialAction = -1; // holds the action index to be called as part of the special
    public bool animateSpecial = false; // set this to true if animator controls special action
    #endregion
    
    public void Awake(){
        // This list holds all the possible boss actions that can be used as part of an attack
        // Put the index of which action you want to use in for the fields related to the alt action and special action
        // PillowHop: 0
        // LaserAttack: 1
        // LaserSwarm: 2
        // TrexStomp: 3
        // MechaRexLaser: 4
        // DualCannonLaser: 5
        bossActions.Add(new PillowHop(materials));
        bossActions.Add(new LaserAttack(materials));
        bossActions.Add(new LaserSwarm(materials));
        bossActions.Add(new TrexStomp(materials));
        bossActions.Add(new MechasaurLaser(materials));
        bossActions.Add(new DualCannonLaserAttack(materials));
    }
    public override void AltAttack(){
        if (!canAttack){
            //Debug.LogWarning("Cannot attack during cooldown");
            return;
        }
        StartCoroutine(AltAttackCoroutine());
    }
    public override IEnumerator AttackCoroutine(){
        //don't allow other attacks during our current attack.
        canAttack = false;

        //say we are currently attacking
        isAttacking = true;

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

    public override IEnumerator AltAttackCoroutine(){
        //don't allow other attacks during our current attack.
        canAttack = false;

        //say we are currently attacking
        isAttacking = true;

        // Change Collision Parameters to Alt attack parameters
        collisionSensor.triggerCollider.radius = altAtkRadius;
        collisionSensor.angle = altAtkAngle;
        collisionSensor.height = altAtkHeight;


        
        
        // preform all the actions associated with this attack
        if (altAction != -1 && !animateAlt) {

            //if this alt attack does melee damage.
            if (bossActions[altAction].dealMeleeDamage)
            {
                yield return DealDamage(bossActions[altAction].meleeDamage);
            }
            yield return bossActions[altAction].ActionCoroutine(transform.GetComponentInParent<BossController>(), 1.0f);
        }

        // Change Collision Parameters back to reg attack parameters
        // Needed for attacking/player detection purposes
        collisionSensor.triggerCollider.radius = attackDistance;
        collisionSensor.angle = atkAngle;
        collisionSensor.height = atkHeight;

        //set color of debug mesh to show we are in cooldown
        collisionSensor.sensorColor = cooldownMeshColor;

        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown) yield return new WaitForSeconds(altCooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        //say we are no longer attacking
        isAttacking = false;

        yield break;
    }

    public IEnumerator DealDamage(float damage)
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
                        //make the damageable take damage.
                        //and tell it we gave it damage.
                        damageable.TakeDamage(1, gameObject);

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
                    }
                }
                else
                {
                    Debug.LogWarning("Object was null while checking if it is damageable".Color("Orange"));
                }
            }
        }

        yield break;
    }

    public override IEnumerator SpecialAttackCoroutine(){
        //don't allow other attacks during our current attack.
        canAttack = false;

        //say we are currently attacking
        isAttacking = true;

        // Change Collision Parameters to special attack parameters
        collisionSensor.triggerCollider.radius = specialAtkRadius;
        collisionSensor.angle = specialAtkAngle;
        collisionSensor.height = specialAtkHeight;

        // preform all actions associated with the special
        if (specialAction != -1 && !animateSpecial) {

            //if this special attack does melee damage.
            if (bossActions[specialAction].dealMeleeDamage)
            {
                yield return DealDamage(bossActions[specialAction].meleeDamage);
            }

            yield return bossActions[specialAction].ActionCoroutine(transform.GetComponentInParent<BossController>(), 1.0f);
        }
        // Change Collision Parameters back to reg attack parameters
        // Needed for attacking/player detection purposes
        collisionSensor.triggerCollider.radius = attackDistance;
        collisionSensor.angle = atkAngle;
        collisionSensor.height = atkHeight;

        //set color of debug mesh to show we are in cooldown
        collisionSensor.sensorColor = cooldownMeshColor;

        //this special attack has no cooldown other than it's animation time.
        //if we have a cooldown, wait the cooldown time before attacking.
        if (hasCooldown) yield return new WaitForSeconds(specialCooldownTime);

        //restore default color
        collisionSensor.sensorColor = ogMeshColor;

        //allow us to attack again.
        canAttack = true;

        //say we are no longer attacking
        isAttacking = false;

        yield break;
    }

    public IEnumerator AnimatorAltAction(){
        if (altAction != -1) {
            yield return bossActions[altAction].ActionCoroutine(transform.GetComponentInParent<BossController>(), 1.0f);
        }
    }

    public IEnumerator AnimatorSpecialAction(){
        if (specialAction != -1) {
            yield return bossActions[specialAction].ActionCoroutine(transform.GetComponentInParent<BossController>(), 1.0f);
        }
    }
}

using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "CoordinatedAttackItem", menuName = "Items/Coordinated Attack")]
public class CoordinatedAttackItem : ItemData
{
    [Header("Attack Settings")]
    [Tooltip("Prefab for the companion entity that will attack with player")]
    public GameObject companionPrefab;

    [Tooltip("Delay between player attack and companion attack")]
    public float attackDelay = 0.2f;

    [Tooltip("Damage done by the companion attack")]
    public int attackDamage = 30;

    private static bool isApplied = false;

    public override void OnPickup()
    {
        Debug.Log("Coordinated Attack item picked up! A companion will now attack with you.");

        if (!isApplied)
        {
            CoordinatedAttackEffect effect = Player.instance.gameObject.AddComponent<CoordinatedAttackEffect>();
            effect.Initialize(this);
            isApplied = true;
        }
    }

    public override void RemoveItem()
    {
        CoordinatedAttackEffect effect = Player.instance.GetComponent<CoordinatedAttackEffect>();
        if (effect != null)
        {
            Destroy(effect);
        }

        isApplied = false;
        Player.instance.inventory.Remove(this);

        Debug.Log("Coordinated Attack item removed from inventory.");
    }

    public override void ApplyEffect()
    {
        // Passive effect, handled by the effect component
    }
}

// Effect component combined with CoordinatedAttackEffect
public class CoordinatedAttackEffect : MonoBehaviour
{
    private CoordinatedAttackItem itemData;
    private GameObject companionInstance;
    private Weapon playerWeapon;

    public void Initialize(CoordinatedAttackItem data)
    {
        itemData = data;
        SpawnCompanion();

        // Subscribe to weapon attacks
/*        playerWeapon = Player.instance.curWeapon;
        if (playerWeapon != null)
        {
            playerWeapon.onAttack += OnPlayerAttack;
        }*/

        //for any of the player's attacks call OnPlayerAttack.
        Player.instance.OnAttack += OnPlayerAttack;
        Player.instance.OnAltAttack += OnPlayerAttack;
        Player.instance.OnSpecialAttack += OnPlayerAttack;

        Debug.Log("Coordinated Attack effect initialized!");
    }

    private void SpawnCompanion()
    {
        Vector3 spawnPos = Player.instance.transform.position + new Vector3(0, 1.5f, 0);
        companionInstance = Instantiate(itemData.companionPrefab ?? CreateDefaultCompanionPrefab(), spawnPos, Quaternion.identity);

        // Set up companion controller
        CompanionController companion = companionInstance.GetComponent<CompanionController>() ??
                                      companionInstance.AddComponent<CompanionController>();
        companion.followTarget = Player.instance.transform;
    }

    private GameObject CreateDefaultCompanionPrefab()
    {
        // Create a basic companion prefab with a default visual
        GameObject companionObj = new GameObject("DefaultCompanion");

        // Add a simple visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.transform.SetParent(companionObj.transform);
        visual.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Add renderer with material
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = new Color(0.3f, 0.3f, 1f);

        return companionObj;
    }

    private void OnPlayerAttack()
    {
        StartCoroutine(PerformCompanionAttack());
    }

    private IEnumerator PerformCompanionAttack()
    {
        yield return new WaitForSeconds(itemData.attackDelay);

        if (companionInstance != null)
        {
            BossController target = FindNearestBoss();

            if (target != null)
            {
                CompanionController companion = companionInstance.GetComponent<CompanionController>();
                companion.AttackTarget(target.transform, itemData.attackDamage);

                Debug.Log("Companion performed coordinated attack!");
            }
        }
    }

    private BossController FindNearestBoss()
    {
        BossController[] bosses = FindObjectsByType<BossController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        BossController nearest = null;
        float nearestDist = float.MaxValue;

        foreach (BossController boss in bosses)
        {
            if (boss.isDead)
                continue;

            float dist = Vector3.Distance(companionInstance.transform.position, boss.transform.position);
            if (dist < nearestDist)
            {
                nearest = boss;
                nearestDist = dist;
            }
        }

        return nearest;
    }

    private void Update()
    {
        // Check if weapon has changed
        if (Player.instance.curWeapon != playerWeapon)
        {
            if (playerWeapon != null)
            {
                playerWeapon.onAttack -= OnPlayerAttack;
            }

            playerWeapon = Player.instance.curWeapon;
            if (playerWeapon != null)
            {
                playerWeapon.onAttack += OnPlayerAttack;
            }
        }

        // Respawn companion if needed
        if (companionInstance == null)
        {
            SpawnCompanion();
        }
    }

    private void OnDisable()
    {
        if (playerWeapon != null)
        {
            playerWeapon.onAttack -= OnPlayerAttack;
        }

        if (companionInstance != null)
        {
            Destroy(companionInstance);
            companionInstance = null;
        }

        Debug.Log("Coordinated Attack effect removed.");
    }
}

// CompanionController combined for simplicity
public class CompanionController : MonoBehaviour
{
    [HideInInspector]
    public Transform followTarget;

    [SerializeField]
    private float followSpeed = 5f;

    [SerializeField]
    private ParticleSystem attackParticles;

    private bool isAttacking = false;

    private void Start()
    {
        //LD get the premade particle system instead.
        attackParticles = GetComponentInChildren<ParticleSystem>();

        // Create attack particles if not assigned
        /* if (attackParticles == null)
         {
             GameObject particlesObj = new GameObject("AttackParticles");
             particlesObj.transform.parent = transform;
             particlesObj.transform.localPosition = Vector3.zero;

             attackParticles = particlesObj.AddComponent<ParticleSystem>();

             // Yellow particle burst
             var main = attackParticles.main;
             main.startColor = Color.yellow;
             main.startSize = 0.2f;
             main.startSpeed = 5f;
             main.startLifetime = 0.5f;

             var emission = attackParticles.emission;
             emission.rateOverTime = 0;
             emission.SetBursts(new ParticleSystem.Burst[] {
                 new ParticleSystem.Burst(0f, 20)
             });
         }*/
    }

    private void Update()
    {
        if (followTarget != null && !isAttacking)
        {
            // Calculate desired position (offset from player)
            Vector3 desiredPos = followTarget.position + new Vector3(0, 1.5f, 0);

            // Move toward that position
            transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

            // Look in same direction as player
            transform.rotation = Quaternion.Lerp(transform.rotation, followTarget.rotation, followSpeed * Time.deltaTime);
        }
    }

    public void AttackTarget(Transform target, int damage)
    {
        Debug.Log("Shikigami Attack!".Color("Cyan"));
        StartCoroutine(AttackRoutine(target, damage));
    }

    private IEnumerator AttackRoutine(Transform target, int damage)
    {
        if (target == null)
            yield break;

        isAttacking = true;

        // Store original position
        Vector3 startPos = transform.position;

        // Move toward target quickly
        float attackTime = 0.3f;
        float elapsed = 0;

        while (elapsed < attackTime && target != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / attackTime;

            // Move toward target
            transform.position = Vector3.Lerp(startPos, target.position + Vector3.up, t);

            // Look at target
            if (target != null)
            {
                transform.LookAt(target);
            }

            yield return null;
        }

        // Deal damage if target still exists
        if (target != null)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, gameObject);

                // Play attack particles
                if (attackParticles != null)
                {
                    attackParticles.Play();
                }
            }
        }

        // Wait a bit after attack
        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
    }
}
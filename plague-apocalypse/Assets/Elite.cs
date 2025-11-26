using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EliteToilet : MonoBehaviour, IElite
{
    [Header("Stats")]
    public int health = 100;
    private int maxHealth;
    public float moveSpeed = 2f;
    public int damage = 10;
    public float attackInterval = 5f;

    [Header("Combat")]
    public float shootRange = 10f;
    public float bulletSpeed = 20f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Ranges")]
    public float chaseRange = 6f;
    public float attackRange = 2f;
    public float shootStartDistance = 10f;
    public float shootDelay = 2f;

    private float shootDelayTimer = 0f;
    public string[] meleeAttackAnimNames;

    [Header("Roar")]
    public string roarAnimName;
    public float roarDuration = 1f;

    [Header("Effects")]
    public ParticleSystem shootEffect;

    [Header("Phase 2 Settings")]
    public float phase2SpeedMultiplier = 1.5f;
    private bool phase2Active = false;

    [HideInInspector] public RoundManager roundManager;

    private Coroutine meleeCoroutine;
    private Coroutine shootCoroutine;

    private NavMeshAgent agent;
    private Transform player;
    private PlayerHealth playerHealth;
    private Animator anim;
    private bool isInvulnerable = false;

    private bool phase2DisableShooting = false;
    private bool isAttacking = false;
    private bool isDead = false;

    // ===== IElite implementation =====
    public void ApplyStats(int newHealth, float newSpeed, RoundManager roundManager)
    {
        this.health = newHealth;
        this.maxHealth = newHealth;
        this.moveSpeed = newSpeed;
        this.roundManager = roundManager;

        if (agent != null)
            agent.speed = moveSpeed;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;
        }

        if (!string.IsNullOrEmpty(roarAnimName))
            StartCoroutine(PlayRoar());
    }

    void Update()
    {
        if (isDead || player == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Pause AI during Phase 2
        if (phase2Active)
        {
            agent.isStopped = true;
            return; // don’t change Speed or IsShooting in Update
        }

        if (distance <= attackRange)
        {
            // MELEE
            shootDelayTimer = 0f;

            if (!isAttacking && meleeCoroutine == null)
                meleeCoroutine = StartCoroutine(MeleeAttackPlayer());

            agent.isStopped = true;
        }
        else if (distance <= chaseRange)
        {
            // CHASE
            shootDelayTimer = 0f;

            StopAllAttacks();

            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else if (distance <= shootRange && !phase2DisableShooting)
        {
            // SHOOT
            shootDelayTimer += Time.deltaTime;

            agent.isStopped = false;
            agent.SetDestination(player.position);
            anim?.SetBool("IsShooting", true);
            anim?.SetFloat("Speed", 0f);

            if (shootDelayTimer >= shootDelay && !isAttacking && shootCoroutine == null)
                shootCoroutine = StartCoroutine(ShootPlayer());
        }
        else
        {
            // Player far away → approach
            shootDelayTimer = 0f;

            StopAllAttacks();

            agent.isStopped = false;
            agent.SetDestination(player.position);
            anim?.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    private IEnumerator PlayRoar()
    {

        agent.isStopped = true;
        anim?.SetTrigger(roarAnimName);
        yield return new WaitForSeconds(roarDuration);
        agent.isStopped = false;
    }

    private IEnumerator MeleeAttackPlayer()
    {
        if (playerHealth == null) yield break;

        isAttacking = true;
        agent.isStopped = true; // stop moving

        while (!isDead)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > attackRange) break;

            // Pick a random punch animation
            string animName = meleeAttackAnimNames.Length > 0 ?
                meleeAttackAnimNames[Random.Range(0, meleeAttackAnimNames.Length)] : null;

            if (!string.IsNullOrEmpty(animName))
                anim?.SetTrigger(animName);

            // Wait for the punch animation to reach the damage frame
            float punchDelay = 1.5f; // adjust this to match the animation timing
            yield return new WaitForSeconds(punchDelay);

            // Deal  damage
            int punchDamage = damage;
            playerHealth.TakeDamage(punchDamage);

            // Wait for the rest of the animation
            float remainingAnimTime = attackInterval - punchDelay;
            if (remainingAnimTime > 0f)
                yield return new WaitForSeconds(remainingAnimTime);
        }

        StopAllAttacks();
    }


    private IEnumerator ShootPlayer()
    {
        if (player == null || projectilePrefab == null) yield break;

        isAttacking = true;

        while (!isDead)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackRange || distance > shootRange)
            {
                StopAllAttacks();
                yield break;
            }

            if (agent != null)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }

            Vector3 aimDir = (player.position - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * 5f);

            if (anim != null)
                anim.SetBool("IsShooting", true);

            ShootProjectile();

            yield return new WaitForSeconds(attackInterval);
        }

        StopAllAttacks();
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null) return;

        if (shootEffect != null)
            shootEffect.Play();

        Vector3 targetPoint = player.position + Vector3.up;
        Vector3 aimDirection = (targetPoint - firePoint.position).normalized;
        firePoint.rotation = Quaternion.LookRotation(aimDirection);

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = firePoint.forward * bulletSpeed;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead || isInvulnerable) return;

        health -= damageAmount;

        if (!phase2Active && health <= maxHealth / 3)
        {
            Debug.Log("Phase 2 triggered! Health: " + health);
            StartCoroutine(EnterPhase2());
        }

        if (health <= 0) Die();
    }

    [Header("Phase 2 Animations")]
    public string fallTrigger = "Phase2_Fall";
    public string getUpTrigger = "Phase2_Up";
    public float fallDuration = 1.5f;
    public float getUpDuration = 1.5f;

    private IEnumerator EnterPhase2()
    {
        phase2Active = true;
        Debug.Log("Entered Phase 2");
        isInvulnerable = true;
        // Stop all attacks
        StopAllAttacks();

        // Stop movement
        agent.isStopped = true;

        if (anim != null)
        {
            anim.SetTrigger(fallTrigger);
            yield return new WaitForSeconds(fallDuration);

            // Start fall loop
            anim.SetBool("IsFallen", true);
            yield return new WaitForSeconds(fallDuration + 5f); // fall + downtime

            // Exit fall loop and get up
            anim.SetBool("IsFallen", false);
            anim.SetTrigger(getUpTrigger);
            yield return new WaitForSeconds(getUpDuration);


            anim.SetTrigger(getUpTrigger);
            yield return new WaitForSeconds(getUpDuration);
        }

        moveSpeed *= phase2SpeedMultiplier;
        if (agent != null)
            agent.speed = moveSpeed;

        phase2DisableShooting = true;
        agent.isStopped = false;
        phase2Active = false;
        isInvulnerable = false;
        Debug.Log("Phase 2 complete, resuming AI");
    }

    private void Die()
    {
        isDead = true;
        anim?.SetBool("IsDead", true);
        agent.isStopped = true;
        roundManager?.EnemyKilled();
        Destroy(gameObject, 3f);
    }

    private void StopAllAttacks()
    {
        isAttacking = false;

        if (meleeCoroutine != null)
        {
            StopCoroutine(meleeCoroutine);
            meleeCoroutine = null;
        }

        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            shootCoroutine = null;
        }

        if (anim != null)
            anim.SetBool("IsShooting", false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shootStartDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}

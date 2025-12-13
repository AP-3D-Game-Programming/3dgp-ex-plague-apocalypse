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
    private float globalFireRateMult = 1f;
    private float globalDamageMult = 1f;
    private float globalPhase2HealthMult = 1f;
    private float globalPhase2SpeedMult = 1f;
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
    public ParticleSystem phase2TransitionEffect;
    public ParticleSystem phase2ConstantEffect;
    private bool phase2Active = false;
    [Header("Points")]
    public int pointsPerShot = 10;
    public int pointsOnDeath = 100;
    private int accumulatedPoints = 0;

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
    public AudioSource audioSource;

    public AudioClip[] ambientSounds;
    public AudioClip[] roarSounds;
    public AudioClip[] meleeSounds;
    public AudioClip[] shootSounds;
    public AudioClip[] hurtSounds;
    public AudioClip[] deathSounds;
    public AudioClip[] phase2TransitionSounds;
    public AudioClip phase1Music;
    public AudioClip phase2Music;
    public float minAmbientInterval = 3f;
    public float maxAmbientInterval = 8f;
    // IElite implementation 
    public void ApplyStats(
            int newHealth,
            float newSpeed,
            RoundManager roundManager,
            float fireRateMult,
            float damageMult,
            float phase2HealthMult,
            float phase2SpeedMult
        )
    {
        this.health = newHealth;
        this.maxHealth = newHealth;
        this.moveSpeed = newSpeed;
        this.roundManager = roundManager;
        this.globalFireRateMult = fireRateMult;
        this.globalDamageMult = damageMult;
        this.globalPhase2HealthMult = phase2HealthMult;
        this.globalPhase2SpeedMult = phase2SpeedMult;

        if (agent != null)
            agent.speed = moveSpeed;
    }

    void Awake()
    {
        maxHealth = health;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
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
    void Start()
    {
        if (MusicManager.Instance != null && phase1Music != null)
        {
            MusicManager.Instance.RequestMusic(phase1Music, 1);
        }
        StartCoroutine(AmbientSoundRoutine());
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
            PlayRandomSound(meleeSounds);
            // Wait for the punch animation to reach the damage frame
            float punchDelay = 1.5f; // adjust this to match the animation timing
            yield return new WaitForSeconds(punchDelay);

            // Deal  damage
            int punchDamage = Mathf.RoundToInt(damage * globalDamageMult);
            playerHealth.TakeDamage(punchDamage);


            // Wait for the rest of the animation
            float actualInterval = attackInterval / globalFireRateMult;
            float remainingAnimTime = actualInterval - punchDelay;
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

            float actualInterval = attackInterval / globalFireRateMult;
            yield return new WaitForSeconds(actualInterval);
        }

        StopAllAttacks();
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null) return;

        if (shootEffect != null)
            shootEffect.Play();
        PlayRandomSound(shootSounds);
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
        PlayRandomSound(hurtSounds);
        health -= damageAmount;
        if (accumulatedPoints < PlayerStats.Instance.maxShootPointsPerEnemy)
        {
            int pointsToGive = Mathf.Min(
                Mathf.RoundToInt(pointsPerShot * PlayerStats.Instance.shotPointsMultiplier),
                PlayerStats.Instance.maxShootPointsPerEnemy - accumulatedPoints
            );

            accumulatedPoints += pointsToGive;
            PlayerStats.Instance.AddPoints(pointsToGive);
        }

        //40 %
        float phase2Threshold = (maxHealth * 0.4f) * globalPhase2HealthMult;
        if (!phase2DisableShooting && !phase2Active && health <= phase2Threshold)
        {
            Debug.Log($"Phase 2 triggered! Health: {health} / Threshold: {phase2Threshold}");
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
        phase2Active = true; // This now blocks TakeDamage from triggering this again
        isInvulnerable = true;
        if (MusicManager.Instance != null && phase2Music != null)
        {
            // 1. Remove the "Phase 1" request so the counter drops to 0 (temporarily)
            MusicManager.Instance.StopRequest(1);
        }
        Debug.Log("Entered Phase 2");

        if (phase2TransitionSounds.Length > 0)
        {
            audioSource.Stop();
            AudioClip clip = phase2TransitionSounds[Random.Range(0, phase2TransitionSounds.Length)];

            audioSource.PlayOneShot(clip, 5.0f);
        }
        if (phase2TransitionEffect != null)
        {
            phase2TransitionEffect.Play();
        }
        StopAllAttacks();
        agent.isStopped = true;

        if (anim != null)
        {
            anim.SetTrigger(fallTrigger);
            yield return new WaitForSeconds(fallDuration);

            // Start fall loop
            anim.SetBool("IsFallen", true);
            yield return new WaitForSeconds(fallDuration + 6.1f); // downtime

            // Exit fall loop and get up
            anim.SetBool("IsFallen", false);
            anim.SetTrigger(getUpTrigger);
            yield return new WaitForSeconds(getUpDuration);


        }
        if (phase2TransitionEffect != null)
            phase2TransitionEffect.Stop();

        if (phase2ConstantEffect != null)
            phase2ConstantEffect.Play();
        moveSpeed *= phase2SpeedMultiplier * globalPhase2SpeedMult;
        if (agent != null)
            agent.speed = moveSpeed;

        phase2DisableShooting = true;
        agent.isStopped = false;
        phase2Active = false;
        isInvulnerable = false;
        // 2. Add the "Phase 2" request so the counter goes back to 1
        MusicManager.Instance.RequestMusic(phase2Music, 1);
        Debug.Log("Phase 2 complete, resuming AI");
    }
    private void Die()
    {
        isDead = true;
        anim?.SetBool("IsDead", true);
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopRequest(1);
        }
        if (phase2ConstantEffect != null)
            phase2ConstantEffect.Stop();
        PlayRandomSound(deathSounds);
        int pointsAwarded = Mathf.RoundToInt(pointsOnDeath * PlayerStats.Instance.deathPointsMultiplier);
        PlayerStats.Instance.AddPoints(pointsAwarded);
        agent.isStopped = true;
        roundManager?.EnemyKilled();
        Destroy(gameObject, 1f);
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
    IEnumerator AmbientSoundRoutine()
    {
        while (!isDead)
        {
            float waitTime = Random.Range(minAmbientInterval, maxAmbientInterval);
            yield return new WaitForSeconds(waitTime);

            if (!isDead && !isAttacking && !phase2Active)
            {
                PlayRandomSound(ambientSounds);
            }
        }
    }

    void PlayRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }
}

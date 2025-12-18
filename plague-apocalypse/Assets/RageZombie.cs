using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))] // Ensures an AudioSource is attached
public class RageZombie : MonoBehaviour, IElite
{
    [Header("Stats")]
    public int health = 100;
    private int maxHealth;
    public float moveSpeed = 2f;
    public int damage = 10;
    public float attackInterval = 1f;
    public float attackRange = 2f;
    public string isMovingParam = "IsMoving";

    // ==========================================
    // 0. AUDIO SETTINGS (NEW)
    // ==========================================
    [Header("Audio Settings")]
    public AudioSource audioSource;

    [Tooltip("Sound played when the zombie takes damage")]
    public AudioClip hitSound;

    [Tooltip("Sound played when the zombie attacks")]
    public AudioClip attackSound;

    [Tooltip("Sound played when the zombie dies")]
    public AudioClip deathSound;

    [Tooltip("Sound played when entering Rage Mode")]
    public AudioClip rageScreamSound;

    [Header("Footsteps")]
    public AudioClip[] footstepSounds; // Array for variety
    public float footstepInterval = 0.5f; // Time between steps
    private float footstepTimer;

    // ==========================================
    // 1. RAGE MODE SETTINGS
    // ==========================================
    [Header("Rage Mode")]
    public float rageHealthThreshold = 0.3f;
    public float rageSpeedMultiplier = 1.5f;
    public float rageDamageMultiplier = 2f;

    public float rageDamageReductionPercent = 0.55f;
    public string screamAnimTrigger = "ScreamRage";
    public float screamDuration = 3f;
    public GameObject rageEffectPrefab;
    public float rageEffectVerticalOffset = 1f;
    private bool isInRageMode = false;
    private bool isScreaming = false;

    // ==========================================
    // 2. INTERNAL REFERENCES
    // ==========================================
    [HideInInspector] public RoundManager roundManager;

    private NavMeshAgent agent;
    private Transform player;
    private PlayerHealth playerHealth;
    private Animator anim;

    private bool isAttacking = false;
    private bool isDead = false;
    public string runVariationParam = "RunVariation";
    [Header("Points")]
    public int pointsPerShot = 10;
    public int pointsOnDeath = 100;
    private int accumulatedPoints = 0;

    // ==========================================
    // 3. AWAKE & UPDATE
    // ==========================================
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        // Setup AudioSource
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        player = GameObject.FindWithTag("Player")?.transform;

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        maxHealth = health;

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;
        }
        if (anim != null)
        {
            anim.SetBool(isMovingParam, true);
        }
    }

    public void ApplyStats(int hp, float speed, RoundManager rm, float fireRateMult, float damageMult, float phase2HealthMult, float phase2SpeedMult)
    {
        this.health = hp;
        this.maxHealth = hp;
        this.moveSpeed = speed;
        this.roundManager = rm;

        // Apply elite multipliers
        this.damage = Mathf.RoundToInt(this.damage * damageMult);
        this.rageSpeedMultiplier *= phase2SpeedMult;

        // Update Agent Speed immediately
        if (GetComponent<UnityEngine.AI.NavMeshAgent>() != null)
            GetComponent<UnityEngine.AI.NavMeshAgent>().speed = speed;
    }

    void Update()
    {
        if (isDead || player == null || agent == null || isScreaming) return;

        HandleFootsteps(); // Check for footstep sounds

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            if (isAttacking)
                StopAttack();

            agent.isStopped = false;
            agent.SetDestination(player.position);
            anim.SetBool(isMovingParam, true);
        }
        else
        {
            if (!isAttacking)
            {
                anim.SetBool(isMovingParam, false);
                StartCoroutine(AttackPlayer());
            }
        }
    }

    // New helper method to handle footstep logic
    private void HandleFootsteps()
    {
        // If we are moving and not stopped
        if (agent.velocity.sqrMagnitude > 0.1f && !agent.isStopped)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                PlayRandomFootstep();
                // Reset timer based on speed (faster speed = faster steps)
                footstepTimer = footstepInterval / (isInRageMode ? rageSpeedMultiplier : 1f);
            }
        }
    }

    // ==========================================
    // 4. DAMAGE LOGIC
    // ==========================================
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // Play Hit Sound
        PlaySoundEffect(hitSound);

        int finalDamage = damage;

        if (isInRageMode && rageDamageReductionPercent > 0)
        {
            float reductionMultiplier = 1f - rageDamageReductionPercent;
            finalDamage = Mathf.RoundToInt(damage * reductionMultiplier);
            finalDamage = Mathf.Max(finalDamage, 1);
        }

        int healthBeforeDamage = health;
        health -= finalDamage;

        bool hitRageThreshold = !isInRageMode && (float)healthBeforeDamage / maxHealth > rageHealthThreshold && (float)health / maxHealth <= rageHealthThreshold;

        if (hitRageThreshold)
        {
            if (health <= 0)
            {
                health = 1;
            }
            StartCoroutine(ActivateRageModeRoutine());
        }
        if (health <= 0)
        {
            Die();
            return;
        }
    }

    private IEnumerator ActivateRageModeRoutine()
    {
        isInRageMode = true;
        isScreaming = true;

        // Play Rage Scream
        PlaySoundEffect(rageScreamSound);

        Debug.Log($"{gameObject.name} entered RAGE MODE!");
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        anim.SetBool(isMovingParam, false);
        anim?.SetTrigger(screamAnimTrigger);

        yield return new WaitForSeconds(screamDuration);

        moveSpeed *= rageSpeedMultiplier;
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }

        damage = Mathf.RoundToInt(damage * rageDamageMultiplier);

        if (rageEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * rageEffectVerticalOffset;
            Instantiate(rageEffectPrefab, spawnPosition, Quaternion.identity, transform);
        }

        agent.isStopped = false;
        isScreaming = false;
    }

    // ==========================================
    // 5. ATTACK & DEATH LOGIC
    // ==========================================
    private void Die()
    {
        // Play Death Sound
        PlaySoundEffect(deathSound);

        anim.SetBool(isMovingParam, false);
        isDead = true;
        anim.SetBool("IsDead", true);
        agent.isStopped = true;

        int pointsAwarded = Mathf.RoundToInt(pointsOnDeath * PlayerStats.Instance.deathPointsMultiplier);
        PlayerStats.Instance.AddPoints(pointsAwarded);

        roundManager?.EnemyKilled();
        Destroy(gameObject, 2f);
    }

    private IEnumerator AttackPlayer()
    {
        if (playerHealth == null) yield break;

        isAttacking = true;
        anim.SetBool("IsAttacking", true);
        agent.isStopped = true;

        while (!isDead)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > attackRange) break;

            // Play Attack Sound
            PlaySoundEffect(attackSound);

            playerHealth.TakeDamage(damage);

            yield return new WaitForSeconds(attackInterval);
        }

        StopAttack();
    }

    private void StopAttack()
    {
        isAttacking = false;
        anim.SetBool("IsAttacking", false);
        agent.isStopped = false;
    }

    // ==========================================
    // 6. AUDIO HELPERS
    // ==========================================
    private void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepSounds != null && footstepSounds.Length > 0 && audioSource != null)
        {
            int index = Random.Range(0, footstepSounds.Length);
            // Lower volume slightly for footsteps so they don't overpower screams
            audioSource.PlayOneShot(footstepSounds[index], 0.6f);
        }
    }
}
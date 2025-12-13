using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Zombie : MonoBehaviour
{
    [Header("Stats")]
    public int health = 100;
    public float moveSpeed = 2f;
    public int damage = 10;
    public float attackInterval = 1f;
    public float attackRange = 2f; // How close zombie must be to attack
    public float attackDamageDelay = 0.5f;
    [HideInInspector] public RoundManager roundManager;
    public float deathAnimationDuration = 1.04f;
    private NavMeshAgent agent;
    private Transform player;
    private PlayerHealth playerHealth;
    private Animator anim;

    private bool isAttacking = false;
    private bool isDead = false;
    [Header("Points")]
    public int pointsPerShot = 10;
    public int pointsOnDeath = 100;
    private int accumulatedPoints = 0;
    [Header("Audio Settings")]
    public AudioSource audioSource;

    public AudioClip[] ambientSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] hurtSounds;
    public AudioClip[] deathSounds;

    public float minAmbientInterval = 3f;
    public float maxAmbientInterval = 10f;
    private float basePitch = 1f;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        player = GameObject.FindWithTag("Player")?.transform;

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
        if (audioSource != null)
        {
            basePitch = audioSource.pitch;
        }
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange; // Stop just at attack range
        }
    }
    void Start()
    {
        // Start making random zombie noises
        StartCoroutine(AmbientSoundRoutine());
    }
    void Update()
    {
        if (isDead || player == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            // Move towards player
            if (isAttacking)
                StopAttack();

            agent.isStopped = false;
            agent.SetDestination(player.position);
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
        else
        {
            // Attack
            if (!isAttacking)
                StartCoroutine(AttackPlayer());
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        if (accumulatedPoints < PlayerStats.Instance.maxShootPointsPerEnemy)
        {
            int pointsToGive = Mathf.Min(
                Mathf.RoundToInt(pointsPerShot * PlayerStats.Instance.shotPointsMultiplier),
                PlayerStats.Instance.maxShootPointsPerEnemy - accumulatedPoints
            );

            accumulatedPoints += pointsToGive;
            PlayerStats.Instance.AddPoints(pointsToGive);
        }
        if (health <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        PlayRandomSound(deathSounds);
        anim.SetBool("IsDead", true);
        agent.isStopped = true;
        StopAllCoroutines();
        //points
        int pointsAwarded = Mathf.RoundToInt(pointsOnDeath * PlayerStats.Instance.deathPointsMultiplier);
        PlayerStats.Instance.AddPoints(pointsAwarded);

        roundManager?.EnemyKilled();
        Destroy(gameObject, deathAnimationDuration); // Allow death animation to play
    }

    private IEnumerator AttackPlayer()
    {
        if (playerHealth == null) yield break;

        isAttacking = true;
        anim.SetBool("IsAttacking", true);
        agent.isStopped = true;

        while (!isDead)
        {
            yield return new WaitForSeconds(attackDamageDelay);

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= attackRange && !isDead)
            {
                PlayRandomSound(attackSounds);
                playerHealth.TakeDamage(damage);
            }
            else if (distance > attackRange)
            {
                break;
            }

            float remainingWaitTime = attackInterval - attackDamageDelay;
            if (remainingWaitTime > 0)
            {
                yield return new WaitForSeconds(remainingWaitTime);
            }
        }

        StopAttack();
    }

    private void StopAttack()
    {
        isAttacking = false;
        anim.SetBool("IsAttacking", false);
        agent.isStopped = false;
    }
    IEnumerator AmbientSoundRoutine()
    {
        while (!isDead)
        {
            float waitTime = Random.Range(minAmbientInterval, maxAmbientInterval);
            yield return new WaitForSeconds(waitTime);

            if (!isDead && !isAttacking)
            {
                PlayRandomSound(ambientSounds);
            }
        }
    }

    void PlayRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];

        audioSource.pitch = basePitch * Random.Range(0.85f, 1.15f);

        audioSource.PlayOneShot(clip);

    }
}

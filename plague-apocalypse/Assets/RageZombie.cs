using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class RageZombie : MonoBehaviour
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
    // 1. RAGE MODE SETTINGS (MODIFIED)
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

    void Update()
    {
        if (isDead || player == null || agent == null || isScreaming) return;

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

    // ==========================================
    // 4. DAMAGE LOGIC (MODIFIED)
    // ==========================================
    public void TakeDamage(int damage)
    {
        if (isDead) return;

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
}
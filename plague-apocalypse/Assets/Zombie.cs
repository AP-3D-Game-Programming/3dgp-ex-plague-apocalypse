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

    [HideInInspector] public RoundManager roundManager;

    private NavMeshAgent agent;
    private Transform player;
    private PlayerHealth playerHealth;
    private Animator anim;

    private bool isAttacking = false;
    private bool isDead = false;

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
            agent.stoppingDistance = attackRange; // Stop just at attack range
        }
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
        if (health <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        anim.SetBool("IsDead", true);
        agent.isStopped = true;
        roundManager?.EnemyKilled();
        Destroy(gameObject, 3f); // Allow death animation to play
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

            // Stop attacking if player moves out of range
            if (distance > attackRange) break;

            // Apply damage
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

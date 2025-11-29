using UnityEngine;

public class GunRobot : MonoBehaviour
{
    [Header("Combat Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 20f;
    public float attackRange = 10f; // Distance at which robot starts shooting
    public int maxHealth = 100;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float hoverHeight = 5f; // height above ground to maintain
    public float hoverSmoothness = 2f; // how fast it adjusts height
    [Header("Effects")]
    public ParticleSystem shootEffect;
    [Header("Points")]
    public int pointsPerShot = 10;
    public int pointsOnDeath = 100;
    private int accumulatedPoints = 0;
    [HideInInspector] public RoundManager roundManager;

    private Transform player;
    private float fireCooldown = 0f;
    private int currentHealth;
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("GunRobot: Player not found!");
    }

    void Update()
    {

        if (isDead || player == null) return;

        // Calculate direction and distance to player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        // Move toward player if outside attack range
        if (distance > attackRange)
        {
            transform.position += directionToPlayer * moveSpeed * Time.deltaTime;
        }

        // Maintain hover height
        Vector3 targetPosition = transform.position;
        targetPosition.y = Mathf.Lerp(transform.position.y, hoverHeight, Time.deltaTime * hoverSmoothness);
        transform.position = targetPosition;

        // Rotate to face player
        Vector3 lookDirection = player.position - transform.position;

        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);

        // Shoot if in range
        if (distance <= attackRange)
        {
            fireCooldown -= Time.deltaTime;
            if (fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = 1f / fireRate;
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;
        if (shootEffect != null)
            shootEffect.Play();

        //  offset to aim slightly above the player's feet
        float aimHeightOffset = 1f;
        Vector3 targetPoint = player.position + Vector3.up * aimHeightOffset;

        // Calculate aim direction
        Vector3 aimDirection = (targetPoint - firePoint.position).normalized;
        firePoint.rotation = Quaternion.LookRotation(aimDirection);

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = firePoint.forward * bulletSpeed;
    }



    public void ApplyScaledStats(int scaledHealth, float scaledSpeed, float scaledFireRate)
    {
        maxHealth = scaledHealth;
        currentHealth = scaledHealth;

        moveSpeed = scaledSpeed;
        fireRate = scaledFireRate;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (accumulatedPoints < PlayerStats.Instance.maxShootPointsPerEnemy)
        {
            int pointsToGive = Mathf.Min(
                Mathf.RoundToInt(pointsPerShot * PlayerStats.Instance.shotPointsMultiplier),
                PlayerStats.Instance.maxShootPointsPerEnemy - accumulatedPoints
            );

            accumulatedPoints += pointsToGive;
            PlayerStats.Instance.AddPoints(pointsToGive);
        }
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        //points
        int pointsAwarded = Mathf.RoundToInt(pointsOnDeath * PlayerStats.Instance.deathPointsMultiplier);
        PlayerStats.Instance.AddPoints(pointsAwarded);

        // Optional: play death animation or effects here
        Destroy(gameObject, 2f);

        // Notify RoundManager
        roundManager?.EnemyKilled();
    }
}

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
public class MechEnemy : MonoBehaviour, IElite
{
    // ==========================================
    // 1. STATS & POINTS
    // ==========================================
    [Header("Base Stats")]
    public int health = 200;
    private int maxHealth;
    public float moveSpeed = 3.5f;

    [Header("Points System")]
    public int pointsPerShot = 10;
    public int pointsOnDeath = 250;
    private int accumulatedPoints = 0;

    [Header("Body Control")]
    public Transform body;
    public float bodyRotationSpeed = 10f;
    public Vector3 rotationOffset;
    private float globalFireRateMult = 1f;
    private float globalDamageMult = 1f;
    [Header("Effects")]
    public GameObject explosionPrefab;
    // ==========================================
    // 2. MECH WEAPON SYSTEM
    // ==========================================
    [Header("Combat Configuration")]
    public float attackRange = 20f;
    public float rotationSpeed = 5f;

    public List<WeaponConfig> weapons;

    [System.Serializable]
    public class WeaponConfig
    {
        public string name;
        public GameObject projectilePrefab;
        public Transform firePoint;
        public float fireRate = 0.5f;
        public float projectileSpeed = 20f;
        public string animTrigger;
        public float startDelay = 0f;
        public AudioClip[] fireSounds;
        [Header("Mortar Settings")]
        public bool isMortar = false; // CHECK THIS TRUE FOR MORTARS
        public float lobHeight = 5f;  // How high the arc goes

        [HideInInspector] public float nextFireTime;
    }
    [Header("Music Settings")]
    public AudioClip mechMusic;
    // ==========================================
    // 3. INTERNAL REFERENCES
    // ==========================================
    private NavMeshAgent agent;
    private Transform player;
    private Animator anim;
    private bool isDead = false;

    [HideInInspector] public RoundManager roundManager;
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] ambientSounds;
    public AudioClip[] hurtSounds;
    public AudioClip[] deathSounds;

    public float minAmbientInterval = 3f;
    public float maxAmbientInterval = 8f;
    // ==========================================
    // 4. IELITE INTERFACE
    // ==========================================
    public void ApplyStats(
            int newHealth,
            float newSpeed,
            RoundManager manager,
            float fireRateMult,
            float damageMult,
            float phase2HealthMult,
            float phase2SpeedMult
        )
    {
        this.health = newHealth;
        this.maxHealth = newHealth;
        this.moveSpeed = newSpeed;
        this.roundManager = manager;


        this.globalFireRateMult = fireRateMult;
        this.globalDamageMult = damageMult;

        foreach (var weapon in weapons)
        {
            weapon.fireRate /= globalFireRateMult;
        }

        if (agent != null)
            agent.speed = moveSpeed;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        player = GameObject.FindWithTag("Player")?.transform;

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = 5f;
        }
        maxHealth = health;
    }
    void Start()
    {
        if (MusicManager.Instance != null && mechMusic != null)
        {
            MusicManager.Instance.RequestMusic(mechMusic, 1);
        }
        StartCoroutine(AmbientSoundRoutine());
    }
    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // --- ANIMATION SPEED CONTROL ---
        if (anim != null && agent != null)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }

        // --- COMBAT LOGIC ---
        if (distance <= attackRange)
        {
            EngageCombat();
        }
        else
        {
            ChasePlayer();
        }
    }

    void LateUpdate()
    {
        if (isDead || player == null || body == null) return;
        RotateTorso();
    }

    void RotateTorso()
    {
        Vector3 directionToPlayer = player.position - body.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            Quaternion correctedRotation = lookRotation * Quaternion.Euler(rotationOffset);
            body.rotation = Quaternion.Slerp(body.rotation, correctedRotation, Time.deltaTime * bodyRotationSpeed);
        }
    }

    private float pathUpdateDelay = 0.2f;
    private float pathUpdateTimer = 0f;
    private bool combatStarted = false;

    void ChasePlayer()
    {
        combatStarted = false;
        if (agent == null) return;
        agent.isStopped = false;

        if (Time.time >= pathUpdateTimer)
        {
            agent.SetDestination(player.position);
            pathUpdateTimer = Time.time + pathUpdateDelay;
        }
    }

    void EngageCombat()
    {
        if (agent == null) return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (!combatStarted)
        {
            combatStarted = true;
            foreach (var weapon in weapons)
            {
                weapon.nextFireTime = Time.time + weapon.startDelay;
            }
        }

        FireWeapons();
    }

    void FireWeapons()
    {
        foreach (var weapon in weapons)
        {
            if (Time.time >= weapon.nextFireTime)
            {
                Shoot(weapon);
                weapon.nextFireTime = Time.time + weapon.fireRate;
            }
        }
    }

    void Shoot(WeaponConfig weapon)
    {
        if (weapon.projectilePrefab == null || weapon.firePoint == null) return;
        PlayRandomSound(weapon.fireSounds);
        // 1. Play Animation
        if (anim != null && !string.IsNullOrEmpty(weapon.animTrigger))
        {
            anim.SetTrigger(weapon.animTrigger);
        }

        // 2. Instantiate
        GameObject projectile = Instantiate(weapon.projectilePrefab, weapon.firePoint.position, weapon.firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            if (weapon.isMortar)
            {
                // === MORTAR LOGIC (ARC) ===
                Vector3 velocity = CalculateLobVelocity(weapon.firePoint.position, player.position, weapon.lobHeight);
                rb.linearVelocity = velocity;

                // Optional: Rotate the shell to face where it's flying
                if (velocity != Vector3.zero)
                    projectile.transform.rotation = Quaternion.LookRotation(velocity);
            }
            else
            {
                // === STANDARD GUN LOGIC (STRAIGHT) ===
                Vector3 targetPos = player.position + Vector3.up; // Aim slightly up at chest
                Vector3 aimDir = (targetPos - weapon.firePoint.position).normalized;
                projectile.transform.rotation = Quaternion.LookRotation(aimDir);
                rb.linearVelocity = aimDir * weapon.projectileSpeed;
            }
        }

        Destroy(projectile, 5f);
    }

    // --- MATH FOR MORTAR ARC ---
    Vector3 CalculateLobVelocity(Vector3 origin, Vector3 target, float height)
    {
        float gravity = Physics.gravity.y;
        float displacementY = target.y - origin.y;
        Vector3 displacementXZ = new Vector3(target.x - origin.x, 0, target.z - origin.z);

        // Basic physics check: height must be higher than the target y difference
        if (displacementY >= height) height = displacementY + 1f;

        // Calculate vertical velocity needed to reach height
        // v_y = sqrt(-2 * g * h)
        float velocityY = Mathf.Sqrt(-2 * gravity * height);

        // Calculate time to reach the peak and time to fall from peak to target
        float timeToPeak = Mathf.Sqrt(-2 * height / gravity);
        float timeToFall = Mathf.Sqrt(2 * (displacementY - height) / gravity);
        float totalTime = timeToPeak + timeToFall;

        // Calculate horizontal velocity needed to cover distance in that time
        Vector3 velocityXZ = displacementXZ / totalTime;

        return velocityXZ + Vector3.up * velocityY;
    }

    public void ShootBigCanonA() { }
    public void ShootBigCanonB() { }
    public void ShootSmallCanonA() { }
    public void ShootSmallCanonB() { }
    public void EndOfWalk() { }

    // ==========================================
    // 6. DAMAGE & DEATH
    // ==========================================
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        health -= damageAmount;
        PlayRandomSound(hurtSounds);
        if (accumulatedPoints < PlayerStats.Instance.maxShootPointsPerEnemy)
        {
            int pointsToGive = Mathf.Min(
                Mathf.RoundToInt(pointsPerShot * PlayerStats.Instance.shotPointsMultiplier),
                PlayerStats.Instance.maxShootPointsPerEnemy - accumulatedPoints
            );
            accumulatedPoints += pointsToGive;
            PlayerStats.Instance.AddPoints(pointsToGive);
        }
        if (health <= 0) Die();
    }

    private void Die()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopRequest(1);
        }
        isDead = true;
        if (explosionPrefab != null)
        {
            Vector3 spawnPos = (body != null) ? body.position : transform.position;
            GameObject explosion = Instantiate(explosionPrefab, spawnPos, Quaternion.identity);

            Destroy(explosion, 4f);
        }
        if (agent != null) agent.isStopped = true;
        if (anim != null) anim.SetBool("IsDead", true);
        PlayRandomSound(deathSounds);
        if (PlayerStats.Instance != null)
        {
            int pointsAwarded = Mathf.RoundToInt(pointsOnDeath * PlayerStats.Instance.deathPointsMultiplier);
            PlayerStats.Instance.AddPoints(pointsAwarded);
        }

        if (roundManager != null) roundManager.EnemyKilled();
        Destroy(gameObject, 0.5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    IEnumerator AmbientSoundRoutine()
    {
        while (!isDead)
        {
            float waitTime = Random.Range(minAmbientInterval, maxAmbientInterval);
            yield return new WaitForSeconds(waitTime);

            if (!isDead)
            {
                PlayRandomSound(ambientSounds);
            }
        }
    }

    void PlayRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        audioSource.pitch = Random.Range(0.8f, 1.0f);

        audioSource.PlayOneShot(clip);
    }
}
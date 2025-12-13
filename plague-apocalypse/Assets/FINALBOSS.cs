using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FINALLBOSS : MonoBehaviour
{
    public enum BossState { Spawning, Moving, Attacking, Dead }

    [Header("State")]
    public BossState currentState = BossState.Spawning;

    [Header("General Settings")]
    public float attackRange = 25f;
    public int maxHealth = 1000;
    public Animator animator; // Link your Animator here!

    [Header("Movement (Erratic)")]
    public float hoverHeight = 8f;
    public float normalMoveSpeed = 5f;
    public float dashSpeed = 50f; // Very fast "teleport" speed
    public float dashInterval = 3f; // How often he changes angle

    [Header("Attack 1: Rocket")]
    public GameObject rocketPrefab;
    public Transform firePoint;
    public float rocketSpeed = 25f;

    [Header("Attack 2: Chaos Beam")]
    public LineRenderer beamLineRenderer; // Assign a LineRenderer component
    public float beamDuration = 3f;
    public float beamDamageRate = 0.1f; // How often damage ticks


    [Header("Attack 3: Meteor Shower")]
    public GameObject meteorPrefab;
    public float meteorHeight = 20f; // How high he flies
    public int meteorCount = 10;
    public float meteorAreaSize = 30f; // Radius of rain

    [Header("Attack 4: Time Stop (Ultimate)")]
    public GameObject ultBulletPrefab;
    public float timeSlowFactor = 0.05f; // Game slows to 5% speed
    public Color timeStopColor = Color.cyan; // Optional: change material color
    [Header("Phase Settings (NEW)")]
    public float phaseTransitionHealthRatio = 0.5f; // 50% HP threshold (500/1000)
    [Tooltip("Damage reduction for Phase 1 (1.5 means 50% damage bonus to player)")]
    public float phaseOneDamageMultiplier = 1.5f; // 1.5x damage taken (Negative reduction)
    [Tooltip("Damage reduction for Phase 2 (0.9 means 10% damage reduction)")]
    public float phaseTwoDamageMultiplier = 0.9f; // 0.9x damage taken

    private int currentPhase = 1;

    [Header("Heal & Shield Attack (NEW)")]
    public float healAmountRatio = 0.05f; // 5% of max HP
    public float shieldAmountRatio = 0.05f; // 5% of max HP
    public float healAttackCooldown = 180f; // 3 minutes in seconds
    private float lastHealTime;
    private float currentShield = 0f; // Track the current shield amount

    [Header("Ultimate Spammer Settings (NEW)")]
    public float spammerDuration = 5f; // How long the ult lasts
    public int beamSpins = 4; // How many times the beam spins 360 degrees
    public int meteorSpamCount = 30; // More meteors for the ult
    public float rocketShotInterval = 0.2f; // How often to shoot rockets during the ult
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip screamSound;
    public AudioClip beamSound;
    public AudioClip timeStopSound;
    public AudioClip shootSound;
    public AudioClip deathSound;
    public AudioClip meteorSummonSound;
    public AudioClip dashSound;
    public AudioClip healSound;
    public AudioClip phaseTransitionSound;

    [Header("Visual Effects")]
    public GameObject healEffectPrefab;
    public GameObject spawnVFX;
    public GameObject dashVFX;
    public GameObject muzzleFlashVFX;
    public GameObject beamChargeVFX;
    public GameObject meteorSummonVFX;
    public GameObject timeStopWaveVFX;
    public GameObject rageAuraVFX;
    public GameObject phaseTransitionVFX;
    public GameObject deathExplosionVFX;
    public GameObject shieldAuraVFX;
    [Header("--- PERSISTENT AURAS ---")]
    public GameObject phaseOneAura;
    public GameObject phaseTwoAura;
    [Header("Music Control (Simple Swap)")]
    public AudioSource ambientMusicSource;
    public AudioClip bossMusicSource;
    public AudioClip phaseTwoMusic;
    [Header("Points Integration")]
    public int pointsOnDeath = 500;
    [HideInInspector] public RoundManager roundManager;

    private Transform player;
    private Rigidbody rb;
    private int currentHealth;
    private float dashTimer;
    private float attackCooldown = 2f;
    private Vector3 targetMovePosition;

    // Internal flags
    private bool isIntroComplete = false;
    private int attacksSinceTimeStop = 0;
    private int attacksSinceSpammer = 0;

    [Header("Pity System Settings")]
    public int timeStopPityThreshold = 5;
    public int spammerPityThreshold = 5;
    public float fakeOutDuration = 17f; // How long he stays "dead"
    public AudioClip fakeOutMusic;
    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Ensure gravity doesn't pull him down

        // Cache player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (beamLineRenderer != null) beamLineRenderer.enabled = false;
    }

    void Start()
    {
        if (phaseOneAura != null) phaseOneAura.SetActive(true);
        if (phaseTwoAura != null) phaseTwoAura.SetActive(false);
        if (shieldAuraVFX != null) shieldAuraVFX.SetActive(false);
        if (animator) animator.SetBool("IsAttacking", false);
        // Start the Intro Sequence
        StartCoroutine(IntroSequence());
    }

    void Update()
    {
        if (player == null || currentState == BossState.Dead) return;

        // Ensure boss faces player (unless doing specific attacks that override this)
        if (currentState != BossState.Spawning)
        {
            FacePlayer();
        }

        switch (currentState)
        {
            case BossState.Moving:
                HandleMovement();
                HandleAttackLogic();
                break;
        }
    }
    private void PlayVFX(GameObject prefab, Vector3 position, float duration = 2f)
    {
        if (prefab != null)
        {
            GameObject vfx = Instantiate(prefab, position, Quaternion.identity);
            Destroy(vfx, duration);
        }
    }

    private GameObject PlayAttachedVFX(GameObject prefab, float duration)
    {
        if (prefab != null)
        {
            GameObject vfx = Instantiate(prefab, transform.position, Quaternion.identity, transform);
            Destroy(vfx, duration);
            return vfx;
        }
        return null;
    }
    // ---------------------------------------------------------
    // 1. INTRO SEQUENCE
    // ---------------------------------------------------------
    IEnumerator IntroSequence()
    {

        if (ambientMusicSource != null)
        {
            ambientMusicSource.Stop();
        }
        if (MusicManager.Instance != null && bossMusicSource != null)
        {
            // Request Phase 1 music at Priority 2 (Highest)
            MusicManager.Instance.RequestMusic(bossMusicSource, 2);
        }
        currentState = BossState.Spawning;
        PlayVFX(spawnVFX, transform.position, 3f);
        // Wait 2 seconds after spawning
        yield return new WaitForSeconds(2f);

        // Scream Animation
        if (animator) animator.SetTrigger("Scream");
        if (audioSource && screamSound) audioSource.PlayOneShot(screamSound, 2f);

        yield return new WaitForSeconds(2f); // Wait for scream to finish

        isIntroComplete = true;
        currentState = BossState.Moving;
        dashTimer = Time.time; // Reset dash timer
        if (BossUI.Instance != null)
        {
            BossUI.Instance.ShowHealthBar();
            if (BossUI.Instance != null)
            {
                BossUI.Instance.UpdateHealthBar(currentHealth, maxHealth, currentShield);
            }
        }
    }

    // ---------------------------------------------------------
    // 2. ERRATIC MOVEMENT
    // ---------------------------------------------------------
    void HandleMovement()
    {
        // Calculate the ideal height position
        float targetY = player.position.y + hoverHeight;

        // Dash Logic: Every few seconds, pick a new random angle around the player
        if (Time.time > dashTimer + dashInterval)
        {
            if (audioSource && dashSound)
            {
                audioSource.PlayOneShot(dashSound, 10);
            }
            PlayVFX(dashVFX, transform.position, 1f);
            // Pick a random point on a circle around the player
            Vector2 randomCircle = Random.insideUnitCircle.normalized * 10f; // 10 units away
            Vector3 randomOffset = new Vector3(randomCircle.x, 0, randomCircle.y);

            targetMovePosition = player.position + randomOffset;
            targetMovePosition.y = targetY; // Maintain height

            dashTimer = Time.time;
        }

        // Lerp towards target. If we just changed target, we move fast (Teleport-ish feel)
        float currentSpeed = (Time.time - dashTimer < 0.5f) ? dashSpeed : normalMoveSpeed;

        // Use MovePosition for physics-safe movement
        Vector3 newPos = Vector3.Lerp(transform.position, targetMovePosition, Time.deltaTime * currentSpeed);
        rb.MovePosition(newPos);
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0; // Keep rotation flat
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
        }
    }

    // ---------------------------------------------------------
    // 3. ATTACK SELECTION
    // ---------------------------------------------------------
    void HandleAttackLogic()
    {
        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0)
        {
            StartCoroutine(SelectAndPerformAttack());
        }
    }

    IEnumerator SelectAndPerformAttack()
    {
        currentState = BossState.Attacking;

        // Check for Heal/Shield Cooldown first
        bool canHeal = Time.time > lastHealTime + healAttackCooldown;

        // --- PITY CHECK ---
        bool guaranteeTimeStop = (currentPhase == 1 && attacksSinceTimeStop >= timeStopPityThreshold);
        bool guaranteeSpammer = (currentPhase == 2 && attacksSinceSpammer >= spammerPityThreshold);
        // ------------------

        if (animator) animator.SetBool("IsAttacking", true);
        float rand = Random.value;

        // =========================================================
        // PHASE 1 LOGIC (Remains the same)
        // =========================================================
        if (currentPhase == 1)
        {
            if (guaranteeTimeStop)
            {
                Debug.Log("PITY: Guaranteeing Time Stop.");
                attacksSinceTimeStop = 0;
                yield return Attack_TimeStop();
            }
            else if (canHeal && rand < 0.2f)
            {
                yield return Attack_HealAndShield();
                attackCooldown = Random.Range(4f, 6f);
            }
            else
            {
                // 40% Rocket, 30% Beam, 20% Meteor, 10% Time Stop
                if (rand < 0.4f) yield return Attack_Rocket();
                else if (rand < 0.7f) yield return Attack_ChaosBeam();
                else if (rand < 0.9f) yield return Attack_MeteorRain();
                else yield return Attack_TimeStop();

                attackCooldown = Random.Range(3f, 4f);
            }
        }
        // =========================================================
        // PHASE 2 LOGIC (Updated to include BOTH Ultimates)
        // =========================================================
        else if (currentPhase == 2)
        {
            if (guaranteeSpammer)
            {
                Debug.Log("PITY: Guaranteeing Spammer Ultimate.");
                attacksSinceSpammer = 0;
                yield return Ultimate_Spammer();
            }
            else if (canHeal && rand < 0.15f)
            {
                yield return Attack_HealAndShield();
                attackCooldown = Random.Range(4f, 6f);
            }
            else
            {
                // NEW PROBABILITIES FOR PHASE 2:
                // 0.0 - 0.3 (30%): Rocket
                // 0.3 - 0.6 (30%): Chaos Beam
                // 0.6 - 0.8 (20%): Meteor Shower
                // 0.8 - 0.9 (10%): TIME STOP 
                // 0.9 - 1.0 (10%): SPAMMER 

                if (rand < 0.3f) yield return Attack_Rocket();
                else if (rand < 0.6f) yield return Attack_ChaosBeam();
                else if (rand < 0.8f) yield return Attack_MeteorRain();
                else if (rand < 0.9f) yield return Attack_TimeStop();
                else yield return Ultimate_Spammer();

                attackCooldown = Random.Range(1.2f, 2f);
            }
        }

        // --- UPDATE PITY COUNTERS ---
        // (This logic needs to check if we actually DID the ult this turn)

        if (currentPhase == 1)
        {
            // If random roll fell into TimeStop range (>= 0.9), reset. Else increment.
            if (!guaranteeTimeStop && rand >= 0.9f) attacksSinceTimeStop = 0;
            else if (!guaranteeTimeStop) attacksSinceTimeStop++;
        }

        // P2: Increment if we didn't do Spammer
        if (currentPhase == 2)
        {
            // If random roll fell into Spammer range (>= 0.9), reset. Else increment.
            if (!guaranteeSpammer && rand >= 0.9f) attacksSinceSpammer = 0;
            else if (!guaranteeSpammer) attacksSinceSpammer++;
        }

        currentState = BossState.Moving;
        if (animator) animator.SetBool("IsAttacking", false);
    }
    // --- ATTACK 1: ROCKET ---
    IEnumerator Attack_Rocket()
    {
        if (animator) animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.5f); // Sync with anim

        if (audioSource && shootSound) audioSource.PlayOneShot(shootSound);
        PlayVFX(muzzleFlashVFX, firePoint.position, 1f);
        GameObject rocket = Instantiate(rocketPrefab, firePoint.position, firePoint.rotation);

        // Aim at player
        rocket.transform.LookAt(player.position + Vector3.up);
        Rigidbody rocketRb = rocket.GetComponent<Rigidbody>();
        if (rocketRb) rocketRb.linearVelocity = rocket.transform.forward * rocketSpeed;

        yield return new WaitForSeconds(1f); // Recovery
    }
    // --- ATTACK 2: CHAOS BEAM (Wiggles on X-Z plane) ---
    IEnumerator Attack_ChaosBeam()
    {
        if (animator) animator.SetTrigger("Shoot");
        // Use the dedicated 3D audio source if you set one up, otherwise use audioSource
        // if (audioSource3D && beamSound) audioSource3D.PlayOneShot(beamSound);
        if (audioSource && beamSound) audioSource.PlayOneShot(beamSound);
        PlayAttachedVFX(beamChargeVFX, 1f);
        // Charge up
        yield return new WaitForSeconds(1f);

        beamLineRenderer.enabled = true;
        float duration = beamDuration;

        while (duration > 0)
        {
            beamLineRenderer.SetPosition(0, firePoint.position);

            // 1. Calculate a Wiggle on the X-Z (Horizontal) Plane
            // Use different speeds (20 and 25) to make the pattern complex and unpredictable.
            // The result is scaled by 5f, so the beam sweeps up to 5 units away from the target point.
            Vector3 horizontalWiggle = new Vector3(
                Mathf.Sin(Time.time * 20) * 5f, // X-Wiggle (Left/Right)
                0,                              // Y-Wiggle (Keep this 0 to stay flat)
                Mathf.Cos(Time.time * 25) * 5f  // Z-Wiggle (Forward/Backward)
            );

            // 2. Define the target as the player's ground position + the chaotic horizontal sweep.
            // This ensures the beam targets the floor level, not the player's center.
            Vector3 target = player.position;
            target.y = player.position.y; // Ensure target is locked to player's ground level
            target += horizontalWiggle;    // Add the chaotic sweep

            beamLineRenderer.SetPosition(1, target);

            // Simple Distance check for damage (Use a capsule check or Physics.CheckSphere 
            // around the player to see if they are within the sweeping beam's path)
            if (Vector3.Distance(target, player.position) < 3f)
            {
                // Apply small damage to player here
                player.GetComponent<PlayerHealth>().TakeDamage(1);
            }

            duration -= Time.deltaTime;
            yield return null;
        }

        beamLineRenderer.enabled = false;
    }

    // --- ATTACK 3: METEOR SHOWER ---
    IEnumerator Attack_MeteorRain()
    {
        if (animator) animator.SetTrigger("Spin");
        PlayAttachedVFX(meteorSummonVFX, 2f);
        // Fly Up
        Vector3 startPos = transform.position;
        Vector3 highPos = startPos + Vector3.up * 10f;

        float t = 0;
        while (t < 1f)
        {
            transform.position = Vector3.Lerp(startPos, highPos, t);
            t += Time.deltaTime * 2f;
            yield return null;
        }
        if (audioSource && meteorSummonSound)
        {
            audioSource.PlayOneShot(meteorSummonSound);
        }
        // Spawn Meteors
        for (int i = 0; i < meteorCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * meteorAreaSize;
            Vector3 spawnPos = new Vector3(player.position.x + randomCircle.x, player.position.y + 20f, player.position.z + randomCircle.y);

            Instantiate(meteorPrefab, spawnPos, Quaternion.Euler(90, 0, 0)); // Aim down

            yield return new WaitForSeconds(0.2f);
        }

        // Return down
        t = 0;
        while (t < 1f)
        {
            transform.position = Vector3.Lerp(highPos, startPos, t);
            t += Time.deltaTime * 2f;
            yield return null;
        }
    }

    // --- ATTACK 4: TIME STOP (ULTIMATE) ---
    IEnumerator Attack_TimeStop()
    {
        if (animator) animator.SetTrigger("Spin");
        if (audioSource && timeStopSound) audioSource.PlayOneShot(timeStopSound);
        PlayVFX(timeStopWaveVFX, transform.position, 4f);
        yield return new WaitForSeconds(1.3f); // Charge

        // STOP TIME
        Time.timeScale = timeSlowFactor;
        // Ensure physics doesn't jitter by scaling fixed timestep
        Time.fixedDeltaTime = 0.02f * Time.timeScale;


        // Play shoot animation
        if (animator) animator.SetTrigger("Shoot");

        yield return new WaitForSecondsRealtime(5.3f);

        // Shoot the ultimate bullet
        if (ultBulletPrefab)
        {
            PlayVFX(muzzleFlashVFX, firePoint.position, 1.5f);
            GameObject bullet = Instantiate(ultBulletPrefab, firePoint.position, firePoint.rotation);

            // AIM AT PLAYER (using the aim height logic from your original script)
            Vector3 targetPoint = player.position + Vector3.up * 1f;
            Vector3 aimDirection = (targetPoint - firePoint.position).normalized;
            bullet.transform.rotation = Quaternion.LookRotation(aimDirection);


            // *** CRUCIAL STEP: Pass the speed to the TimeScaleIgnoringProjectile script ***
            TimeIgnore movementScript = bullet.GetComponent<TimeIgnore>();
            if (movementScript != null)
            {
                movementScript.moveSpeed = rocketSpeed;
            }
        }

        yield return new WaitForSecondsRealtime(1f);

        // RESUME TIME
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
    // --- NEW ATTACK 5: HEAL & SHIELD ---
    IEnumerator Attack_HealAndShield()
    {
        if (animator) animator.SetTrigger("Heal");
        if (audioSource && healSound)
        {
            audioSource.PlayOneShot(healSound);
        }
        if (healEffectPrefab)
        {
            // Instantiate the effect and destroy it after 2 seconds
            GameObject effect = Instantiate(healEffectPrefab, transform.position, Quaternion.identity, transform);
            Destroy(effect, 2f);
        }
        // Calculate values
        int healAmount = Mathf.RoundToInt(maxHealth * healAmountRatio);
        float shieldGain = maxHealth * shieldAmountRatio;

        // 1. Heal
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 2. Gain Shield
        currentShield += shieldGain;
        if (shieldAuraVFX != null)
        {
            shieldAuraVFX.SetActive(true);
        }
        // Log the action (good for testing)
        Debug.Log($"Boss used HEAL & SHIELD: Healed {healAmount} HP. Shield now: {currentShield:F0}");

        // Update the health bar with the shield
        if (BossUI.Instance != null)
        {
            // Add 'currentShield' as the 3rd argument
            BossUI.Instance.UpdateHealthBar(currentHealth, maxHealth, currentShield);
        }
        lastHealTime = Time.time;
        yield return new WaitForSeconds(2f); // Attack delay/recovery
    }
    IEnumerator Ultimate_Spammer()
    {
        if (animator) animator.SetTrigger("Spin");
        if (audioSource && timeStopSound) audioSource.PlayOneShot(timeStopSound);
        PlayAttachedVFX(rageAuraVFX, spammerDuration + 2f);
        float startTime = Time.time;
        float rocketTimer = 0f;
        float currentSpinAngle = 0f;

        // Fly Up
        Vector3 startPos = transform.position;
        Vector3 highPos = startPos + Vector3.up * 15f;

        float t = 0;
        while (t < 1f)
        {
            transform.position = Vector3.Lerp(startPos, highPos, t);
            t += Time.deltaTime * 2f;
            yield return null;
        }

        // Enable Beam
        if (beamLineRenderer != null)
        {
            beamLineRenderer.enabled = true;
        }

        // --- CHAOS SPAM LOOP ---
        while (Time.time < startTime + spammerDuration)
        {
            // 1. Spin the boss
            float spinDelta = 360f / spammerDuration * beamSpins * Time.deltaTime;
            currentSpinAngle += spinDelta;
            transform.rotation = Quaternion.Euler(0, currentSpinAngle, 0);

            // --- UPDATED BEAM LOGIC (Angles Down + Vertical Sweep) ---
            if (beamLineRenderer != null && beamLineRenderer.enabled)
            {
                beamLineRenderer.SetPosition(0, firePoint.position);

                // Calculate Wiggle: Now includes Y (Vertical) movement!
                // We use a different speed (15) for Y so it doesn't sync perfectly with X/Z
                Vector3 localWiggle = new Vector3(
                    Mathf.Sin(Time.time * 20) * 5f,
                    Mathf.Sin(Time.time * 15) * 8f, // Y-Wiggle (Sweeps up and down)
                    Mathf.Cos(Time.time * 25) * 5f
                );

                // Define Target Direction:
                // transform.forward is straight out. We subtract Vector3.up * 0.5f to tilt it DOWN.
                // You can increase 0.5f to 1.0f to aim steeper into the floor.
                Vector3 downwardTilt = (transform.forward - Vector3.up * 0.5f).normalized;
                Vector3 targetDirection = downwardTilt * 50f;

                // Calculate Beam End
                Vector3 beamEnd = firePoint.position + targetDirection + transform.TransformVector(localWiggle * 0.5f);

                beamLineRenderer.SetPosition(1, beamEnd);

                // Damage Check
                Vector3 closestPointToPlayer = GetClosestPointOnLineSegment(firePoint.position, beamEnd, player.position);
                if (Vector3.Distance(closestPointToPlayer, player.position) < 3f)
                {
                    player.GetComponent<PlayerHealth>().TakeDamage(1);
                }
            }

            // --- UPDATED ROCKET LOGIC (Random Downward Spray) ---
            rocketTimer += Time.deltaTime;
            if (rocketTimer >= rocketShotInterval)
            {
                if (audioSource && shootSound) audioSource.PlayOneShot(shootSound, 0.7f);
                PlayVFX(muzzleFlashVFX, firePoint.position, 0.2f);
                // Calculate Random Pitch (Vertical Angle)
                // 0 = Horizontal, 90 = Straight Down.
                // Range: 10 (Slightly Down) to 60 (Steeply Down)
                float randomPitch = Random.Range(10f, 60f);

                // Apply rotation: Random Pitch (X) + Current Spin (Y)
                Quaternion rocketRotation = Quaternion.Euler(randomPitch, currentSpinAngle, 0);

                GameObject rocket = Instantiate(rocketPrefab, firePoint.position, rocketRotation);
                Rigidbody rocketRb = rocket.GetComponent<Rigidbody>();

                // Use the rocket's own NEW rotation to determine forward direction
                if (rocketRb) rocketRb.linearVelocity = rocket.transform.forward * rocketSpeed;

                rocketTimer = 0f;
            }

            // 3. Meteor Spam
            if (Random.value < 0.1f)
            {
                Vector2 randomCircle = Random.insideUnitCircle * meteorAreaSize;
                Vector3 spawnPos = new Vector3(player.position.x + randomCircle.x, player.position.y + 20f, player.position.z + randomCircle.y);
                Instantiate(meteorPrefab, spawnPos, Quaternion.Euler(90, 0, 0));
            }

            yield return null;
        }

        // Disable Beam
        if (beamLineRenderer != null)
        {
            beamLineRenderer.enabled = false;
        }

        // Return to start position
        t = 0;
        while (t < 1f)
        {
            transform.position = Vector3.Lerp(highPos, startPos, t);
            t += Time.deltaTime * 2f;
            yield return null;
        }
        transform.rotation = Quaternion.LookRotation(player.position - transform.position);
    }
    // ---------------------------------------------------------
    // HEALTH & DAMAGE (Your original logic)
    // ---------------------------------------------------------
    public void TakeDamage(int damage)
    {
        if (currentState == BossState.Dead || currentState == BossState.Spawning) return;

        // 1. Calculate Damage Multiplier based on current Phase
        float damageMultiplier = (currentPhase == 1) ? phaseOneDamageMultiplier : phaseTwoDamageMultiplier;
        int modifiedDamage = Mathf.RoundToInt(damage * damageMultiplier);

        // 2. Apply Shield Logic (if any shield exists)
        if (currentShield > 0)
        {
            float damageToShield = Mathf.Min(modifiedDamage, currentShield);
            currentShield -= damageToShield;
            modifiedDamage -= (int)damageToShield; // Reduce damage dealt to health
            if (currentShield <= 0)
            {
                currentShield = 0;
                if (shieldAuraVFX != null) shieldAuraVFX.SetActive(false);
            }
            if (currentShield < 0) currentShield = 0; // Prevent negative shield
        }

        // 3. Apply Damage to Health
        currentHealth -= modifiedDamage;

        Debug.Log($"P{currentPhase} Boss hit! Raw:{damage}. Effective:{modifiedDamage}. Shield:{currentShield:F0}. New Health:{currentHealth}/{maxHealth}");

        // 4. Check for Phase Transition
        if (currentPhase == 1 && (float)currentHealth / maxHealth <= phaseTransitionHealthRatio)
        {
            StopAllCoroutines();

            StartCoroutine(TransitionToPhaseTwo());
        }

        if (BossUI.Instance != null)
        {
            BossUI.Instance.UpdateHealthBar(currentHealth, maxHealth, currentShield);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator TransitionToPhaseTwo()
    {
        // 1. STOP EVERYTHING
        if (phaseOneAura != null) phaseOneAura.SetActive(false);
        if (shieldAuraVFX != null) shieldAuraVFX.SetActive(false); // Hide shield if active
        currentPhase = 2;
        currentState = BossState.Spawning; // This prevents him from taking damage or attacking

        // 2. FAKE DEATH
        Debug.Log("BOSS: Faking Death...");
        if (animator) animator.SetBool("Dead", true); // Play death animation

        // 3. PLAY FAKE OUT MUSIC
        if (MusicManager.Instance != null && fakeOutMusic != null)
        {
            // Play the quiet/fakeout song
            MusicManager.Instance.RequestMusic(fakeOutMusic, 2);
        }

        // 4. WAIT FOR 17 SECONDS (The Fake Out)
        yield return new WaitForSeconds(fakeOutDuration);

        // 5. RESURRECTION / RAGE
        Debug.Log("BOSS: PHASE 2 START!");
        if (animator) animator.SetBool("Dead", false); // Stop being dead
        if (animator) animator.SetTrigger("Rage");     // Play scream/rage anim

        PlayAttachedVFX(phaseTransitionVFX, 4f);
        if (audioSource && phaseTransitionSound)
        {
            audioSource.PlayOneShot(phaseTransitionSound, 5f);
        }

        // 7. START ACTUAL PHASE 2 MUSIC
        if (MusicManager.Instance != null && phaseTwoMusic != null)
        {
            MusicManager.Instance.RequestMusic(phaseTwoMusic, 2);
        }

        if (BossUI.Instance != null)
        {
            BossUI.Instance.TriggerPhase2Visuals();
        }
        yield return new WaitForSeconds(3f);

        // 9. RESUME COMBAT
        if (phaseTwoAura != null) phaseTwoAura.SetActive(true);
        currentState = BossState.Moving;
    }
    private void Die()
    {
        if (phaseOneAura != null) phaseOneAura.SetActive(false);
        if (phaseTwoAura != null) phaseTwoAura.SetActive(false);
        currentState = BossState.Dead;
        Time.timeScale = 1f;
        PlayVFX(deathExplosionVFX, transform.position, 5f);
        if (audioSource && deathSound) audioSource.PlayOneShot(deathSound);
        // REPLACE your old music code with this:
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopRequest(2);
        }
        if (ambientMusicSource != null)
        {
            ambientMusicSource.Play();
        }
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.AddPoints(pointsOnDeath);
        if (BossUI.Instance != null)
        {
            BossUI.Instance.HideHealthBar();
        }
        roundManager?.EnemyKilled();

        if (animator) animator.SetBool("Dead", true);
        Destroy(gameObject, 8f);
    }
    private Vector3 GetClosestPointOnLineSegment(Vector3 A, Vector3 B, Vector3 P)
    {
        Vector3 AP = P - A;
        Vector3 AB = B - A;
        float magnitudeAB = AB.sqrMagnitude;
        float APAB = Vector3.Dot(AP, AB);
        float t = APAB / magnitudeAB;
        t = Mathf.Clamp(t, 0.0f, 1.0f);

        return A + AB * t;
    }
}
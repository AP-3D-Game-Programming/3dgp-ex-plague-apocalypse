using UnityEngine;

public class dubbel_barrel : MonoBehaviour
{
    [Header("Bullet")]
    public Transform muzzlePoint1;       // Waar de eerste kogel uitkomt
    public Transform muzzlePoint2;       // Waar de tweede kogel uitkomt
    public GameObject bulletPrefab;      // Je bullet prefab
    public float bulletForce = 800f;     // Snelheid van de kogels

    [Header("Effects")]
    public ParticleSystem muzzleFlash;   // Wijs hier je "muzzle_flash" ParticleSystem toe

    [Header("Ammo Settings")]
    public int magazineSize = 10;         // Capaciteit van het shotgun (10 kogels)
    public int reserveAmmo = 40;         // Reserve kogels
    public float reloadTime = 5f;        // Tijd om te herladen (5 seconden)
    public float fireRate = 0.5f;        // Tijd tussen schoten
    public int bulletsPerShot = 2;       // 2 kogels per schot

    [HideInInspector]
    public int currentAmmo;              // Kogels momenteel in magazijn

    bool isReloading = false;
    Animator animator;
    float shotFired = 0f;

    void Start()
    {
        // Begin met een vol magazijn
        currentAmmo = magazineSize;
        animator = GetComponent<Animator>();
        Debug.Log(animator);
    }

    void Update()
    {
        if (isReloading)
            return;

        // R om te herladen (indien niet vol en er is reserve)
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }

        // Hou Linkermuisknop ingedrukt voor schoten
        if (Input.GetButton("Fire1"))
        {
            // Zet animatie aan zolang we fire houden en er ammo is
            if (animator != null)
                animator.SetBool("isFiring", currentAmmo > 0);

            // Schiet volgens fireRate
            if (Time.time - shotFired >= fireRate)
            {
                Shoot();
                shotFired = Time.time;
            }
        }
        else
        {
            if (animator != null)
                animator.SetBool("isFiring", false);
        }
    }

    void Shoot()
    {
        // Geen ammo in magazijn
        if (currentAmmo <= 0)
        {
            Debug.Log("Klik... Geen kogels in magazijn! Reload met R.");
            return;
        }

        // Spawn 2 bullets uit beide muzzle points
        Transform[] muzzlePoints = { muzzlePoint1, muzzlePoint2 };
        
        for (int i = 0; i < bulletsPerShot; i++)
        {
            Transform muzzlePoint = muzzlePoints[i % muzzlePoints.Length];

            if (bulletPrefab != null && muzzlePoint != null)
            {
                // bepaal richting met licht random spreiding voor shotgun effect
                Vector3 shootDir = muzzlePoint.forward;
                float spreadAngle = 5f; // Wat spreiding tussen kogels
                shootDir = Quaternion.Euler(
                    Random.Range(-spreadAngle, spreadAngle),
                    Random.Range(-spreadAngle, spreadAngle),
                    0
                ) * shootDir;

                GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.LookRotation(shootDir));

                // Zorg dat de bullet meteen een vaste wereld-snelheid krijgt
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.interpolation = RigidbodyInterpolation.Interpolate; // smoother movement
                    rb.AddForce(shootDir * bulletForce, ForceMode.VelocityChange);
                }
                else
                {
                    Debug.LogWarning("DubbelBarrel: Bullet prefab has no Rigidbody — it won't move.");
                }

                Destroy(bullet, 5f); // verwijder na 5 seconden
            }
        }

        // Play muzzle flash at both muzzle points.
        Transform[] flashPoints = { muzzlePoint1, muzzlePoint2 };
        
        for (int i = 0; i < flashPoints.Length; i++)
        {
            Transform flashPoint = flashPoints[i];
            
            if (muzzleFlash != null && flashPoint != null)
            {
                if (muzzleFlash.gameObject.scene.IsValid())
                {
                    // Parent the scene ParticleSystem to the muzzlePoint so it aligns properly.
                    muzzleFlash.transform.SetParent(flashPoint, false);
                    muzzleFlash.transform.localPosition = Vector3.zero;
                    muzzleFlash.transform.localRotation = Quaternion.identity;
                    muzzleFlash.Play();
                }
                else
                {
                    ParticleSystem spawned = Instantiate(muzzleFlash, flashPoint);
                    spawned.transform.localPosition = Vector3.zero;
                    spawned.transform.localRotation = Quaternion.identity;
                    spawned.Play();

                    // Destroy the instantiated particle after its lifetime (duration + startLifetime)
                    var main = spawned.main;
                    float startLifetime = 0f;
                    // handle different startLifetime modes
                    if (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                        startLifetime = main.startLifetime.constantMax;
                    else
                        startLifetime = main.startLifetime.constant;

                    float lifetime = main.duration + startLifetime;
                    Destroy(spawned.gameObject, lifetime);
                }
            }
        }

        // Speel kickback animatie (naam van je animatieclip: "DubbelBarrel").
        if (animator != null)
        {
            animator.Play("DubbelBarrel", 0, 0f);
        }
        
        // 1 kogel kwijt (per schot)
        currentAmmo--;

        Debug.Log($"Ammo: {currentAmmo} / {reserveAmmo}");
        shotFired = Time.time;
    }

    void TryReload()
    {
        // Als magazijn al vol is → niets doen
        if (currentAmmo >= magazineSize)
        {
            Debug.Log("Magazijn is al vol.");
            return;
        }

        // Geen reserve kogels
        if (reserveAmmo <= 0)
        {
            Debug.Log("Geen reserve ammo!");
            return;
        }

        // Start reload
        StartCoroutine(ReloadCoroutine());
    }

    System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        // Hoeveel kogels hebben we nodig om magazijn te vullen?
        int needed = magazineSize - currentAmmo;

        // Hoeveel kunnen we effectief bijvullen?
        int toLoad = Mathf.Min(needed, reserveAmmo);

        currentAmmo += toLoad;
        reserveAmmo -= toLoad;

        Debug.Log($"Reload klaar: {currentAmmo} / {reserveAmmo}");

        isReloading = false;
    }
}

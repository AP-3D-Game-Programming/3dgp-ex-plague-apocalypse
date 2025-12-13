using UnityEngine;

public class SniperRifle : MonoBehaviour
{
    [Header("Bullet")]
    public Transform muzzlePoint;        // Waar de kogel uitkomt
    public GameObject bulletPrefab;      // Je bullet prefab
    public float bulletForce = 1500f;    // Snelheid van de kogel (sniper = snel)

    [Header("Effects")]
    public ParticleSystem muzzleFlash;   // Wijs hier je "muzzle_flash" ParticleSystem toe

    [Header("Ammo Settings")]
    public int magazineSize = 10;        // Capaciteit van de sniper (10 kogels)
    public int reserveAmmo = 30;         // Reserve kogels
    public float reloadTime = 4f;        // Tijd om te herladen (4 seconden)
    public float fireRate = 1.5f;        // Tijd tussen schoten (1.5 seconden)

    [HideInInspector]
    public int currentAmmo;              // Kogels momenteel in magazijn

    bool isReloading = false;
    Animator animator;
    float lastShotTime = 0f;

    void Start()
    {
        // Begin met een vol magazijn
        currentAmmo = magazineSize;
        animator = GetComponent<Animator>();

        // Zorg ervoor dat muzzle_flash uitgeschakeld is aan het begin
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.gameObject.SetActive(false);
            Debug.Log("Sniper muzzle flash disabled at start");
        }
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

        // Eén schot per klik (GetButtonDown = single shot)
        if (Input.GetButtonDown("Fire1"))
        {
            // Check of er genoeg tijd is verstreken sinds laatste schot
            if (Time.time - lastShotTime >= fireRate)
            {
                Shoot();
                lastShotTime = Time.time;
            }
            else
            {
                Debug.Log($"Te snel! Wacht nog {fireRate - (Time.time - lastShotTime):F1} seconden");
            }
        }
    }

    void Shoot()
    {
        // Geen ammo in magazijn
        if (currentAmmo <= 0)
        {
            Debug.Log("Geen kogels! Druk op R om te herladen.");
            return;
        }

        // Speel animatie af (indien beschikbaar)
        if (animator != null)
        {
            animator.Play("SniperShot", 0, 0f);
        }

        // Spawn één kogel
        if (bulletPrefab != null && muzzlePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 shootDirection = muzzlePoint.forward;
                rb.AddForce(shootDirection * bulletForce, ForceMode.VelocityChange);
            }
        }

        // Play muzzle flash bij muzzlePoint
        if (muzzleFlash != null && muzzlePoint != null)
        {
            if (muzzleFlash.gameObject.scene.IsValid())
            {
                muzzleFlash.gameObject.SetActive(true);
                muzzleFlash.transform.SetParent(muzzlePoint, false);
                muzzleFlash.transform.localPosition = Vector3.zero;
                muzzleFlash.transform.localRotation = Quaternion.identity;
                
                muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                muzzleFlash.Play();
                
                var main = muzzleFlash.main;
                StartCoroutine(DisableMuzzleFlashAfter(main.duration + 0.5f));
            }
        }

        // Verminder ammo
        currentAmmo--;
        Debug.Log($"Sniper fired! Ammo: {currentAmmo} / {reserveAmmo}");
    }

    void TryReload()
    {
        // Magazijn is al vol
        if (currentAmmo >= magazineSize)
        {
            Debug.Log("Magazijn is al vol!");
            return;
        }

        // Geen reserve meer
        if (reserveAmmo <= 0)
        {
            Debug.Log("Geen reserve ammo meer!");
            return;
        }

        // Start reload coroutine
        StartCoroutine(ReloadCoroutine());
    }

    System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log("Reloading sniper rifle...");

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

    System.Collections.IEnumerator DisableMuzzleFlashAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.gameObject.SetActive(false);
        }
    }
}

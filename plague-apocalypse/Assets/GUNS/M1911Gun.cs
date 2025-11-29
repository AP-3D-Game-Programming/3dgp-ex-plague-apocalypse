using UnityEngine;

public class M1911Gun : MonoBehaviour
{
    [Header("Bullet")]
    public Transform muzzlePoint;        // Waar de kogel uitkomt (voor op de loop)
    public GameObject bulletPrefab;      // Je bullet prefab
    public float bulletForce = 800f;     // Snelheid van de kogel

    [Header("Ammo Settings")]
    public int magazineSize = 7;         // Grootte van magazijn
    public int reserveAmmo = 35;         // Reserve kogels
    public float reloadTime = 1.5f;      // Tijd om te herladen

    [HideInInspector]
    public int currentAmmo;              // Kogels momenteel in magazijn

    bool isReloading = false;

    void Start()
    {
        // Begin met een vol magazijn
        currentAmmo = magazineSize;
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

        // Linkermuisknop om te schieten
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
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

        // Spawn bullet
        if (bulletPrefab != null && muzzlePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(muzzlePoint.forward * bulletForce);
            }

            Destroy(bullet, 3f); // verwijder na 3 seconden
        }

        // 1 kogel kwijt
        currentAmmo--;

        Debug.Log($"Ammo: {currentAmmo} / {reserveAmmo}");
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

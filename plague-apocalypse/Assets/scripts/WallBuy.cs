using UnityEngine;

public class WallBuy : Interactable
{
    [Header("Wall Buy Settings")]
    [SerializeField] private WeaponData weaponForSale;
    [SerializeField] private int weaponCost = 500;
    [SerializeField] private int ammoCost = 250; 

    private bool playerHasWeapon = false; 

    private void Awake()
    {
        if (weaponForSale.weaponPrefab != null)
        {
            GameObject weaponVisual = Instantiate(weaponForSale.weaponPrefab, this.transform);
            weaponVisual.transform.localPosition = Vector3.zero;
            weaponVisual.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // Verwijder scripts en physics
            foreach (MonoBehaviour script in weaponVisual.GetComponents<MonoBehaviour>()) Destroy(script);
            if (weaponVisual.GetComponent<Rigidbody>()) Destroy(weaponVisual.GetComponent<Rigidbody>());
            if (weaponVisual.GetComponent<Collider>()) Destroy(weaponVisual.GetComponent<Collider>());
        }
    }

    private void Update()
    {
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null)
        {
            playerHasWeapon = inventory.HasWeapon(weaponForSale);
            promptMessage = playerHasWeapon 
                ? $"Press E to buy Ammo for {weaponForSale.weaponName} [{ammoCost}]" 
                : $"Press E to buy {weaponForSale.weaponName} [{weaponCost}]";
        }
    }

    public override void OnInteract(PlayerInventory inventory)
    {
        if (weaponForSale == null) return;

        bool alreadyHasWeapon = inventory.HasWeapon(weaponForSale);
        int currentPrice = alreadyHasWeapon ? ammoCost : weaponCost;

        if (PlayerStats.Instance != null && PlayerStats.Instance.points >= currentPrice)
        {
            PlayerStats.Instance.RemovePoints(currentPrice);
            if (alreadyHasWeapon) inventory.RefillAmmo(weaponForSale);
            else inventory.PickupWeapon(weaponForSale);
        }
    }
}
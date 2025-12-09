using UnityEngine;

public class WallBuy : Interactable
{
    [Header("Wall Buy Settings")]
    [SerializeField] private WeaponData weaponForSale;
    [SerializeField] private int weaponCost = 500;
    [SerializeField] private int ammoCost = 250; // Goedkoper dan het wapen zelf

    private bool playerHasWeapon = false; // Om bij te houden wat we moeten tonen

    private void Awake()
    {
        // Visuele weergave van het wapen op de muur
        GameObject weaponVisual = Instantiate(weaponForSale.weaponPrefab, this.transform);
        weaponVisual.transform.localPosition = new Vector3(0f, 1.4f, 0f);
        weaponVisual.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        // Verwijder scripts van het model op de muur zodat je er niet mee kan schieten
        Destroy(weaponVisual.GetComponent<Gun>());
        Destroy(weaponVisual.GetComponent<Rigidbody>());
        Destroy(weaponVisual.GetComponent<Collider>());
    }

    private void Update()
    {
        // Dit is een simpele manier om de tekst te updaten als de speler in de buurt is.
        // In een echte game zou je dit efficiënter doen, maar dit werkt prima.
        UpdatePromptMessage();
    }

    private void UpdatePromptMessage()
    {
        // Zoek de inventory (je kan dit optimaliseren door het te cachen in OnTriggerEnter)
        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();

        if (inventory != null)
        {
            playerHasWeapon = inventory.HasWeapon(weaponForSale);

            if (playerHasWeapon)
            {
                promptMessage = $"Press E to buy Ammo for {weaponForSale.weaponName} [{ammoCost}]";
            }
            else
            {
                promptMessage = $"Press E to buy {weaponForSale.weaponName} [{weaponCost}]";
            }
        }
    }

    public override void OnInteract(PlayerInventory inventory)
    {
        // 1. Check of we het wapen al hebben
        bool alreadyHasWeapon = inventory.HasWeapon(weaponForSale);

        // 2. Bepaal de prijs
        int currentPrice = alreadyHasWeapon ? ammoCost : weaponCost;

        // 3. Check Punten (via de Singleton die je hebt gestuurd)
        if (PlayerStats.Instance.points >= currentPrice)
        {
            // 4. Betaal
            PlayerStats.Instance.RemovePoints(currentPrice);

            // 5. Geef Item
            if (alreadyHasWeapon)
            {
                inventory.RefillAmmo(weaponForSale);
                Debug.Log("Ammo Refilled!");
            }
            else
            {
                inventory.PickupWeapon(weaponForSale);
                Debug.Log($"Je kocht een {weaponForSale.weaponName}!");
            }
        }
        else
        {
            Debug.Log("Niet genoeg punten!");
        }
    }
}
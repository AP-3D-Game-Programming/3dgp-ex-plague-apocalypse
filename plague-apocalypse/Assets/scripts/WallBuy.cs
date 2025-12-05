using UnityEngine;

public class WallBuy : Interactable
{
    [Header("Wall Buy Settings")]
    [SerializeField] private WeaponData weaponForSale;
    [SerializeField] private int cost = 500;  
    private void Awake()
    {
        GameObject weaponVisual = Instantiate(weaponForSale.weaponPrefab, this.transform);
        weaponVisual.transform.localPosition = new Vector3(0f, 1.4f, 0f);
        weaponVisual.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        promptMessage = $"Press E to buy {weaponForSale.weaponName} [{cost}]";
    }
    public override void OnInteract(PlayerInventory inventory)
    {
        // Hier kun je later je punten check toevoegen
        // if (score < cost) return;

        inventory.PickupWeapon(weaponForSale);
        Debug.Log($"Je kocht een {weaponForSale.weaponName}!");
    }
}

using UnityEngine;

public class WallShop : MonoBehaviour
{
    [SerializeField] private WeaponData weaponForSale;
    [SerializeField] private GameObject promptUI;
    private void Awake()
    {
        GameObject weaponVisual = Instantiate(weaponForSale.weaponPrefab, this.transform);
        weaponVisual.transform.localPosition = new Vector3(0f, 1.4f, 0f);
    }
}

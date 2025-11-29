using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName;       // Bijv: "M1911"
    public string description;      // Bijv: "Startpistool"

    [Header("Visuals")]
    public Sprite icon;             // Plaatje voor in de UI
    public GameObject weaponPrefab; // Het 3D model

    [Header("Stats")]
    public int maxAmmo;             // Bijv: 80
    public int magazineSize;        // Bijv: 8
    public float damage;            // Bijv: 25
    public float fireRate;          // Tijd tussen schoten
}

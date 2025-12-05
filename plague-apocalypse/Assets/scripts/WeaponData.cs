using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName; 
    public bool isAutomatic;
    [Header("Visuals")]
    public GameObject weaponPrefab;
    public GameObject muzzleFlashPrefab;

    [Header("Effects")]
    public List<BulletEffect> effects;

    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Stats")]
    public WeaponType weaponType;
    public int maxAmmo;
    public int magazineSize;
    public float damage; 
    public float fireRate; 
    public float reloadTime;
}

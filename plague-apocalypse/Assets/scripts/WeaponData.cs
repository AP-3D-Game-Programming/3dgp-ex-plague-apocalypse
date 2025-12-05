using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName; 
    public GameObject weaponPrefab;

    [Header("Effects")]
    public List<BulletEffect> effects;

    [Header("Projectile")]
    public GameObject projectilePrefab; // Het kogel model

    [Header("Stats")]
    public WeaponType weaponType;
    public int maxAmmo;             // Bijv: 80
    public int magazineSize;        // Bijv: 8
    public float damage;            // Bijv: 25
    public float fireRate;          // Tijd tussen schoten
}

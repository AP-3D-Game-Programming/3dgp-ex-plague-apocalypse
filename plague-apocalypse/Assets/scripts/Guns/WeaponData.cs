using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName;
    public WeaponType weaponType;

    [Header("Visuals")]
    public GameObject weaponPrefab; // Het model dat de speler vasthoudt

    [Header("Stats")]
    public int maxAmmo;
    public int magazineSize;
    public float damage;
    public float fireRate;
    public float reloadTime;

    [Header("Projectile")]
    public GameObject projectilePrefab; // De kogel prefab

    [Header("Effects")]
    public List<BulletEffect> effects;
    [Header("UI")]
    public Sprite weaponIcon;
}
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Base Stats")]
    public float speed = 50f;
    public float lifetime = 5f;

    // Deze data wordt ingevuld door het wapen
    [HideInInspector] public float damage; 
    [HideInInspector] public WeaponType sourceWeaponType;

    private Rigidbody rb;
    public void Initialize(float weaponDamage, WeaponType type, List<BulletEffect> effects)
    {
        this.damage = weaponDamage;
        this.sourceWeaponType = type;

        if (effects != null)
        {
            foreach (var effect in effects)
            {
                effect.Apply(this.gameObject);
            }
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // (Zorg dat je UseGravity UIT hebt staan op de Rigidbody, tenzij het een granaat is)
        rb.linearVelocity = transform.forward * speed; 

        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1. Check of we een zombie raken
        // We gebruiken InParent voor het geval we een arm of been raken
        // ZombieHealth zombie = collision.gameObject.GetComponentInParent<ZombieHealth>();
        
        // if (zombie != null)
        // {
        //     zombie.TakeDamage(damage);
        // }

        // 2. Vernietig de kogel
        // (Later kun je hier code toevoegen om NIET te destroyen als je bouncy bent)
        Destroy(gameObject);
    }
}
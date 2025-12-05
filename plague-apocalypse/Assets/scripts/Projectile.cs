using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Base Stats")]
    public float speed = 50f;
    public float lifetime = 5f;

    [HideInInspector] public float damage; 
    [HideInInspector] public WeaponType sourceWeaponType;
    [HideInInspector] public Action<Collision> onHit;
    [HideInInspector] public bool destroyOnHit = true;

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
        // Basis functionaliteit: Damage to a zombie (indien aanwezig)
        // ZombieHealth zombie = collision.gameObject.GetComponentInParent<ZombieHealth>();
        
        // if (zombie != null)
        // {
        //     zombie.TakeDamage(damage);
        // }

        onHit?.Invoke(collision);
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
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
    private List<BulletLogic> activeLogics = new List<BulletLogic>();

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
        activeLogics.AddRange(GetComponents<BulletLogic>());
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

        BulletAction finalAction = BulletAction.Destroy;

        foreach (var logic in activeLogics)
        {
            finalAction = logic.OnHit(collision, finalAction);
        }

        ExecuteAction(finalAction, collision);
    }
    void ExecuteAction(BulletAction action, Collision collision)
    {
        switch (action)
        {
            case BulletAction.Destroy:
                Destroy(gameObject);
                break;

            case BulletAction.PassThrough:
                // Piercing: We doen niks, de kogel vliegt gewoon door.
                // Wel belangrijk: Zet collision uit tussen deze kogel en deze zombie
                // anders krijg je "rattle" geluiden of dubbele hits.
                Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
                break;

            case BulletAction.Bounce:
                // Bouncing: De logica heeft de richting waarschijnlijk al aangepast,
                // of we doen het hier. Voor nu laten we de physics engine stuiteren.
                break;
        }
    }
}
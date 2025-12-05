using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    public float speed = 50f;
    public float damage = 20f;
    public float lifetime = 5f; // Vernietig na 5 sec als we niks raken

    private Rigidbody rb;

    // VIRTUAL: Zodat granaten dit kunnen aanpassen (bv. zwaartekracht aan)
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Zorg dat de kogel vliegt in de richting waarin hij kijkt
        rb.linearVelocity = transform.forward * speed; 
        
        // Veiligheid: vernietig kogel na X seconden
        Destroy(gameObject, lifetime);
    }

    // VIRTUAL: Wat gebeurt er bij impact?
    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Hier kun je schade toebrengen aan wat je raakt

        // Vernietig de kogel na impact
        Destroy(gameObject);
    }
}
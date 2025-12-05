using UnityEngine;

public class ExplosiveLogic : MonoBehaviour
{
    public float radius = 5f;
    public float explosionDamage = 50f;
    public void Explode(Collision collision)
    {
        Debug.Log("BOEM! Explosie op " + collision.contacts[0].point);
        
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach(var hit in hits)
        {
            // ZombieHealth z = hit.GetComponentInParent<ZombieHealth>();
            // if(z != null) z.TakeDamage(explosionDamage);
        }
        
        // Spawn particle effect hier...
    }
}

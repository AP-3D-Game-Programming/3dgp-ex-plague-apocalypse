using UnityEngine;

public class BouncingLogic : BulletLogic
{
    public int bouncesLeft = 2;

    public override BulletAction OnHit(Collision collision, BulletAction currentAction)
    {
        bool isWall = !(collision.gameObject.layer == LayerMask.NameToLayer("Zombie") || collision.gameObject.CompareTag("Enemy"));

        if (isWall && bouncesLeft > 0)
        {
            bouncesLeft--;
            
            // Handmatig stuiteren (Vector Reflectie)
            // Dit is nodig omdat Projectiles vaak 'IsKinematic' of triggers zijn,
            // of omdat we controle willen.
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 normal = collision.contacts[0].normal;
                Vector3 reflectDir = Vector3.Reflect(rb.linearVelocity.normalized, normal);
                rb.linearVelocity = reflectDir * rb.linearVelocity.magnitude;

                transform.rotation = Quaternion.LookRotation(reflectDir);
            }

            return BulletAction.Bounce;
        }

        return currentAction;
    }
}
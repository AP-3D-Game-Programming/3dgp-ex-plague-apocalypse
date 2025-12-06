using UnityEngine;

public class PiercingLogic : BulletLogic
{
    public int pierceCount = 3;

    public override BulletAction OnHit(Collision collision, BulletAction currentAction)
    {
        bool isZombie = collision.gameObject.layer == LayerMask.NameToLayer("Zombie") || collision.gameObject.CompareTag("Enemy");

        if (isZombie)
        {
            if (pierceCount > 0)
            {
                pierceCount--;
                return BulletAction.PassThrough;
            }
        }
        return currentAction; 
    }
}   

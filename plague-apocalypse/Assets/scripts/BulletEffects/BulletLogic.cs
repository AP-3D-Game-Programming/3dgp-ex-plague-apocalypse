using UnityEngine;

public abstract class BulletLogic : MonoBehaviour
{
    public abstract BulletAction OnHit(Collision collision, BulletAction currentAction);
}

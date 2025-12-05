using UnityEngine;

[CreateAssetMenu(fileName = "ExplosiveEffect", menuName = "Scriptable Objects/ExplosiveEffect")]
public class ExplosiveEffect : BulletEffect
{
    public override void Apply(GameObject bullet)
    {
        ExplosiveLogic logic = bullet.AddComponent<ExplosiveLogic>();
        
        Projectile p = bullet.GetComponent<Projectile>();

        p.onHit += logic.Explode;
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "ExplosiveEffect", menuName = "Effects/Explosive")]
public class ExplosiveEffect : BulletEffect
{
    public override void Apply(GameObject bullet)
    {
        ExplosiveLogic logic = GetOrAddLogic<ExplosiveLogic>(bullet);

    }
}

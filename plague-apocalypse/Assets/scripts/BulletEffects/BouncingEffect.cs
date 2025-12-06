using UnityEngine;

[CreateAssetMenu(fileName = "BouncingEffect", menuName = "Effects/Bouncing")]
public class BouncingEffect : BulletEffect
{
    public int bounceCount = 3;
    public override void Apply(GameObject bullet)
    {
        BouncingLogic logic = bullet.AddComponent<BouncingLogic>();
        logic.bouncesLeft = bounceCount;
    }
}

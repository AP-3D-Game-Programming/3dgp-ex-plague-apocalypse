using UnityEngine;

[CreateAssetMenu(fileName = "PiercingEffect", menuName = "Effects/Piercing")]
public class PiercingEffect : BulletEffect
{
    public int amount = 2;

    public override void Apply(GameObject bullet)
    {
        // Voeg het component toe
        PiercingLogic logic = bullet.AddComponent<PiercingLogic>();
        
        // Stel de variabelen in
        logic.pierceCount = amount;
    }
}

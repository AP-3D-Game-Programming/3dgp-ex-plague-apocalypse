using UnityEngine;
using System.Collections.Generic;

public class PlayerEffectManager : MonoBehaviour
{
    private List<BulletEffect> collectedEffects = new List<BulletEffect>();

    // Deze functie roep je aan als je een kaart pakt (vanuit je CardSystem)
    public void AddEffect(BulletEffect newEffect)
    {
        if (newEffect != null)
        {
            collectedEffects.Add(newEffect);
            Debug.Log($"Effect toegevoegd aan speler: {newEffect.name}");
        }
    }
    public List<BulletEffect> GetActiveEffectsForWeapon(WeaponType type)
    {
        List<BulletEffect> validEffects = new List<BulletEffect>();
        foreach (var effect in collectedEffects)
        {
            // Check: Staat het type in de 'allowedTypes' van het effect?
            if (effect.allowedTypes.Contains(type))
            {
                validEffects.Add(effect);
            }
        }

        return validEffects;
    }
}
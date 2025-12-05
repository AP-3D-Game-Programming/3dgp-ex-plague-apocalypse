using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BulletEffect", menuName = "Scriptable Objects/BulletEffect")]
public abstract class BulletEffect : ScriptableObject
{

    public List<WeaponType> allowedTypes; 

    // Dit is de functie die de kogel aanpast
    // We geven het GameObject mee zodat je componenten kunt toevoegen (AddComponent)
    public abstract void Apply(GameObject bullet);
}

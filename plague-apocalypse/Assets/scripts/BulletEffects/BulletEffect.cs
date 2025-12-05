using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BulletEffect", menuName = "Scriptable Objects/BulletEffect")]
public abstract class BulletEffect : ScriptableObject
{

    public List<WeaponType> allowedTypes; 

    public abstract void Apply(GameObject bullet);
}

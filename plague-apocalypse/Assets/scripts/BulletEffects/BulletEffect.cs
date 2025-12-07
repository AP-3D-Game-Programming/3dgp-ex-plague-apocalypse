using UnityEngine;
using System.Collections.Generic;
public abstract class BulletEffect : ScriptableObject
{

    public List<WeaponType> allowedTypes; 

    public abstract void Apply(GameObject bullet);

    protected T GetOrAddLogic<T>(GameObject bullet) where T : BulletLogic
    {
        T logic = bullet.GetComponent<T>();
        if (logic == null)
        {
            logic = bullet.AddComponent<T>();
        }
        return logic;
    }
}

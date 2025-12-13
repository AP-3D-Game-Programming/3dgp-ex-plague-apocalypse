using UnityEngine;
using System.Collections;

public class csDemoSceneCode : MonoBehaviour
{
    public string[] EffectNames;
    public string[] Effect2Names;
    public Transform[] Effect;
    int i = 0;
    int a = 0;


    void Start()
    {
        Instantiate(Effect[i], new Vector3(0, 5, 0), Quaternion.identity);
    }

}

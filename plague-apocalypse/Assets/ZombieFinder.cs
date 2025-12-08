using UnityEngine;

public class ZombieFinger : MonoBehaviour
{
    public void ResetAllZombiesToZero()
    {

        ZombieDance[] allZombies = FindObjectsOfType<ZombieDance>();

        foreach (ZombieDance zombie in allZombies)
        {
            zombie.SetDance(0);
        }
    }
}
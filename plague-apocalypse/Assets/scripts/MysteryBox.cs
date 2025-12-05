using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MysteryBox : MonoBehaviour
{
    [SerializeField] private List<WeaponData> weapons;
    [SerializeField] private GameObject promptUI;
    private void Start()
    {
        HidePrompt();
    }

    public void ShowPrompt()
    {
        promptUI.SetActive(true);
    }
    public void HidePrompt()
    {
        promptUI.SetActive(false);
    }

    public WeaponData OpenChest()
    {
        int randomIndex = Random.Range(0, weapons.Count);
        WeaponData newWeapon = weapons[randomIndex];
        return newWeapon;
    }
}

using UnityEngine;

public class AmmoHUD : MonoBehaviour
{
    public TMPro.TextMeshProUGUI ammoText;
    public void UpdateAmmoDisplay(int clip, int reserve)
    {
        ammoText.text = $"{clip} / {reserve}";
    }
}

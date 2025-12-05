using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    private Gun currentGun;

    void Update()
    {
        // Zoek het wapen in je handen (als we gewisseld zijn)
        // Dit kan efficiënter, maar voor nu is GetComponentInChildren prima
        if (currentGun == null)
        {
            currentGun = GetComponentInChildren<Gun>();
        }

        // Als we een wapen hebben én klikken
        if (currentGun != null && Input.GetButton("Fire1"))
        {
            currentGun.AttemptShoot();
        }
    }
    
    // Roep dit aan vanuit PlayerInventory als je van wapen wisselt!
    public void UpdateCurrentGun()
    {
        currentGun = GetComponentInChildren<Gun>();
    }
}

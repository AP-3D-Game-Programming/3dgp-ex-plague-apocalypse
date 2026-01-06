[System.Serializable]
public class WeaponInstance
{
    public WeaponData data;
    public int currentClip;
    public int currentReserve;

    public WeaponInstance(WeaponData data)
    {
        this.data = data;
        this.currentClip = data.magazineSize;
        this.currentReserve = data.maxAmmo;
    }

    public void RefillFull()
    {
        currentClip = data.magazineSize;
        currentReserve = data.maxAmmo;
    }
}
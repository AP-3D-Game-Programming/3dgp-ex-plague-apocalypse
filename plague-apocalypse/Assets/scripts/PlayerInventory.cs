using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private List<WeaponData> startingWeaponsData = new List<WeaponData>(); 
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private AmmoHUD ammoHUD;
    
    private List<WeaponInstance> weapons = new List<WeaponInstance>(); 
    private int currentWeaponIndex = 0;
    private int maxWeapons = 2;
    private GameObject currentWeaponModel;
    private PlayerEffectManager effectManager;

    private void Awake()
    {
        effectManager = GetComponent<PlayerEffectManager>();
    }

    private void Start()
    {
        foreach(var data in startingWeaponsData)
        {
            PickupWeapon(data); 
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            int newIndex = currentWeaponIndex == 0 ? 1 : 0;
            EquipWeapon(newIndex);
        }
    }

    public void PickupWeapon(WeaponData newWeaponData)
    {
        WeaponInstance existing = weapons.Find(x => x.data == newWeaponData);
        
        if (existing != null)
        {
            RefillAmmo(newWeaponData);
            return;
        }

        WeaponInstance newInstance = new WeaponInstance(newWeaponData);

        if (weapons.Count < maxWeapons)
        {
            weapons.Add(newInstance);
            EquipWeapon(weapons.Count - 1);
        }
        else
        {
            weapons[currentWeaponIndex] = newInstance;
            EquipWeapon(currentWeaponIndex);
        }
    }

    void EquipWeapon(int index)
    {
        if (index >= weapons.Count) return;
        if (currentWeaponModel != null) Destroy(currentWeaponModel);

        WeaponInstance instanceToEquip = weapons[index];

        if (instanceToEquip.data.weaponPrefab != null)
        {
            currentWeaponModel = Instantiate(instanceToEquip.data.weaponPrefab, weaponHolder);
        }
        else
        {
            Debug.LogError($"FOUT: Wapen {instanceToEquip.data.weaponName} heeft geen prefab!");
            return;
        }

        WeaponController ctrl = currentWeaponModel.GetComponent<WeaponController>();
        if (ctrl != null)
        {
            ctrl.Initialize(instanceToEquip, effectManager);
            ctrl.onAmmoChanged = null; 
            ctrl.onAmmoChanged += ammoHUD.UpdateAmmoDisplay;
            ammoHUD.UpdateAmmoDisplay(instanceToEquip.currentClip, instanceToEquip.currentReserve);
        }
        
        currentWeaponModel.transform.localPosition = Vector3.zero; 
        currentWeaponModel.transform.localRotation = Quaternion.identity;
        currentWeaponIndex = index;
    }

    public WeaponData GetCurrentWeapon()
    {
        if (weapons.Count == 0) return null;
        if (currentWeaponIndex < weapons.Count) return weapons[currentWeaponIndex].data;
        return null;
    }

    public bool HasWeapon(WeaponData weaponToCheck)
    {
        return weapons.Exists(x => x.data == weaponToCheck);
    }

    public void RefillAmmo(WeaponData weaponToRefill)
    {
        WeaponInstance instance = weapons.Find(x => x.data == weaponToRefill);
        if(instance != null)
        {
            instance.RefillFull();
            
            if(GetCurrentWeapon() == weaponToRefill && currentWeaponModel != null)
            {
                WeaponController ctrl = currentWeaponModel.GetComponent<WeaponController>();
                if (ctrl != null) ctrl.onAmmoChanged?.Invoke(instance.currentClip, instance.currentReserve);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.Events;
using TMPro; // 1. Vergeet deze niet!

public class PlayerInteract : MonoBehaviour
{
    [Header("Setup")]
    public Camera playerCamera;
    public TextMeshProUGUI promptText;
    public UnityEvent onInteract;
    private PlayerInventory inventory;
    private MysteryBox currentChest;
    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {

        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            AttemptInteract();
        }
    }

    void CheckForInteractable()
    {
        RaycastHit hit;

        if(Physics.SphereCast(playerCamera.transform.position, 0.5f, playerCamera.transform.forward, out hit, 3f))
        {
            MysteryBox newChest = hit.collider.GetComponent<MysteryBox>();
            
            if(newChest != null)
            {
                if(currentChest != newChest)
                {
                    if( currentChest != null)
                    {
                        currentChest.HidePrompt();
                    }
                    currentChest = newChest;
                    currentChest.ShowPrompt();
                }
                return;
            }

        }
        if(currentChest != null)
        {
            currentChest.HidePrompt();
            currentChest = null;
        }
    }

    public void AttemptInteract()
    {
        if(currentChest != null)
        {
            WeaponData newWeapon = currentChest.OpenChest();
            inventory.PickupWeapon(newWeapon);
            onInteract.Invoke();
        }
    }
}
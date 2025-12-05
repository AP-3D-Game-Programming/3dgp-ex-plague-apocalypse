using UnityEngine;
using UnityEngine.Events;
using TMPro; // 1. Vergeet deze niet!

public class PlayerInteract : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Camera playerCamera;
    private PlayerInventory inventory;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float interactRange = 3f;
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
        MysteryBox targetChest = null;

        if(Physics.SphereCast(playerCamera.transform.position, 0.5f,
            playerCamera.transform.forward, out hit, interactRange, interactableLayer))
        {
            targetChest = hit.collider.GetComponent<MysteryBox>();
        }

        if (targetChest != currentChest)
        {
            if(currentChest != null)
            {
                currentChest.HidePrompt();
            }

            if(targetChest != null)
            {
                targetChest.ShowPrompt();
            }
            currentChest = targetChest;
        }
    }

    public void AttemptInteract()
    {
        if(currentChest != null)
        {
            WeaponData newWeapon = currentChest.OpenChest();
            inventory.PickupWeapon(newWeapon);
        }
    }
}
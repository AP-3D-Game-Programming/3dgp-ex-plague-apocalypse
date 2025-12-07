using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask interactLayers;
    private PlayerInventory inventory;
    private Interactable currentInteractable; 

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if(currentInteractable != null)
            {
                currentInteractable.OnInteract(inventory);
            }
        }
    }

    void CheckForInteractable()
    {
        RaycastHit hit;
        Interactable newInteractable = null;

        if(Physics.SphereCast(playerCamera.transform.position, 0.5f, playerCamera.transform.forward, out hit, 3f, interactLayers))
        {
            newInteractable = hit.collider.GetComponent<Interactable>();
            Debug.Log("Ik raak object: " + hit.collider.gameObject.name);
        }

        if (newInteractable != currentInteractable)
        {
            if (currentInteractable != null) currentInteractable.HidePrompt();
            if (newInteractable != null) newInteractable.ShowPrompt();
            
            currentInteractable = newInteractable;
        }
    }
}
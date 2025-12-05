using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Base Settings")]
    public string promptMessage = "Press E to Interact";
    public GameObject promptUI;

    public void ShowPrompt()
    {
        if (promptUI != null) promptUI.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    public virtual void OnInteract(PlayerInventory inventory)
    {
        Debug.Log("Basis interactie - Dit moet overschreven worden!");
    }
}

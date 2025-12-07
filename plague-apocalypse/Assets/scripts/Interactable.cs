using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Base Settings")]
    public string promptMessage = "Press E to Interact";
    public GameObject promptUI;

    private void Awake()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

    }

    public void ShowPrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
            promptUI.GetComponent<TextMeshPro>().text = promptMessage;
        }
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

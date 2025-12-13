using UnityEngine;

public class FINALLBOSSSUMMONER : MonoBehaviour
{
    [Header("Summoning Settings")]
    public GameObject bossPrefab;
    public KeyCode interactionKey = KeyCode.E;

    [Header("UI/Visuals")]
    public GameObject promptUI; // Optional: Drag a UI canvas element here

    private bool playerInRange = false;

    void Start()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            SummonBoss();
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
        }
    }

    void SummonBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Boss Prefab is not assigned to the BossSummoner script.");
            return;
        }


        Vector3 spawnPosition = transform.position + Vector3.up * 1f;
        GameObject bossInstance = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);

        FINALLBOSS bossScript = bossInstance.GetComponent<FINALLBOSS>();
        if (bossScript != null)
        {
        }


        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

        Destroy(gameObject);
    }
}
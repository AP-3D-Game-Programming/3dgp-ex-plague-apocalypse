using UnityEngine;
using System.Collections;

public class FINALLBOSSSUMMONER : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueEntry
    {
        public GameObject textObject;
        public float duration;
    }

    [Header("Summoning Settings")]
    public GameObject bossPrefab;
    public KeyCode interactionKey = KeyCode.E;
    private bool hasTriggered = false;

    [Header("Music Settings")]
    public AudioClip bossMusic; // ASSIGN YOUR MUSIC CLIP HERE

    [Header("Dialogue Settings")]
    public DialogueEntry[] dialogueLines;
    private bool dialogueIsFinished = false;

    [Header("Locations")]
    public Transform cutsceneBossObject;
    public Transform teleportTarget;
    public Transform realBossSpawnPoint;

    [Header("Cameras")]
    public GameObject camera1;
    public GameObject camera2;

    [Header("VFX")]
    public GameObject teleportVFX;
    public GameObject summonVFX;

    [Header("UI/Visuals")]
    public GameObject promptUI;
    private bool playerInRange = false;

    void Start()
    {
        if (promptUI != null) promptUI.SetActive(false);

        if (camera1 != null) camera1.SetActive(false);
        if (camera2 != null) camera2.SetActive(false);
        if (cutsceneBossObject != null) cutsceneBossObject.gameObject.SetActive(false);

        foreach (DialogueEntry line in dialogueLines)
        {
            if (line.textObject != null)
                line.textObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey) && !hasTriggered)
        {
            StartCoroutine(SummonSequence());
        }
    }

    IEnumerator SummonSequence()
    {
        hasTriggered = true;
        if (promptUI != null) promptUI.SetActive(false);
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopRequest(1);
        }
        if (MusicManager.Instance != null && bossMusic != null)
        {
            MusicManager.Instance.ForceMusic(bossMusic, 2);
        }

        // 1. FREEZE THE GAME
        Time.timeScale = 0f;

        // 2. Setup
        if (cutsceneBossObject != null) cutsceneBossObject.gameObject.SetActive(true);
        if (camera1 != null) camera1.SetActive(true);

        // 3. START DIALOGUE
        StartCoroutine(DialogueRoutine());

        yield return new WaitForSecondsRealtime(3.5f);

        // --- TELEPORT ---
        if (cutsceneBossObject != null && teleportTarget != null)
        {
            // Move Boss
            cutsceneBossObject.position = teleportTarget.position;
            cutsceneBossObject.rotation = teleportTarget.rotation;
            if (teleportVFX != null)
            {
                Vector3 vfxPosition = cutsceneBossObject.position + (Vector3.up * 2.0f);
                GameObject currentVFX = Instantiate(teleportVFX, vfxPosition, Quaternion.identity);
                StartCoroutine(DestroyVFXRealtime(currentVFX, 2f));
            }
        }

        yield return new WaitForSecondsRealtime(3f);

        // --- CAMERA SWITCH ---
        if (camera1 != null) camera1.SetActive(false);
        if (camera2 != null) camera2.SetActive(true);

        yield return new WaitForSecondsRealtime(18.0f);

        // --- WAIT FOR DIALOGUE TO FINISH ---
        while (!dialogueIsFinished)
        {
            yield return null;
        }

        // 5. UNFREEZE & CLEANUP
        Time.timeScale = 1f;

        if (camera2 != null) camera2.SetActive(false);
        if (cutsceneBossObject != null) cutsceneBossObject.gameObject.SetActive(false);

        SpawnBossLogic();
    }

    IEnumerator DestroyVFXRealtime(GameObject obj, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (obj != null) Destroy(obj);
    }

    IEnumerator DialogueRoutine()
    {
        dialogueIsFinished = false;

        foreach (DialogueEntry line in dialogueLines)
        {
            if (line.textObject != null)
            {
                line.textObject.SetActive(true);
                yield return new WaitForSecondsRealtime(line.duration);
                line.textObject.SetActive(false);
            }
        }

        dialogueIsFinished = true;
    }

    void SpawnBossLogic()
    {
        if (bossPrefab == null) return;

        Vector3 finalSpawnPos = transform.position;
        Quaternion finalSpawnRot = Quaternion.identity;

        if (realBossSpawnPoint != null)
        {
            finalSpawnPos = realBossSpawnPoint.position;
            finalSpawnRot = realBossSpawnPoint.rotation;
        }

        if (summonVFX != null)
        {
            GameObject vfx = Instantiate(summonVFX, finalSpawnPos, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        Instantiate(bossPrefab, finalSpawnPos, finalSpawnRot);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            playerInRange = true;
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }
}
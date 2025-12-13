using UnityEngine;

public class AreaMusicTrigger : MonoBehaviour
{
    [Header("Music Settings")]
    public AudioClip areaMusic; // Drag your area music here
    public int priorityLevel = 1; // Default to 1 (MiniBoss/Area level)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (MusicManager.Instance != null && areaMusic != null)
            {
                // Tell the manager to play this music at Priority 1
                MusicManager.Instance.RequestMusic(areaMusic, priorityLevel);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (MusicManager.Instance != null)
            {
                // Tell the manager we are leaving, so stop requesting Priority 1
                MusicManager.Instance.StopRequest(priorityLevel);
            }
        }
    }
}
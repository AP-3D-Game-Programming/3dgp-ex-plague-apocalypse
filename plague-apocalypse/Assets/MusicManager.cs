using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Source")]
    public AudioSource musicSource;
    public float fadeDuration = 1.0f;
    public float maxVolume = 0.3f;
    // The "Playlist" slots
    // 0 = Ambient
    // 1 = MiniBoss
    // 2 = FinalBoss
    private AudioClip[] activeRequests = new AudioClip[3];

    // Tracks HOW MANY enemies are currently requesting each priority level
    private int[] priorityCounts = new int[3];

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RequestMusic(AudioClip clip, int priority)
    {
        if (priority < 0 || priority >= activeRequests.Length) return;

        // 1. Assign the clip
        activeRequests[priority] = clip;

        // 2. Increment the counter
        priorityCounts[priority]++;
        // This ensures that if a Boss (2) appears, any existing MiniBoss (1) 
        // requests are forgotten. 
        // WARNING: If the Boss dies while a MiniBoss is alive, music will go to Ambient.
        for (int i = 0; i < priority; i++)
        {
            activeRequests[i] = null;
            priorityCounts[i] = 0;
        }
        // ----------------------------------

        EvaluateMusic();
    }
    public void StopRequest(int priority)
    {
        if (priority < 0 || priority >= activeRequests.Length) return;

        // 1. Decrement the counter
        priorityCounts[priority]--;

        // 2. Only remove the music if the counter hits ZERO
        // If it's > 0, it means another boss is still alive and wants this music.
        if (priorityCounts[priority] <= 0)
        {
            priorityCounts[priority] = 0; // Safety clamp
            activeRequests[priority] = null;
        }

        EvaluateMusic();
    }

    private void EvaluateMusic()
    {
        AudioClip clipToPlay = null;

        // Check Highest Priority First
        if (activeRequests[2] != null)
        {
            clipToPlay = activeRequests[2];
        }
        // Check Mini Boss
        else if (activeRequests[1] != null)
        {
            clipToPlay = activeRequests[1];
        }
        // Check Ambient
        else if (activeRequests[0] != null)
        {
            clipToPlay = activeRequests[0];
        }

        // Only switch if the song is actually different
        if (musicSource.clip != clipToPlay)
        {
            StopAllCoroutines();
            StartCoroutine(CrossfadeMusic(clipToPlay));
        }
    }

    IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        float startVolume = maxVolume; // I changed 0.3f to use your public maxVolume variable so it's adjustable

        // Fade Out
        if (musicSource.isPlaying)
        {
            startVolume = musicSource.volume;
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;
                yield return null;
            }
            musicSource.Stop();
        }

        // Swap Clip
        musicSource.clip = newClip;

        // Fade In
        if (newClip != null)
        {
            // --- ADD THIS LINE ---
            musicSource.loop = true;
            // ---------------------

            musicSource.Play();
            while (musicSource.volume < maxVolume)
            {
                musicSource.volume += maxVolume * Time.deltaTime / fadeDuration;
                yield return null;
            }
        }
        musicSource.volume = maxVolume;
    }
}
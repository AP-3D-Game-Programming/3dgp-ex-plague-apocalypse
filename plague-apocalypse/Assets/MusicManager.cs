using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Source")]
    public AudioSource musicSource;
    public float fadeDuration = 1.0f;
    public float maxVolume = 0.3f;

    // INCREASED SIZE to 11 to allow index 10
    // 0 = Ambient
    // 1 = MiniBoss
    // 2 = FinalBoss
    // 10 = Game Over (Highest)
    private AudioClip[] activeRequests = new AudioClip[11];
    private int[] priorityCounts = new int[11];

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RequestMusic(AudioClip clip, int priority)
    {
        if (priority < 0 || priority >= activeRequests.Length) return;

        activeRequests[priority] = clip;
        priorityCounts[priority]++;

        // Clean up lower priorities if a major event starts (optional but keeps it clean)
        // If Game Over (10) starts, we don't care about anything else.
        if (priority == 10)
        {
            for (int i = 0; i < 10; i++)
            {
                activeRequests[i] = null;
                priorityCounts[i] = 0;
            }
        }

        EvaluateMusic();
    }

    public void StopRequest(int priority)
    {
        if (priority < 0 || priority >= activeRequests.Length) return;

        priorityCounts[priority]--;

        if (priorityCounts[priority] <= 0)
        {
            priorityCounts[priority] = 0;
            activeRequests[priority] = null;
        }

        EvaluateMusic();
    }

    private void EvaluateMusic()
    {
        AudioClip clipToPlay = null;

        // 1. CHECK PRIORITY 10 (GAME OVER) FIRST
        if (activeRequests[10] != null)
        {
            clipToPlay = activeRequests[10];
        }
        // 2. Final Boss
        else if (activeRequests[2] != null)
        {
            clipToPlay = activeRequests[2];
        }
        // 3. Mini Boss
        else if (activeRequests[1] != null)
        {
            clipToPlay = activeRequests[1];
        }
        // 4. Ambient
        else if (activeRequests[0] != null)
        {
            clipToPlay = activeRequests[0];
        }

        if (musicSource.clip != clipToPlay)
        {
            StopAllCoroutines();
            StartCoroutine(CrossfadeMusic(clipToPlay));
        }
    }

    IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        float startVolume = maxVolume;

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

        musicSource.clip = newClip;

        if (newClip != null)
        {
            musicSource.loop = true; // Ensure looping
            musicSource.Play();
            while (musicSource.volume < maxVolume)
            {
                musicSource.volume += maxVolume * Time.deltaTime / fadeDuration;
                yield return null;
            }
        }
        musicSource.volume = maxVolume;
    }
    public void ForceMusic(AudioClip clip, int priority)
    {
        // 1. Clear all lower priorities (optional, use if you want to 'forget' previous music)
        for (int i = 0; i < activeRequests.Length; i++)
        {
            activeRequests[i] = null;
            priorityCounts[i] = 0;
        }

        // 2. Stop any current fades and cut immediately (optional)
        StopAllCoroutines();
        musicSource.volume = maxVolume; // Reset volume if it was fading

        RequestMusic(clip, priority);
    }
}
using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Source")]
    public AudioSource musicSource;
    public float fadeDuration = 1.0f;
    public float maxVolume = 1.0f; // This is your standard default volume

    private AudioClip[] activeRequests = new AudioClip[11];
    private int[] priorityCounts = new int[11];
    private float[] targetVolumes = new float[11];
    private float currentTargetVolume;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initialize all priorities to use your standard maxVolume
        for (int i = 0; i < targetVolumes.Length; i++) targetVolumes[i] = maxVolume;
    }

    // By adding "= -1f", the volume parameter becomes OPTIONAL. 
    // If you don't provide it, it uses your default maxVolume.
    public void RequestMusic(AudioClip clip, int priority, float customVolume = -1f)
    {
        if (priority < 0 || priority >= activeRequests.Length) return;

        activeRequests[priority] = clip;
        priorityCounts[priority]++;

        // If no custom volume was provided, use the default maxVolume
        targetVolumes[priority] = (customVolume <= 0) ? maxVolume : customVolume;

        if (priority == 10)
        {
            for (int i = 0; i < 10; i++)
            {
                activeRequests[i] = null;
                priorityCounts[i] = 0;
                targetVolumes[i] = maxVolume;
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
            targetVolumes[priority] = maxVolume; // Reset back to default
        }

        EvaluateMusic();
    }

    private void EvaluateMusic()
    {
        AudioClip clipToPlay = null;
        float volumeToSet = maxVolume;

        // Loop backwards to find the highest active priority
        for (int i = 10; i >= 0; i--)
        {
            if (activeRequests[i] != null)
            {
                clipToPlay = activeRequests[i];
                volumeToSet = targetVolumes[i];
                break;
            }
        }

        // Change music if the clip is different OR if the volume target has changed
        if (musicSource.clip != clipToPlay || currentTargetVolume != volumeToSet)
        {
            currentTargetVolume = volumeToSet;
            StopAllCoroutines();
            StartCoroutine(CrossfadeMusic(clipToPlay, volumeToSet));
        }
    }

    IEnumerator CrossfadeMusic(AudioClip newClip, float targetVolume)
    {
        if (musicSource.isPlaying)
        {
            float startVol = musicSource.volume;
            while (musicSource.volume > 0)
            {
                // Use unscaledDeltaTime so music keeps fading if the game pauses
                musicSource.volume -= startVol * Time.unscaledDeltaTime / fadeDuration;
                yield return null;
            }
            musicSource.Stop();
        }

        musicSource.clip = newClip;

        if (newClip != null)
        {
            musicSource.loop = true;
            musicSource.Play();
            while (musicSource.volume < targetVolume)
            {
                musicSource.volume += targetVolume * Time.unscaledDeltaTime / fadeDuration;
                yield return null;
            }
        }
        musicSource.volume = targetVolume;
    }
}
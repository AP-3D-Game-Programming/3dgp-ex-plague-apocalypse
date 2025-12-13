using UnityEngine;

public class RandomSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] sounds;
    [Header("Variety Settings")]
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    void Start()
    {
        if (sounds.Length == 0) return;


        AudioClip randomClip = sounds[Random.Range(0, sounds.Length)];

        if (audioSource != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(randomClip);
        }
    }
}
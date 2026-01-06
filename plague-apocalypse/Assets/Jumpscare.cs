using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // Required for Video Player
using System.Collections;

public class JumpscareManager : MonoBehaviour
{
    public RawImage displayImage;
    public VideoPlayer videoPlayer;
    public AudioSource scareAudio;

    private bool isRunning = false;

    void Start()
    {
        displayImage.enabled = false;
        StartCoroutine(JumpscareRoutine());
    }

    IEnumerator JumpscareRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (PauseMenu.isPaused || isRunning) continue;

            if (PlayerPrefs.GetInt("JumpscaresEnabled", 1) == 0) continue;

            if (Random.Range(1, 10001) == 1)
            {
                StartCoroutine(TriggerScare());
            }
        }
    }

    IEnumerator TriggerScare()
    {
        isRunning = true;

        // Prepare and Play
        displayImage.enabled = true;
        videoPlayer.Play();
        if (scareAudio != null)
        {
            scareAudio.volume = 2f;
            scareAudio.Play();
        }

        // Wait for the video duration
        yield return new WaitForSeconds((float)videoPlayer.length);

        // Reset
        videoPlayer.Stop();
        displayImage.enabled = false;
        isRunning = false;
    }
}
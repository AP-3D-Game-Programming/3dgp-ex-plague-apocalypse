using UnityEngine;

public class ZombieDance : MonoBehaviour
{
    private Animator animator;
    public int danceCount = 10;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        PlayRandomDance();
    }

    public void PlayRandomDance()
    {
        if (!animator) return;

        int randomIndex = Random.Range(0, danceCount);
        animator.SetFloat("DanceIndex", (float)randomIndex);
        animator.Play("ZombieDance", 0, 0f);
    }
}

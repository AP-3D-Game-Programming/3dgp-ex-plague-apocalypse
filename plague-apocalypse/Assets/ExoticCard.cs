using UnityEngine;
using UnityEngine.UI;

public class ExoticHoloGradient : MonoBehaviour
{
    public Image image;
    public Color[] colors;
    public float speed = 0.5f;

    void Awake()
    {
        if (image == null)
            image = GetComponent<Image>();

        // Default colors if none set
        if (colors == null || colors.Length == 0)
            colors = new Color[] { Color.magenta, Color.cyan, Color.blue };
    }

    void Update()
    {
        if (image == null || colors.Length == 0) return;

        // 1. Use UNSCALED time so it works while paused
        float time = Time.unscaledTime * speed;

        // 2. Smoothly cycle through indices
        // If we have 3 colors, we want to slide from 0->1, 1->2, 2->0
        float t = Mathf.Repeat(time, colors.Length);

        int currentIndex = Mathf.FloorToInt(t);
        int nextIndex = (currentIndex + 1) % colors.Length;

        // Get the fractional part (0.0 to 1.0) between the two indices
        float blend = t - currentIndex;

        image.color = Color.Lerp(colors[currentIndex], colors[nextIndex], blend);
    }
}
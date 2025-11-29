using UnityEngine;
using UnityEngine.UI;

public class RainbowCard : MonoBehaviour
{
    public Image image;
    [Range(0.1f, 5f)]
    public float speed = 5f;

    [Header("Color")]
    [Range(0f, 1f)]
    public float saturation = 0.7f;
    [Range(0f, 1f)]
    public float brightness = 1.0f;

    void Awake()
    {
        if (image == null) image = GetComponent<Image>();
    }

    void Update()
    {
        if (image == null) return;

        float t = Mathf.Repeat(Time.unscaledTime * speed, 1f);

        Color c = Color.HSVToRGB(t, saturation, brightness);

        c.a = image.color.a;
        image.color = c;
    }
}
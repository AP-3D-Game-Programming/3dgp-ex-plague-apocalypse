using UnityEngine;
using System.Collections;
using TMPro;

public class DramaticTXT : MonoBehaviour
{
    [Header("Settings")]
    public float typeSpeed = 0.05f;
    public bool shakeEffect = false;
    public float shakeAmount = 2f;

    private TMP_Text textComponent;
    private string originalText;
    private Vector3 originalPos;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent != null)
        {
            originalText = textComponent.text;
        }
        originalPos = transform.localPosition;
    }

    void OnEnable()
    {
        if (textComponent != null)
        {
            textComponent.text = "";
            StartCoroutine(TypewriterRoutine());
        }
    }

    IEnumerator TypewriterRoutine()
    {
        foreach (char letter in originalText.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }
    }

    void Update()
    {
        if (shakeEffect)
        {
            float x = Random.Range(-shakeAmount, shakeAmount);
            float y = Random.Range(-shakeAmount, shakeAmount);
            transform.localPosition = originalPos + new Vector3(x, y, 0);
        }
    }

    void OnDisable()
    {
        transform.localPosition = originalPos;
    }

    // --- ADDED THIS METHOD TO FIX THE ERROR IN BOSSUI ---
    public void SetShakeIntensity(float intensity)
    {
        if (intensity > 0f)
        {
            shakeEffect = true;
            shakeAmount = intensity;
        }
        else
        {
            shakeEffect = false;
            transform.localPosition = originalPos; // Reset position immediately
        }
    }
}
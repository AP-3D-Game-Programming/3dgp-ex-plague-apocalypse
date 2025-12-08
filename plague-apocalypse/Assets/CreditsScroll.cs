using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroll : MonoBehaviour
{
    [Header("Settings")]
    public float scrollSpeed = 50f;
    public float endYPosition = 2500f;
    public string menuSceneName = "MAINMENU";

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            EndCredits();
        }

        if (rectTransform.anchoredPosition.y > endYPosition)
        {
            EndCredits();
        }
    }

    public void EndCredits()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
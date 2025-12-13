using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class Gameoverscript : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI roundsText;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI scoreText;
    [Header("Scene Settings")]
    public string mainMenuSceneName = "MAINMENU";
    public float fadeDuration = 1.5f;

    public void Setup(int roundsSurvived, int totalKills, int totalScore)
    {
        GameObject[] hudObjects = GameObject.FindGameObjectsWithTag("HUD");
        foreach (GameObject obj in hudObjects)
        {
            obj.SetActive(false);
        }
        panel.SetActive(true);
        canvasGroup.alpha = 0f;

        roundsText.text = "Rounds Survived: " + roundsSurvived;
        killsText.text = "Zombies Killed: " + totalKills;
        scoreText.text = "Total Points: " + totalScore;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(FadeIn());
    }
    void Start()
    {

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }



    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
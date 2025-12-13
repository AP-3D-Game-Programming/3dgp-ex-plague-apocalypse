using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuScript : MonoBehaviour
{
    [Header("Scene Selection")]
    public string gameSceneName = "GameScene";
    public string creditsSceneName = "CreditsScene";

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;

    [Header("Settings UI")]
    public Slider volumeSlider;
    public TMP_Dropdown qualityDropdown;
    public Toggle motionBlurToggle;
    public Slider sensitivitySlider;
    public TMP_Text sensitivityValueText;
    [Header("Lighting")]
    public Light mainDirectionalLight;
    public Color[] difficultyColors;
    [Header("Difficulty UI")]
    public Image mainDifficultyIcon;

    public TMP_Text mainDifficultyText;
    public GameObject difficultyPopup;

    public Sprite[] difficultyIcons;

    // TYPE YOUR NAMES HERE (Easy, Normal, Hard) IN THE INSPECTOR
    public string[] difficultyNames;

    private const string DifficultyKey = "Difficulty";
    private const string VolumePrefKey = "MasterVolume";
    private const string QualityPrefKey = "QualitySetting";
    private const string MotionBlurKey = "MotionBlur";
    private const string SensitivityKey = "MouseSensitivity";
    void Start()
    {
        if (difficultyPopup != null) difficultyPopup.SetActive(false);

        Time.timeScale = 1f;
        LoadSettings();

        // This updates the text immediately when the game starts
        UpdateDifficultyIcon();
        ReturnToMain();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        SceneManager.LoadScene(creditsSceneName);
    }

    public void ReturnToMain()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt(QualityPrefKey, qualityIndex);
    }

    public void SetMotionBlur(bool isEnabled)
    {
        PlayerPrefs.SetInt(MotionBlurKey, isEnabled ? 1 : 0);
    }
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat(SensitivityKey, sensitivity);

        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = sensitivity.ToString("F1");
        }
    }

    private void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        AudioListener.volume = savedVolume;

        int defaultIndex = 3;
        int savedQuality = PlayerPrefs.GetInt(QualityPrefKey, defaultIndex);
        if (qualityDropdown != null)
        {
            qualityDropdown.value = savedQuality;
            qualityDropdown.RefreshShownValue();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
        QualitySettings.SetQualityLevel(savedQuality);

        int defaultBlur = 1;
        int savedBlur = PlayerPrefs.GetInt(MotionBlurKey, defaultBlur);
        if (motionBlurToggle != null)
        {
            motionBlurToggle.isOn = (savedBlur == 1);
            motionBlurToggle.onValueChanged.AddListener(SetMotionBlur);
        }
        float defaultSens = 2.0f;
        float savedSens = PlayerPrefs.GetFloat(SensitivityKey, defaultSens);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSens;
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);


            if (sensitivityValueText != null)
            {
                sensitivityValueText.text = savedSens.ToString("F1");
            }
        }
    }

    public void ToggleDifficultyMenu()
    {
        bool isActive = difficultyPopup.activeSelf;
        difficultyPopup.SetActive(!isActive);
    }

    public void SelectDifficulty(int index)
    {
        PlayerPrefs.SetInt(DifficultyKey, index);
        PlayerPrefs.Save();
        UpdateDifficultyIcon();
        difficultyPopup.SetActive(false);
    }

    private void UpdateDifficultyIcon()
    {
        int currentDiff = PlayerPrefs.GetInt(DifficultyKey, 1);
        Color targetColor = Color.white;
        if (currentDiff >= 0 && currentDiff < difficultyColors.Length)
        {
            targetColor = difficultyColors[currentDiff];
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.color = targetColor;
            }
        }

        if (mainDifficultyIcon != null && currentDiff >= 0 && currentDiff < difficultyIcons.Length)
        {
            mainDifficultyIcon.sprite = difficultyIcons[currentDiff];
        }


        if (mainDifficultyText != null && currentDiff >= 0 && currentDiff < difficultyNames.Length)
        {
            mainDifficultyText.text = difficultyNames[currentDiff];

            mainDifficultyText.color = targetColor;
        }
    }
}
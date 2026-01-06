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
    private const string JumpscareKey = "JumpscaresEnabled";
    [Header("Lighting")]
    public Light mainDirectionalLight;
    public Color[] difficultyColors;
    [Header("Audio")]
    public AudioSource menuMusicSource;
    public AudioClip menuMusicClip;
    public AudioClip[] difficultySelectSounds;
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
    public Toggle jumpscareToggle;
    void Start()
    {
        if (difficultyPopup != null) difficultyPopup.SetActive(false);

        Time.timeScale = 1f;
        LoadSettings();
        if (menuMusicSource != null && menuMusicClip != null)
        {
            menuMusicSource.clip = menuMusicClip;
            menuMusicSource.loop = true;
            if (!menuMusicSource.isPlaying)
            {
                menuMusicSource.Play();
            }
        }
        // This updates the text immediately when the game starts
        UpdateDifficultyIcon();
        ReturnToMain();
    }
    public void SetJumpscares(bool isEnabled)
    {
        PlayerPrefs.SetInt(JumpscareKey, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
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
        int defaultJmp = 1;
        int savedJmp = PlayerPrefs.GetInt(JumpscareKey, defaultJmp);
        if (jumpscareToggle != null)
        {
            jumpscareToggle.isOn = (savedJmp == 1);
            jumpscareToggle.onValueChanged.RemoveAllListeners(); // Clean up first
            jumpscareToggle.onValueChanged.AddListener(SetJumpscares);
        }
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        AudioListener.volume = savedVolume;

        int defaultIndex = 0;
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
        if (menuMusicSource != null && difficultySelectSounds != null && index >= 0 && index < difficultySelectSounds.Length)
        {
            AudioClip clipToPlay = difficultySelectSounds[index];
            if (clipToPlay != null)
            {
                menuMusicSource.PlayOneShot(clipToPlay);
            }
        }
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
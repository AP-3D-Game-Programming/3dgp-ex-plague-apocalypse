using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering; // For Motion Blur (Volume)

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseRoot;
    public GameObject mainPanel;
    public GameObject settingsPanel;
    private const string JumpscareKey = "JumpscaresEnabled";

    [Header("Jumpscare Settings")]
    public Toggle jumpscareToggle;
    [Header("Settings UI")]
    public Slider volumeSlider;
    public TMP_Dropdown qualityDropdown;
    public Toggle motionBlurToggle;
    public Slider sensitivitySlider;
    public TMP_Text sensitivityValueText;
    [Header("In-Game Stats UI")]
    public TMP_Text inGameStatsText;
    public Image pauseDifficultyIcon;
    public TMP_Text pauseDifficultyText;
    public Color[] difficultyColors;
    [Header("Scene Config")]
    public string mainMenuScene = "MAINMENU";

    public static bool isPaused = false;
    private const string VolumeKey = "MasterVolume";
    private const string QualityKey = "QualitySetting";
    private const string MotionBlurKey = "MotionBlur";
    private const string SensitivityKey = "MouseSensitivity";
    private const string DifficultyKey = "Difficulty";
    private GameObject[] cachedHudObjects;
    private RoundManager roundManager;
    private PlayerStats playerStats;
    public Sprite[] difficultyIcons;
    public string[] difficultyNames;

    void Start()
    {

        pauseRoot.SetActive(false);
        isPaused = false;
        roundManager = FindObjectOfType<RoundManager>();
        playerStats = PlayerStats.Instance;
        UpdateDifficultyDisplay();
        LoadCurrentSettings();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UpdateInGameStatsUI();
            if (isPaused) Resume();
            else Pause();
        }
    }
    private void UpdateDifficultyDisplay()
    {

        int currentDiff = PlayerPrefs.GetInt(DifficultyKey, 1);
        Color targetColor = Color.white;
        if (difficultyColors != null && currentDiff >= 0 && currentDiff < difficultyColors.Length)
        {
            targetColor = difficultyColors[currentDiff];
        }

        if (pauseDifficultyIcon != null && difficultyIcons != null &&
            currentDiff >= 0 && currentDiff < difficultyIcons.Length)
        {
            pauseDifficultyIcon.sprite = difficultyIcons[currentDiff];
        }


        if (pauseDifficultyText != null && difficultyNames != null &&
            currentDiff >= 0 && currentDiff < difficultyNames.Length)
        {
            pauseDifficultyText.text = difficultyNames[currentDiff];
            pauseDifficultyText.color = targetColor;
        }
    }

    public void Pause()
    {
        isPaused = true;
        pauseRoot.SetActive(true);
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
        UpdateInGameStatsUI();
        UpdateDifficultyDisplay();
        //  Hide all hud
        cachedHudObjects = GameObject.FindGameObjectsWithTag("HUD");
        foreach (var obj in cachedHudObjects)
        {
            obj.SetActive(false);
        }
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {

        isPaused = false;
        pauseRoot.SetActive(false);
        if (cachedHudObjects != null)
        {
            foreach (var obj in cachedHudObjects)
            {

                if (obj != null) obj.SetActive(true);
            }
        }
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }


    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
        LoadCurrentSettings();

    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt(QualityKey, index);
    }

    public void SetMotionBlur(bool isEnabled)
    {

        PlayerPrefs.SetInt(MotionBlurKey, isEnabled ? 1 : 0);

        Volume globalVol = FindObjectOfType<Volume>();
        if (globalVol != null)
        {
            UnityEngine.Rendering.Universal.MotionBlur mb;
            if (globalVol.profile.TryGet(out mb))
            {
                mb.active = isEnabled;
            }
        }
    }
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat(SensitivityKey, sensitivity);

        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = sensitivity.ToString("F1");
        }

        FirstPersonLook playerScript = FindObjectOfType<FirstPersonLook>();
        if (playerScript != null)
        {
            playerScript.sensitivity = sensitivity;
        }
    }
    public void SetJumpscares(bool isEnabled)
    {
        PlayerPrefs.SetInt(JumpscareKey, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    private void LoadCurrentSettings()
    {
        // Volume
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // Quality
        if (qualityDropdown != null)
        {
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }

        // Motion Blur
        if (motionBlurToggle != null)
        {
            int blurState = PlayerPrefs.GetInt(MotionBlurKey, 1);
            motionBlurToggle.isOn = (blurState == 1);
            motionBlurToggle.onValueChanged.RemoveAllListeners();
            motionBlurToggle.onValueChanged.AddListener(SetMotionBlur);
        }
        // sensativity
        float defaultSens = 1f;
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
        // jumpscare
        if (jumpscareToggle != null)
        {
            int jmpState = PlayerPrefs.GetInt(JumpscareKey, 1); // Default to ON (1)
            jumpscareToggle.isOn = (jmpState == 1);
            jumpscareToggle.onValueChanged.RemoveAllListeners();
            jumpscareToggle.onValueChanged.AddListener(SetJumpscares);
        }
    }
    private void UpdateInGameStatsUI()
    {
        if (inGameStatsText == null) return;

        string displayText = "<b>-- GAME STATS --</b>\n\n";
        if (playerStats != null)
        {
            CurrentPlayerStats playerStatsData = playerStats.GetCurrentPlayerMultipliers();

            string bouncing = playerStatsData.hasBouncingBullets ? "<color=yellow>ON</color>" : "<color=grey>OFF</color>";

            displayText +=
                "<b>-- PLAYER BUFFS (Cards) --</b>\n" +
                $"<color=#00ffc4>Damage Multiplier:</color> {playerStatsData.totalDamageMultiplier:0.00}x\n" +
                $"<color=#00ffc4>Fire Rate Multiplier:</color> {playerStatsData.totalFireRateMultiplier:0.00}x\n" +
                $"<color=#00ffc4>Reload Speed Multiplier:</color> {playerStatsData.totalReloadSpeedMultiplier:0.00}x\n" +
                $"<color=#00ffc4>Mag Size Bonus:</color> +{playerStatsData.totalMagazineSizeBonus}\n" +
                $"<color=#e0b3ff>Luck Multiplier:</color> {playerStatsData.totalLuckMultiplier:0.00}x\n" +
                $"<color=#ffcc66>Lifesteal per Hit:</color> {playerStatsData.totalLifeSteal:0.00} HP\n" +
                $"<color=#ffcc66>Bouncing Bullets:</color> {bouncing}\n\n";
        }
        else
        {
            displayText += "Player Stats: PlayerStats not found.\n";
        }
        if (roundManager != null)
        {
            CurrentEnemyStats stats = roundManager.GetCurrentEnemyMultipliers();

            displayText +=
                "<b>-- ENEMY MULTIPLIERS --</b>\n" +
                $"<color=#ff8888>Zombie/Elite Health:</color> {stats.totalHealthMultiplier:0.00}x\n" +
                $"<color=#ff8888>Health Increment:</color> +{stats.currentHealthIncrement:0}/round\n" +
                $"<color=#88ff88>Zombie/Elite Speed:</color> {stats.totalSpeedMultiplier:0.00}x\n" +
                $"<color=#8888ff>Elite Fire Rate:</color> {stats.totalFireRateMultiplier:0.00}x\n" +
                $"<color=#ff88ff>Elite Damage:</color> {stats.totalDamageMultiplier:0.00}x";
        }
        else
        {
            displayText += "Enemy Stats: RoundManager not found.\n\n";
        }

        inGameStatsText.text = displayText;
    }
}
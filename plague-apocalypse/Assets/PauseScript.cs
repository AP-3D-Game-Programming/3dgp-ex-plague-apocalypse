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

    [Header("Settings UI")]
    public Slider volumeSlider;
    public TMP_Dropdown qualityDropdown;
    public Toggle motionBlurToggle;

    [Header("Scene Config")]
    public string mainMenuScene = "MAINMENU";

    public static bool isPaused = false;
    private const string VolumeKey = "MasterVolume";
    private const string QualityKey = "QualitySetting";
    private const string MotionBlurKey = "MotionBlur";
    private GameObject[] cachedHudObjects;
    void Start()
    {

        pauseRoot.SetActive(false);
        isPaused = false;

        LoadCurrentSettings();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }


    public void Pause()
    {
        isPaused = true;
        pauseRoot.SetActive(true);
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);

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
    }
}
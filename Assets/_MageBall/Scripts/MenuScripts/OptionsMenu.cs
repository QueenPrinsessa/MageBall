using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{

    public static readonly string ResolutionPlayerPrefsKey = "Resolution";
    public static readonly string QualityPlayerPrefsKey = "GraphicsQuality";
    public static readonly string WindowModePlayerPrefsKey = "WindowMode";
    public static readonly string MasterVolumePlayerPrefsKey = "MasterVolume";
    public static readonly string MouseSensitivityPlayerPrefsKey = "MouseSensitivity";
    public static readonly string InvertMouseXAxisPlayerPrefsKey = "InvertMouseX";
    public static readonly string InvertMouseYAxisPlayerPrefsKey = "InvertMouseY";

    [Header("General")]
    [SerializeField] private GameObject optionsCanvas;

    [Header("Graphics settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown graphicsQualityDropdown;
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    private Resolution[] resolutions;

    [Header("Control settings")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle invertMouseXToggle;
    [SerializeField] private Toggle invertMouseYToggle;

    [Header("Sound settings")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer audioMixer;

    public event Action OptionsMenuOpened;
    public event Action OptionsMenuClosed;
    public static event Action ControlSettingsChanged;

    private void Start()
    {
        LoadGraphicsSettings();
        LoadControlSettings();
        LoadSoundSettings();
    }

    private void LoadGraphicsSettings()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> resolutionOptions = new List<string>();

        int resolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string resolutionOption = $"{resolutions[i].width} x {resolutions[i].height} {resolutions[i].refreshRate}Hz";
            resolutionOptions.Add(resolutionOption);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                resolutionIndex = i;
        }

        resolutionIndex = PlayerPrefs.GetInt(ResolutionPlayerPrefsKey, resolutionIndex);
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = resolutionIndex;
        resolutionDropdown.RefreshShownValue();
        SetResolution(resolutionIndex);

        int windowMode = PlayerPrefs.GetInt(WindowModePlayerPrefsKey, (int)Screen.fullScreenMode);
        windowModeDropdown.value = windowMode;
        SetWindowMode(windowMode);

        int quality = PlayerPrefs.GetInt(QualityPlayerPrefsKey, QualitySettings.GetQualityLevel());
        graphicsQualityDropdown.value = quality;
        SetQuality(quality);
    }

    private void LoadControlSettings()
    {
        float mouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityPlayerPrefsKey, 1f);
        mouseSensitivitySlider.value = mouseSensitivity;

        int invertMouseX = PlayerPrefs.GetInt(InvertMouseXAxisPlayerPrefsKey, Convert.ToInt32(false));
        invertMouseXToggle.isOn = Convert.ToBoolean(invertMouseX);

        int invertMouseY = PlayerPrefs.GetInt(InvertMouseYAxisPlayerPrefsKey, Convert.ToInt32(false));
        invertMouseYToggle.isOn = Convert.ToBoolean(invertMouseY);
    }

    private void Update()
    {
        if (!optionsCanvas.activeInHierarchy)
            return;

        if (Input.GetButtonDown("Cancel") || Input.GetButtonDown("Submit"))
            CloseMenu();
    }


    private void LoadSoundSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat(MasterVolumePlayerPrefsKey, 0f);
        volumeSlider.value = masterVolume;
        SetMasterVolume(masterVolume);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt(ResolutionPlayerPrefsKey, resolutionIndex);
    }

    public void SetWindowMode(int windowModeIndex)
    {
        if (windowModeIndex == (int) FullScreenMode.MaximizedWindow)
            windowModeIndex = (int) FullScreenMode.Windowed;

        Screen.fullScreenMode = (FullScreenMode) windowModeIndex;
        PlayerPrefs.SetInt(WindowModePlayerPrefsKey, windowModeIndex);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt(QualityPlayerPrefsKey, qualityIndex);
    }

    public void SetInvertMouseYAxis(bool invert)
    {
        PlayerPrefs.SetInt(InvertMouseYAxisPlayerPrefsKey, Convert.ToInt32(invert));
        ControlSettingsChanged?.Invoke();
    }

    public void SetInvertMouseXAxis(bool invert)
    {
        PlayerPrefs.SetInt(InvertMouseXAxisPlayerPrefsKey, Convert.ToInt32(invert));
        ControlSettingsChanged?.Invoke();
    }

    public void SetMouseSensitivity(float mouseSensitivity)
    {
        PlayerPrefs.SetFloat(MouseSensitivityPlayerPrefsKey, mouseSensitivity);
        ControlSettingsChanged?.Invoke();
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat(MasterVolumePlayerPrefsKey, volume);
    }

    public void OpenMenu()
    {
        optionsCanvas.SetActive(true);
        OptionsMenuOpened?.Invoke();
    }

    public void CloseMenu()
    {
        optionsCanvas.SetActive(false);
        OptionsMenuClosed?.Invoke();
    }

}

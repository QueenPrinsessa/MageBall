using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace MageBall
{
    public class OptionsMenu : MonoBehaviour
    {

        public static readonly string ResolutionPlayerPrefsKey = "Resolution";
        public static readonly string QualityPlayerPrefsKey = "GraphicsQuality";
        public static readonly string WindowModePlayerPrefsKey = "WindowMode";
        public static readonly string MasterVolumePlayerPrefsKey = "MasterVolume";
        public static readonly string MusicVolumePlayerPrefsKey = "MusicVolume";
        public static readonly string EffectsVolumePlayerPrefsKey = "EffectsVolume";
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
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider effectsVolumeSlider;
        [SerializeField] private AudioMixer audioMixer;

        public event Action OptionsMenuOpened;
        public event Action OptionsMenuClosed;
        public static event Action ControlSettingsChanged;

        public bool IsOpen => optionsCanvas.activeInHierarchy;

        private void Start()
        {
            InitializeResolutions();
            RestoreSettingsFromPrefs();
            ApplySettings();
        }

        private void InitializeResolutions()
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();

            List<string> resolutionOptions = new List<string>();

            for (int i = 0; i < resolutions.Length; i++)
            {
                string resolutionOption = $"{resolutions[i].width} x {resolutions[i].height} {resolutions[i].refreshRate}Hz";
                resolutionOptions.Add(resolutionOption);
            }

            resolutionDropdown.AddOptions(resolutionOptions);
            resolutionDropdown.RefreshShownValue();
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            if (Input.GetButtonDown("Cancel"))
                Cancel();
        }

        public void ApplySettings()
        {
            SetResolution(resolutionDropdown.value);
            SetWindowMode(windowModeDropdown.value);
            SetQuality(graphicsQualityDropdown.value);
            SetMouseSensitivity(mouseSensitivitySlider.value);
            SetMasterVolume(masterVolumeSlider.value);
            SetMusicVolume(musicVolumeSlider.value);
            SetEffectsVolume(effectsVolumeSlider.value);
            SetInvertMouseXAxis(invertMouseXToggle.isOn);
            SetInvertMouseYAxis(invertMouseYToggle.isOn);
        }

        private void RestoreSettingsFromPrefs()
        {
            int resolutionIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height && resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
                    resolutionIndex = i;
            }
            resolutionIndex = PlayerPrefs.GetInt(ResolutionPlayerPrefsKey, resolutionIndex);
            int quality = PlayerPrefs.GetInt(QualityPlayerPrefsKey, QualitySettings.GetQualityLevel());
            int windowMode = PlayerPrefs.GetInt(WindowModePlayerPrefsKey, (int)Screen.fullScreenMode);
            float masterVolume = PlayerPrefs.GetFloat(MasterVolumePlayerPrefsKey, 1f);
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumePlayerPrefsKey, 1f);
            float effectsVolume = PlayerPrefs.GetFloat(EffectsVolumePlayerPrefsKey, 1f);
            float mouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityPlayerPrefsKey, 1f);
            int invertMouseX = PlayerPrefs.GetInt(InvertMouseXAxisPlayerPrefsKey, Convert.ToInt32(false));
            int invertMouseY = PlayerPrefs.GetInt(InvertMouseYAxisPlayerPrefsKey, Convert.ToInt32(false));

            resolutionDropdown.value = resolutionIndex;
            graphicsQualityDropdown.value = quality;
            windowModeDropdown.value = windowMode;
            mouseSensitivitySlider.value = mouseSensitivity;
            masterVolumeSlider.value = masterVolume;
            musicVolumeSlider.value = musicVolume;
            effectsVolumeSlider.value = effectsVolume;
            invertMouseXToggle.isOn = Convert.ToBoolean(invertMouseX);
            invertMouseYToggle.isOn = Convert.ToBoolean(invertMouseY);
        }

        public void Cancel()
        {
            RestoreSettingsFromPrefs();
            CloseMenu();
        }

        public void SetResolution(int resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetInt(ResolutionPlayerPrefsKey, resolutionIndex);
        }

        public void SetWindowMode(int windowModeIndex)
        {
            if (windowModeIndex == (int)FullScreenMode.MaximizedWindow)
                windowModeIndex = (int)FullScreenMode.Windowed;

            Screen.fullScreenMode = (FullScreenMode)windowModeIndex;
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
            SetVolumeLogarithmically("MasterVolume", volume, MasterVolumePlayerPrefsKey);
        }

        public void SetMusicVolume(float volume)
        {
            SetVolumeLogarithmically("MusicVolume", volume, MusicVolumePlayerPrefsKey);
        }
        public void SetEffectsVolume(float volume)
        {
            SetVolumeLogarithmically("EffectsVolume", volume, EffectsVolumePlayerPrefsKey);
        }

        private void SetVolumeLogarithmically(string name, float volume, string playerPrefsKey)
        {
            audioMixer.SetFloat(name, Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat(playerPrefsKey, volume);
        }

        public void OpenMenu()
        {
            RestoreSettingsFromPrefs();
            optionsCanvas.SetActive(true);
            OptionsMenuOpened?.Invoke();
        }

        public void CloseMenu()
        {
            optionsCanvas.SetActive(false);
            OptionsMenuClosed?.Invoke();
        }

    }
}
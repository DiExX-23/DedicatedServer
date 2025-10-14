using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Brightness")]
    public Slider brightnessSlider;
    public float brightnessSliderValue;
    public Image brightnessPanel;

    [Header("Volume")]
    public Slider volumeSlider;
    public float volumeSliderValue;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    void Start()
    {
        brightnessSlider.value = PlayerPrefs.GetFloat("brightness", 0.5f);
        float alpha = 1f - brightnessSlider.value;
        alpha = Mathf.Clamp(alpha, 0f, 1f);
        brightnessPanel.color = new Color(brightnessPanel.color.r, brightnessPanel.color.g, brightnessPanel.color.b, alpha);

        volumeSlider.value = PlayerPrefs.GetFloat("audioVolume", 0.5f);
        AudioListener.volume = volumeSlider.value;
        CheckIfMuted();
    }

    public void FullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void ChangeBrightness(float value)
    {
        brightnessSliderValue = value;
        PlayerPrefs.SetFloat("brightness", brightnessSliderValue);
        float alpha = 1f - brightnessSliderValue;
        alpha = Mathf.Clamp(alpha, 0f, 1f);
        brightnessPanel.color = new Color(brightnessPanel.color.r, brightnessPanel.color.g, brightnessPanel.color.b, alpha);
    }

    public void ChangeVolume(float value)
    {
        volumeSliderValue = value;
        PlayerPrefs.SetFloat("audioVolume", volumeSliderValue);
        AudioListener.volume = volumeSlider.value;
        CheckIfMuted();
    }

    void CheckIfMuted()
    {
        if (volumeSlider.value <= 0.01f)
        {
            Debug.Log("The game is muted.");
        }
        else
        {
            Debug.Log("The game has volume.");
        }
    }

    public void ChangeQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }
}

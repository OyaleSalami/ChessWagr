using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public AudioSource audioSource; // The AudioSource whose volume you want to control
    public Slider volumeSlider; // The slider to adjust the volume

    private const string VolumeKey = "Volume1"; // Key to save volume in PlayerPrefs

    void Start()
    {
        // Load saved volume and set the slider value at the start
        LoadVolumeSetting();

        // Add a listener to the slider to handle volume changes
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    // Method to handle volume changes from the slider
    void OnVolumeChanged(float value)
    {
        audioSource.volume = value;
        SaveVolumeSetting(value); // Save the new volume to PlayerPrefs
    }

    // Save the volume setting using PlayerPrefs
    void SaveVolumeSetting(float volume)
    {
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    // Load the volume setting from PlayerPrefs
    void LoadVolumeSetting()
    {
        if (PlayerPrefs.HasKey(VolumeKey))
        {
            float savedVolume = PlayerPrefs.GetFloat(VolumeKey);
            audioSource.volume = savedVolume;
            volumeSlider.value = savedVolume;
        }
        else
        {
            audioSource.volume = volumeSlider.value; // Default to slider's value if no saved volume exists
        }
    }
}

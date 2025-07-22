using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SongManager : MonoBehaviour
{
    public AudioSource audioSource; // The AudioSource component to play the songs
    public List<AudioClip> songs = new List<AudioClip>(); // List to hold songs
    public Button switchSongButton; // Button to switch the song
    public Slider volumeSlider; // Slider to control the volume

    private int currentSongIndex = -1; // Tracks the current song index
    private const string VolumeKey = "Volume"; // Key to save volume in PlayerPrefs

    void Start()
    {
        // Load saved volume and set the slider value
        LoadVolumeSetting();

        // Play a random song at the start of the game
        PlayRandomSong();

        // Add a listener to the switch song button
        switchSongButton.onClick.AddListener(SwitchSong);

        // Add a listener to the slider to control volume
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void Update()
    {
        // Check if the song has finished playing
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            SwitchSong(); // Automatically switch to the next song when the current one ends
        }
    }

    // Method to play a random song
    void PlayRandomSong()
    {
        if (songs.Count == 0)
        {
            return;
        }

        // Randomly select a song that isn't currently playing
        int randomIndex = Random.Range(0, songs.Count);
        while (randomIndex == currentSongIndex && songs.Count > 1)
        {
            randomIndex = Random.Range(0, songs.Count); // Ensure we get a different song
        }

        currentSongIndex = randomIndex;
        audioSource.clip = songs[currentSongIndex];
        audioSource.Play();
    }

    // Method to switch the song
    void SwitchSong()
    {
        if (songs.Count == 0)
        {
            return;
        }

        // Move to the next song in the list
        currentSongIndex = (currentSongIndex + 1) % songs.Count;
        audioSource.clip = songs[currentSongIndex];
        audioSource.Play();

    }

    // Method to handle volume slider changes
    void OnVolumeChanged(float value)
    {
        audioSource.volume = value;
        SaveVolumeSetting(value); // Save the volume when changed
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

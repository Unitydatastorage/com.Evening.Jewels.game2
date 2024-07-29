using UnityEngine;

public class SoundReset : MonoBehaviour
{
    public AudioSource soundManager; // SoundManager
    public AudioSource musicManager; // MusicManager
    public GameObject onOffButton; // ON>OFF
    public GameObject offOnButton; // OFF>ON
    public GameObject onOffButtonMusic; // ON>OFF
    public GameObject offOnButtonMusic; // OFF>ON

    public void ResetSound()
    {
        // Включаем звук и музыку
        soundManager.mute = false; 
        musicManager.mute = false; 

        // Управляем видимостью кнопок
        onOffButton.SetActive(true); 
        offOnButton.SetActive(false); 
        // Управляем видимостью кнопок
        onOffButtonMusic.SetActive(true); 
        offOnButtonMusic.SetActive(false); 
    }
}
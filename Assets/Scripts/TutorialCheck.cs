using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCheck : MonoBehaviour
{
    private int tutorial;
    public GameObject tutorialWindow;
    void Start()
    {
        LoadLevels();
        if(tutorial == 0){
            tutorialWindow.SetActive(true);
        }
    }
    public void TutorialDone(){
        tutorial = 1;
        PlayerPrefs.SetInt("Tutor", tutorial);
        PlayerPrefs.Save();
    }
    void LoadLevels()
    {
        tutorial = PlayerPrefs.GetInt("Tutor", 0);
    
    }
}
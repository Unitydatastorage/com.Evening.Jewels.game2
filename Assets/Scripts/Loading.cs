using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Loading : MonoBehaviour
{
    public Slider loadingSlider;

    void Start()
    {
        StartCoroutine(LoadSceneAsync("Game")); 
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        
        while (!asyncLoad.isDone)
        {
            
            loadingSlider.value = asyncLoad.progress;

            
            if (asyncLoad.progress >= 0.9f)
            {
                loadingSlider.value = 1f;
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Home : MonoBehaviour
{
    public static string previousScene;

    public void LoadLevelsScene()
    {
        previousScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("Levels");
    }

    public void LoadCreditsScene()
    {
        previousScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("Credits");
    }

    public void LoadNextLevel() //make sure to load the levels in order in the build settings
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels to load.");
        }
    }

    public void GoBack()
    {
        if (!string.IsNullOrEmpty(previousScene))
        {
            SceneManager.LoadScene(previousScene);
        }
        else
        {
            Debug.Log("No previous scene to go back to");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Home : MonoBehaviour
{
    public static string previousScene;
    public List<AudioClip> onHoverClips;
    public AudioClip onClickCredits;
    public AudioClip onClickPlay;
    public void LoadLevelsScene()
    {
        previousScene = SceneManager.GetActiveScene().name;
        SoundManager.current.SFXPlayer.PlayOneShot(onClickCredits);
        SceneManager.LoadScene("Levels");
    }

    public void LoadCreditsScene()
    {
        previousScene = SceneManager.GetActiveScene().name;
        SoundManager.current.SFXPlayer.PlayOneShot(onClickCredits);
        SceneManager.LoadScene("Credits");
    }

    public void LoadNextLevel() //make sure to load the levels in order in the build settings
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        SoundManager.current.SFXPlayer.PlayOneShot(onClickPlay);

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels to load.");
        }
    }
    public void OnHover()
    {
        SoundManager.current.SFXPlayer.PlayOneShot(onHoverClips[Random.Range(0, onHoverClips.Count - 1)]);
    }
    public void GoBack()
    {
        if (!string.IsNullOrEmpty(previousScene))
        {
            SoundManager.current.SFXPlayer.PlayOneShot(onClickCredits);
            SceneManager.LoadScene(previousScene);
        }
        else
        {
            Debug.Log("No previous scene to go back to");
        }
    }
}

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
    public GameObject menuButton;
    public GameObject menuPanel;
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseGame()
    {
        Debug.Log("PauseGame called");
        SoundManager.current.SFXPlayer.PlayOneShot(onClickCredits);
        Time.timeScale = 0f;
        menuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        SoundManager.current.SFXPlayer.PlayOneShot(onClickCredits);
        Time.timeScale = 1f;
        menuPanel.SetActive(false);
        menuButton.SetActive(true);
    }
    public void LoadHome()
    {
        SoundManager.current.SFXPlayer.PlayOneShot(onClickCredits);
        SceneManager.LoadScene("Home");
    }

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

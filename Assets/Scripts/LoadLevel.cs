using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    public void LoadScene()
    {
        if(LevelManager.gameStarted) 
        {
            SceneManager.LoadScene(12);
        }
        else
        {
            LevelManager.gameStarted = true;
            SceneManager.LoadScene(1);
        }
    }

    public void LoadSceneIndex(int i)
    {
        SceneManager.LoadScene(i);
    }
}

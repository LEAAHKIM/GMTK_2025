using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class WaterManager : MonoBehaviour
{
    public Image waterBar;
    public float waterAmount = 0.3f;

    //to check if player has enough water for an action
    public bool UseWater(float amount)
    {
        if (waterAmount >= amount)
        {
            DecreaseWater(amount);
            return true;
        }
        else
        {
            Debug.Log("Not enough water!");
            return false;
        }
    }

    public void DecreaseWater(float amount)
    {
        waterAmount -= amount;
        waterBar.fillAmount = waterAmount / 1f;
        if (waterAmount < 0)
        {
            waterAmount = 0;
            // TODO: player evaporates and gets sent to the start appearing as a cloud
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void IncreaseWater(float amount)
    {
        waterAmount += amount;
        waterAmount = Mathf.Clamp(waterAmount, 0f, 1f);
        waterBar.fillAmount = waterAmount / 1f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("WaterSource"))
        {
            IncreaseWater(0.2f); // Example amount to increase water
            Debug.Log("Water collected!");
        }
        if (other.CompareTag("WaterDeplete"))
        {
            DecreaseWater(0.2f); // Example amount to decrease water
            Debug.Log("Water drained!");
        }
    }
}

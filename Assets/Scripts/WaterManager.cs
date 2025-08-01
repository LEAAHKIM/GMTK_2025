using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class WaterManager : MonoBehaviour
{
    public Image waterBar;
    public float waterAmount = 0.5f;
    private float waterLerpDuration = 0.35f; // animation time
    private Coroutine fillAnimCoroutine;


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
        if (waterAmount < 0)
        {
            waterAmount = 0;
            StartFillAnimation(waterAmount / 1f);
            // TODO: evaporate here ?
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            StartFillAnimation(waterAmount / 1f);
        }
    }

    public void IncreaseWater(float amount)
    {
        waterAmount += amount;
        waterAmount = Mathf.Clamp(waterAmount, 0f, 1f);
        StartFillAnimation(waterAmount / 1f);
    }

    private void StartFillAnimation(float targetFill)
    {
        if (fillAnimCoroutine != null)
        {
            StopCoroutine(fillAnimCoroutine);
        }
        fillAnimCoroutine = StartCoroutine(AnimateFillAmount(targetFill));
    }

    private IEnumerator AnimateFillAmount(float targetFill)
    {
        float startFill = waterBar.fillAmount;
        float elapsed = 0f;

        while (elapsed < waterLerpDuration)
        {
            elapsed += Time.deltaTime;
            waterBar.fillAmount = Mathf.Lerp(startFill, targetFill, elapsed / waterLerpDuration);
            yield return null;
        }

        waterBar.fillAmount = targetFill;
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


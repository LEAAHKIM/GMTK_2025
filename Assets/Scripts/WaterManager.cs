using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class WaterManager : MonoBehaviour
{
    public Image _waterBar;
    public float _waterAmount = 0.5f;
    private float _waterLerpDuration = 0.35f; // animation time
    private Coroutine _fillAnimCoroutine;
    private bool _isEvaporating = false;

    //to check if player has enough water for an action
    public bool UseWater(float amount)
    {
        if (_waterAmount >= amount)
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
        if (_isEvaporating) return;

        _waterAmount -= amount;
        if (_waterAmount < 0)
        {
            _waterAmount = 0;
            StartFillAnimation(_waterAmount / 1f);
            StartCoroutine(Evaporate());
        }
        else
        {
            StartFillAnimation(_waterAmount / 1f);
        }
    }

    public void IncreaseWater(float amount)
    {
        _waterAmount += amount;
        _waterAmount = Mathf.Clamp(_waterAmount, 0f, 1f);
        StartFillAnimation(_waterAmount / 1f);
    }

    private void StartFillAnimation(float targetFill)
    {
        if (_fillAnimCoroutine != null)
        {
            StopCoroutine(_fillAnimCoroutine);
        }
        _fillAnimCoroutine = StartCoroutine(AnimateFillAmount(targetFill));
    }

    private IEnumerator AnimateFillAmount(float targetFill)
    {
        float startFill = _waterBar.fillAmount;
        float elapsed = 0f;

        while (elapsed < _waterLerpDuration)
        {
            elapsed += Time.deltaTime;
            _waterBar.fillAmount = Mathf.Lerp(startFill, targetFill, elapsed / _waterLerpDuration);
            yield return null;
        }

        _waterBar.fillAmount = targetFill;
    }

    private IEnumerator Evaporate()
    {
        _isEvaporating = true;
        //TODO: play poof animation 
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Poof");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("WaterSource"))
        {
            IncreaseWater(0.2f); // Example amount to increase water
            Debug.Log("Water collected!");
            Destroy(other.gameObject);
        }
        if (other.CompareTag("WaterDeplete"))
        {
            DecreaseWater(0.2f); // Example amount to decrease water
            Debug.Log("Water drained!");
        }
    }
}


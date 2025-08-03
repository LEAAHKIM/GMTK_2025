using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackScreenAppear : MonoBehaviour
{
    public float blackScreenTime;
    private float _blackScreenTimer;
    public Image blackScreenImage;
    private void Start()
    {
        if (blackScreenTime == 0) { blackScreenTime = 5; }
    }
    public void SetBlackScreen()
    {
        _blackScreenTimer = blackScreenTime;
    }
    public void RemoveBlackScreen()
    {

    }
    public void Update()
    {
        blackScreenImage.color = new Color(0,0,0, _blackScreenTimer / blackScreenTime);
        _blackScreenTimer -= Time.deltaTime;
        _blackScreenTimer = Mathf.Min(_blackScreenTimer, 0);
    }
}

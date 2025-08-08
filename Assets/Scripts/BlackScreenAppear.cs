using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackScreenAppear : MonoBehaviour
{
    public static BlackScreenAppear current;
    public float blackScreenTime;
    private float _blackScreenTimer;
    public Image blackScreenImage;
    bool _appear;
    private void Awake()
    {
        current = this;
    }
    private void Start()
    {
        if (blackScreenTime == 0) { blackScreenTime = 1; }
        SetBlackScreen(false);
    }
    public void SetBlackScreen(bool appear = false, float delay=0)
    {
        _blackScreenTimer = blackScreenTime+delay;
        _appear = appear;
    }
    public void Update()
    {
        float a = _blackScreenTimer / blackScreenTime;
        if (_appear) { a = 1 - a; }
        a = Mathf.Clamp01(a);
        blackScreenImage.color = new Color(0,0,0, a);
        _blackScreenTimer -= Time.deltaTime;
        _blackScreenTimer = Mathf.Max(_blackScreenTimer, 0);
    }
}

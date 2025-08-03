using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{

    public float floatSpeed = 3.3f;        
    public float floatAmount = 0.15f;     

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}

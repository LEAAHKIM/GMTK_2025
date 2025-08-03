using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private void Start()
    {
        LevelManager.current.collectibleAmount++;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        LevelManager.current.collectibleAmount--;
        Destroy(gameObject);
    }
}

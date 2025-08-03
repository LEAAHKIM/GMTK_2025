using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostInstantiate : MonoBehaviour
{
    GameObject[] _gameObjects;
    [HideInInspector]public bool isOriginal=true;
    private void Start()
    {
        if (!isOriginal) { return; }
        int idx = 0;
        _gameObjects = new GameObject[8];
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 & j == 0) { continue; }
                Vector2 offset = (new Vector2(i, j)) * LevelManager.current.currentLevelExtents * 2;
                GameObject a = Instantiate(gameObject);
                a.GetComponent<GhostInstantiate>().isOriginal = false;

                a.transform.position = transform.position + (Vector3)offset;
                _gameObjects[idx] = a;
                idx++;
            }
        }
    }
    private void OnDestroy()
    {
        if (!isOriginal) { return; }

        for (int i = 0; i < 8; i++)
        {
            Destroy(_gameObjects[i]);
        }
    }
}

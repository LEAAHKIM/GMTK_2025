using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// for static ghost objects
public class GhostObjectStatic : MonoBehaviour
{
    GameObject[] _gameObjects;
    private void Start()
    {
        int idx = 0;
        _gameObjects = new GameObject[8];
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 & j == 0) { continue; }
                Vector2 offset = (new Vector2(i, j)) * LevelManager.current.currentLevelExtents * 2;
                GameObject a = new GameObject();
                if (TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRend))
                {
                    SpriteRenderer newSpriteRend = a.AddComponent<SpriteRenderer>();
                    newSpriteRend.sprite = spriteRend.sprite;
                    newSpriteRend.color = spriteRend.color;
                }
                if (TryGetComponent<BoxCollider2D>(out BoxCollider2D boxCol))
                {
                    BoxCollider2D newBoxCol = a.AddComponent<BoxCollider2D>();
                    newBoxCol.offset = boxCol.offset;
                    newBoxCol.size = boxCol.size;
                }
                a.layer = gameObject.layer;
                a.transform.position = transform.position + (Vector3)offset;
                a.transform.localScale = transform.localScale;
                a.transform.localRotation = transform.localRotation;

                _gameObjects[idx] = a;
                idx++;
            }
        }
    }
    private void OnDestroy()
    {
        for (int i = 0; i < 8; i++)
        {
            Destroy(_gameObjects[i]);
        }
    }
}

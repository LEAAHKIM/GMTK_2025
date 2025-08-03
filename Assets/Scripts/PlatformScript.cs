using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformScript : MonoBehaviour
{
    BoxCollider2D _col;
    Vector2 colExtents;
    private void Start()
    {
        _col = GetComponent<BoxCollider2D>();
        colExtents = _col.size / 2;
    }
    private void FixedUpdate()
    {
        if ( (LevelManager.current.playerMovement.transform.position.y-.55f) >= (transform.position.y+colExtents.y+_col.offset.y)) { _col.enabled = true; }
        else { _col.enabled = false; }
        if (LevelManager.current.goDownInput) { _col.enabled = false; }
    }
}

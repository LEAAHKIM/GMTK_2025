using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGhostObject : MonoBehaviour
{
    Tilemap tilemap;
    //TilemapRenderer renderer;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }
    private void Start()
    {
        Vector3Int levelSize = new Vector3Int(
    (int)(LevelManager.current.currentLevelExtents.x * 2),
    (int)(LevelManager.current.currentLevelExtents.y * 2),
    1
);

        BoundsInt bounds = new BoundsInt(tilemap.origin, levelSize) ; // new BoundsInt(Vector3Int.zero, new Vector3Int( levelSize.x, levelSize.y, 1));
        TileBase[] a = tilemap.GetTilesBlock(bounds);

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) { continue; }
                Vector3Int offsetPosition = new Vector3Int(levelSize.x * i, levelSize.y * j, 0);
                BoundsInt targetBounds = new BoundsInt(bounds.position + offsetPosition, levelSize);
                
                tilemap.SetTilesBlock(targetBounds, a);
            }
        }
    }
}

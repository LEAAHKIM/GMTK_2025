using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/Anchored Rule Tile")]
public class AnchoredRuleTile : RuleTile
{
    [SerializeField]
    private List<Vector2> anchorOffsets = new List<Vector2>();

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        Matrix4x4 transform = Matrix4x4.identity;

        for (int i = 0; i < m_TilingRules.Count; i++)
        {
            var rule = m_TilingRules[i];
            if (RuleMatches(rule, position, tilemap,ref transform))
            {
                tileData.sprite = rule.m_Sprites[0];
                tileData.colliderType = rule.m_ColliderType;
                tileData.flags = TileFlags.LockAll;

                Vector2 offset = Vector2.zero;
                if (i < anchorOffsets.Count)
                    offset = anchorOffsets[i];

                tileData.transform = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
                return;
            }
        }

        base.GetTileData(position, tilemap, ref tileData);
    }
}
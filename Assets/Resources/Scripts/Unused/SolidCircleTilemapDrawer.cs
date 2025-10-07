using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CircleWithWallTilemapDrawer : MonoBehaviour
{
    [Header("基本设置")]
    public Tilemap groundTilemap;    // 地面Tilemap
    public Tilemap wallTilemap;      // 墙壁Tilemap
    public TileBase groundTile;      // 地面瓦片
    public TileBase wallTile;        // 墙壁瓦片
    
    [Header("圆形参数")]
    public int radius = 5;
    public Vector3Int center = Vector3Int.zero;
    public int wallThickness = 1;    // 墙壁厚度
    
    [Header("调试")]
    public bool showGizmos = true;

    // 绘制带墙壁的圆形
    [ContextMenu("绘制带墙壁的圆形")]
    public void DrawCircleWithWall()
    {
        if (groundTilemap == null || wallTilemap == null || groundTile == null || wallTile == null)
        {
            Debug.LogError("请先分配所有Tilemap和Tile引用！");
            return;
        }
        
        Debug.Log("开始绘制带墙壁的圆形...");
        
        // 清除之前的圆形
        ClearCircle();
        
        float outerRadiusSquared = radius * radius;
        float innerRadiusSquared = (radius - wallThickness) * (radius - wallThickness);
        
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int tilePosition = new Vector3Int(center.x + x, center.y + y, center.z);
                float distanceSquared = x * x + y * y;
                
                // 绘制墙壁（在内外半径之间）
                if (distanceSquared <= outerRadiusSquared && distanceSquared > innerRadiusSquared)
                {
                    wallTilemap.SetTile(tilePosition, wallTile);
                }
                // 绘制地面（在内半径之内）
                else if (distanceSquared <= innerRadiusSquared)
                {
                    groundTilemap.SetTile(tilePosition, groundTile);
                }
            }
        }
        
        Debug.Log($"带墙壁圆形绘制完成！半径: {radius}, 墙壁厚度: {wallThickness}, 中心点: {center}");
    }

    // 只绘制实心圆形（不带墙壁）
    [ContextMenu("只绘制实心圆形")]
    public void DrawSolidCircleOnly()
    {
        if (groundTilemap == null || groundTile == null) return;
        
        ClearCircle();
        
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                float distanceSquared = x * x + y * y;
                
                if (distanceSquared <= radius * radius)
                {
                    Vector3Int tilePosition = new Vector3Int(center.x + x, center.y + y, center.z);
                    groundTilemap.SetTile(tilePosition, groundTile);
                }
            }
        }
    }

    // 只绘制圆形墙壁
    [ContextMenu("只绘制圆形墙壁")]
    public void DrawCircleWallOnly()
    {
        if (wallTilemap == null || wallTile == null) return;
        
        ClearWallsOnly();
        
        float outerRadiusSquared = radius * radius;
        float innerRadiusSquared = (radius - wallThickness) * (radius - wallThickness);
        
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                float distanceSquared = x * x + y * y;
                
                if (distanceSquared <= outerRadiusSquared && distanceSquared > innerRadiusSquared)
                {
                    Vector3Int tilePosition = new Vector3Int(center.x + x, center.y + y, center.z);
                    wallTilemap.SetTile(tilePosition, wallTile);
                }
            }
        }
    }

    [ContextMenu("清除所有")]
    public void ClearCircle()
    {
        if (groundTilemap != null)
        {
            for (int x = -radius - wallThickness; x <= radius + wallThickness; x++)
            {
                for (int y = -radius - wallThickness; y <= radius + wallThickness; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(center.x + x, center.y + y, center.z);
                    groundTilemap.SetTile(tilePosition, null);
                }
            }
        }
        
        ClearWallsOnly();
    }

    [ContextMenu("只清除墙壁")]
    public void ClearWallsOnly()
    {
        if (wallTilemap != null)
        {
            for (int x = -radius - wallThickness; x <= radius + wallThickness; x++)
            {
                for (int y = -radius - wallThickness; y <= radius + wallThickness; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(center.x + x, center.y + y, center.z);
                    wallTilemap.SetTile(tilePosition, null);
                }
            }
        }
    }

    // 在编辑器中显示可视化
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || groundTilemap == null) return;
        
        Vector3 worldCenter = groundTilemap.CellToWorld(center);
        float cellSize = groundTilemap.cellSize.x;
        
        // 绘制外圆（墙壁外边界）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(worldCenter, radius * cellSize);
        
        // 绘制内圆（墙壁内边界/地面边界）
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(worldCenter, (radius - wallThickness) * cellSize);
        
        // 绘制中心点
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(worldCenter, Vector3.one * cellSize * 0.5f);
    }

    // 验证参数合理性
    private void OnValidate()
    {
        wallThickness = Mathf.Clamp(wallThickness, 1, radius - 1);
    }
}
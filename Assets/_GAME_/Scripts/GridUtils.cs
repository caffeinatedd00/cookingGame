using UnityEngine;

public static class GridUtils
{
    public static Vector2Int WorldToGrid(Vector2 worldPos)//converts world position to grid position
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    public static Vector2 GridToWorld(Vector2 gridPos)//converts grid position to world position
    {
        return new Vector2(gridPos.x, gridPos.y);
    }
}

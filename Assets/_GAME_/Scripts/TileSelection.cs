using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class TileSelection : MonoBehaviour
{
    public Camera mainCamera;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap obstacleTilemap;
    [SerializeField] private float offset = 0.5f;
    [SerializeField] private Vector2 gridSize = new Vector2(1f, 1f);

    private Vector2Int highlightedTilePosition = Vector2Int.zero;

    void Update() //tileSelector selects a tile that the mouse cursor is hovered on
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2Int gridPos = new Vector2Int(
            Mathf.FloorToInt(mouseWorldPos.x / gridSize.x) * Mathf.RoundToInt(gridSize.x),
            Mathf.FloorToInt(mouseWorldPos.y / gridSize.y) * Mathf.RoundToInt(gridSize.y)
        );

        highlightedTilePosition = gridPos;
        Vector2 worldPos = GridUtils.GridToWorld(gridPos) + new Vector2(offset, offset);
        transform.position = worldPos;
    }

    public Vector2Int HighlightedPosition
    {
        get {return highlightedTilePosition;}
    }

    public bool IsHighlightedTileClicked(Vector2 clickedPosition)//check for clicks
    {
        Vector2Int gridPos = GridUtils.WorldToGrid(clickedPosition);
        return gridPos == highlightedTilePosition;
    }

    public Vector2 GetHighlightedTilePos()//returns mouse cursor location
    {
        return GridUtils.GridToWorld(highlightedTilePosition);
    }

    public bool isTileObastacle(Vector2Int position)//check for obstacles
    {
        Vector3 worldPositon = GridUtils.GridToWorld(position);
        RaycastHit2D hit = Physics2D.Raycast(worldPositon, Vector2.zero);
        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }

}

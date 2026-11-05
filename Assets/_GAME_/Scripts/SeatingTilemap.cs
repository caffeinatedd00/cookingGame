using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static SeatingTilemap;

public class SeatingTilemap : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;

    [SerializeField] private ObstacleTilemap obstacleTilemap;

    private HashSet<Vector3Int> seatingTilePositions = new HashSet<Vector3Int>();
    public List<Seat> seats = new List<Seat>();

    public class Seat
    {
        public Vector3Int seatTile;
        public Vector2Int approachTile;
        public bool isOccupied;
        public CustomerBehaviour currentCustomer;
    }

    private void Awake()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();

        InitializeSeatingTiles();
    }

    private void Start()
    {
        FindSeatProx();
    }

    public void InitializeSeatingTiles() //stores seating tiles into a list of seat classed objects
    {
        seatingTilePositions.Clear();

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Vector3Int tilePosition = new Vector3Int(bounds.x + x, bounds.y + y, 0);
                    seatingTilePositions.Add(tilePosition);
                }
            }
        }

    }

    public void FindSeatProx() //finds the neighbouring tile of seats from list and select first tile that is not a seat or a tile and adds them to their class
    {
        seats.Clear();
        foreach (var seat in seatingTilePositions)
        {
            List<Vector2Int> neighbours = GetAdjacentPositions(((Vector2Int)seat));
            foreach (var neighbor in neighbours)
            {
                Vector2 worldPos = GridUtils.GridToWorld(neighbor);

                Debug.Log("seat: " + seat + ", neighbour: " + worldPos);
                Debug.Log("is neighbour obstacle?: " + obstacleTilemap.isTileObstacle(worldPos) + " is neighbour seat: " + seatingTilePositions.Contains((Vector3Int)neighbor));
                if (!obstacleTilemap.isTileObstacle(worldPos) && !seatingTilePositions.Contains((Vector3Int)neighbor))
                {
                    seats.Add(new Seat
                    {
                        seatTile = seat,
                        approachTile = neighbor,
                        isOccupied = false,
                        currentCustomer = null
                    });
                    Debug.Log("New seat at: " + seat + ", neighbour at: " + worldPos);
                    break;
                }
            }
        }
    }

    private static List<Vector2Int> GetAdjacentPositions(Vector2Int position) //gets neighbour tile positions
    {
        return new List<Vector2Int>
        {
            position + Vector2Int.up,
            position + Vector2Int.down,
            position + Vector2Int.left,
            position + Vector2Int.right,
        };
    }

}

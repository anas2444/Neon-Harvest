using System.Collections;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public GameObject tilePrefab;

    public int gridWidth = 8;
    public int gridHeight = 8;
    public float tileSize = 2f;
    private GridTile[,] grid;
    public GameObject enemyPrefab;
    public float spawnDelay = 3f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BuildGrid();
    }

    void BuildGrid()
    {
        grid = new GridTile[gridWidth, gridHeight];

        // Center the grid
        Vector2 startPos = new Vector2(
            -(gridWidth / 2f) + 0.5f,
            -(gridHeight / 2f) + 0.5f
        );

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 spawnPos = new Vector2(
                    startPos.x + (x * tileSize),
                    startPos.y + (y * tileSize)
                );

                GameObject tileObj = Instantiate(
                    tilePrefab,
                    spawnPos,
                    Quaternion.identity,
                    transform
                );

                tileObj.name = "Tile_" + x + "_" + y;

                GridTile tile = tileObj.GetComponent<GridTile>();
                grid[x, y] = tile;
            }
        }
    }

    public GridTile GetTile(int x, int y)
    {
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
            return null;

        return grid[x, y];
    }
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x + 3.5f);
        int y = Mathf.RoundToInt(worldPos.y + 3.5f);

        return new Vector2Int(x, y);
    }
   
    public void GrowAllTiles()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].Grow();
                }
            }
        }
    }
    public bool AreAllTilesCorrupt()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null &&
                    grid[x, y].currentState != GridTile.TileState.Corrupt)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Manages the game grid, including tile creation, blasting, and shuffling.
/// Ensures the game board remains playable by handling deadlock situations.
/// </summary>
public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public int colorCount = 6;

    public int A_Threshold = 4;
    public int B_Threshold = 7;
    public int C_Threshold = 9;

    public GameObject tilePrefab;
    public TileSprites tileSprites;
    private Tile[,] grid;

    private Stack<Tile> floodStack = new Stack<Tile>(100);
    private bool[,] visited;

    void Start()
    {
        if (tileSprites != null) {
            tileSprites.tileAtlas = Resources.Load<SpriteAtlas>("Tiles");
            if (tileSprites.tileAtlas == null)
            {
                Debug.LogError("Sprite atlas failed to load. Check the file name.");
            }
        }

        GenerateGrid();
        CheckForDeadlock();

        // Updates the sprites of tiles based on their groups at start.
        List<Tile> allTiles = new List<Tile>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    allTiles.Add(grid[x, y]);
                }
            }
        }
        UpdateTileSprites(allTiles);
    }

    // Creates the grid of given width and height.
    void GenerateGrid()
    {
        grid = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateTile(x, y);
            }
        }
        Debug.Log("Tiles have been created.");
    }

    // Randomly creates a tile at the given coordinate.
    Tile CreateTile(int x, int y)
    {
        // For tiles to be in the middle of the screen
        Vector3 boardCenter = GetBoardCenter();
        Vector3 spawnPosition = boardCenter + new Vector3(x, y + 2, 0);

        Tile tile = TilePool.Instance.GetTile();
        tile.transform.position = spawnPosition;
        tile.Init(x, y, Random.Range(0, colorCount));

        grid[x, y] = tile;
        tile.SetNewPosition(x, y);

        return tile;
    }

    // Uses a flood-fill algorithm to find all connected tiles of the same color.
    // Returns a list of tiles that form a valid group.
    public List<Tile> GetConnectedTiles(int startX, int startY)
    {
        List<Tile> connectedTiles = new List<Tile>();

        if (!IsValidPosition(startX, startY) || grid[startX, startY] == null)
            return connectedTiles;

        int targetColor = grid[startX, startY].colorID;

        // Updates visited array according to current grid size
        if (visited == null || visited.GetLength(0) != width || visited.GetLength(1) != height)
        {
            visited = new bool[width, height];
        }
        else
        {
            // Clears previous visits
            System.Array.Clear(visited, 0, visited.Length);
        }

        // Clearing and reusing the same array for memory optimization.
        floodStack.Clear();
        floodStack.Push(grid[startX, startY]);

        // Searchs until find the group of tiles.
        while (floodStack.Count > 0)
        {
            Tile current = floodStack.Pop();
            if (current == null || visited[current.x, current.y]) continue;

            visited[current.x, current.y] = true;
            connectedTiles.Add(current);

            foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;

                if (IsValidPosition(newX, newY) && !visited[newX, newY] && grid[newX, newY] != null && grid[newX, newY].colorID == targetColor)
                {
                    floodStack.Push(grid[newX, newY]);
                }
            }
        }

        return connectedTiles;
    }

    //
    bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    // Destroys selected tiles.
    public void BlastTiles(List<Tile> tilesToBlast)
    {
        List<Tile> changedTiles = new List<Tile>();

        foreach (Tile tile in tilesToBlast)
        {
            changedTiles.Add(tile);
            TilePool.Instance.ReturnTile(tile);
            grid[tile.x, tile.y] = null;
        }

        DropTiles(changedTiles);
    }

    // Fills the gaps by dropping tiles down and replacing them with new tiles
    void DropTiles(List<Tile> changedTiles)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    for (int dropY = y + 1; dropY < height; dropY++)
                    {
                        if (grid[x, dropY] != null)
                        {
                            grid[x, y] = grid[x, dropY];
                            grid[x, dropY] = null;
                            grid[x, y].SetNewPosition(x, y);
                            changedTiles.Add(grid[x, y]);
                            break;
                        }
                    }
                }
            }
        }

        // Create new tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    Tile newTile = CreateTile(x, y);
                    changedTiles.Add(newTile);
                }
            }
        }

        UpdateTileSprites(changedTiles);
        CheckForDeadlock(); // Post-explosion deadlock control
    }

    // Updates only the sprites of the changed tiles and the tiles grouped with them for optimization.
    // Ensures that all connected tiles reflect the correct sprite variations.

    public void UpdateTileSprites(List<Tile> changedTiles)
    {
        HashSet<Tile> tilesToUpdate = new HashSet<Tile>();

        // Finds the group of each changed stone and add it to the list to be updated
        foreach (Tile tile in changedTiles)
        {
            if (tile != null)
            {
                List<Tile> group = GetConnectedTiles(tile.x, tile.y);
                foreach (Tile t in group)
                {
                    tilesToUpdate.Add(t);
                }
            }
        }

        // Updates sprites of all specified tiles
        foreach (Tile tile in tilesToUpdate)
        {
            if (tile != null)
            {
                int groupSize = GetConnectedTiles(tile.x, tile.y).Count;
                tile.UpdateSprite(groupSize);
            }
        }

        Debug.Log("Changed tile sprites have been updated!");
    }

    public void UpdateAllTileSprites()
    {
        Debug.Log("Updating all tile sprites...");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    int groupSize = GetConnectedTiles(grid[x, y].x, grid[x, y].y).Count;
                    grid[x, y].UpdateSprite(groupSize);
                }
            }
        }

        Debug.Log("All tile sprites have been updated!");
    }

    // Deadlock checks if there are any groups
    public bool IsDeadlock()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    List<Tile> group = GetConnectedTiles(x, y);
                    if (group.Count >= 2)
                    {
                        Debug.Log("There is no deadlock.");
                        return false; // No deadlock if there is at least one group.
                    }
                }
            }
        }
        Debug.Log("There is deadlock.");
        return true; // There are no blastable groups, there is deadlock!
    }

    // Handles the shuffling of tiles when no moves are available (deadlock).
    // Ensures at least a few valid groups exist after shuffling.
    public void ShuffleBoard()
    {
        Debug.Log("Deadlock detected! Smart mixing is in progress...");

        List<Tile> allTiles = new List<Tile>();

        // List all stones.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    allTiles.Add(grid[x, y]);
                    grid[x, y] = null; // Temporarily clear.
                }
            }
        }

        // Randomly select cluster points on the board (3-7).
        int clusterCount = Mathf.Clamp((width * height) / 10, 1, 6); // Less on small boards, more on large boards.
        List<Vector2Int> clusterPoints = new List<Vector2Int>();

        for (int i = 0; i < clusterCount; i++)
        {
            int randomX = Random.Range(0, width);
            int randomY = Random.Range(0, height);
            clusterPoints.Add(new Vector2Int(randomX, randomY));
        }

        // Place tiles on cluster points.
        foreach (Vector2Int cluster in clusterPoints)
        {
            if (allTiles.Count == 0) break;
            Tile clusterTile = allTiles[Random.Range(0, allTiles.Count)];
            allTiles.Remove(clusterTile);
            grid[cluster.x, cluster.y] = clusterTile;
            clusterTile.SetNewPosition(cluster.x, cluster.y);
        }

        // Place tiles of similar colors around the cluster points.
        foreach (Vector2Int cluster in clusterPoints)
        {
            Tile centerTile = grid[cluster.x, cluster.y];
            if (centerTile == null) continue;

            foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                int newX = cluster.x + dir.x;
                int newY = cluster.y + dir.y;

                if (IsValidPosition(newX, newY) && grid[newX, newY] == null && allTiles.Count > 0)
                {
                    Tile neighborTile = allTiles.Find(t => t.colorID == centerTile.colorID);
                    if (neighborTile != null)
                    {
                        allTiles.Remove(neighborTile);
                        grid[newX, newY] = neighborTile;
                        neighborTile.SetNewPosition(newX, newY);
                    }
                }
            }
        }

        // Place the remaining tiles randomly in the empty spaces.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    if (allTiles.Count > 0)
                    {
                        Tile randomTile = allTiles[Random.Range(0, allTiles.Count)];
                        allTiles.Remove(randomTile);
                        grid[x, y] = randomTile;
                        randomTile.SetNewPosition(x, y);
                    }
                    else
                    {
                        // If we run out of tiles, create a new tile and add it
                        Tile newTile = CreateTile(x, y);
                        grid[x, y] = newTile;
                    }
                }
            }
        }

        UpdateAllTileSprites();

        Debug.Log("Shuffle completed! Playable groups are created.");
    }

    public void CheckForDeadlock()
    {
        if (IsDeadlock())
        {
            StartCoroutine(DelayedShuffle());
        }
    }

    // Waits for a while for it to be understood that it is deadlock.
    private IEnumerator DelayedShuffle()
    {
        yield return new WaitForSeconds(0.5f);
        ShuffleBoard();
    }

    // Calculates the midpoint
    public Vector3 GetBoardCenter()
    {
        float centerX = (width - 1) / 2f;
        float centerY = (height - 1) / 2f;
        return new Vector3(-centerX, -centerY, 0);
    }

}

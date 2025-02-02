using System.Collections.Generic;
using UnityEngine;

public class TilePool : MonoBehaviour
{
    public static TilePool Instance;

    public GameObject tilePrefab;
    public int poolSize = 50;

    private Queue<Tile> tilePool = new Queue<Tile>();

    private void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject tileObj = Instantiate(tilePrefab);
            Tile tile = tileObj.GetComponent<Tile>();
            tileObj.SetActive(false);
            tilePool.Enqueue(tile);
        }
    }

    public Tile GetTile()
    {
        if (tilePool.Count > 0)
        {
            Tile tile = tilePool.Dequeue();
            tile.gameObject.SetActive(true);
            return tile;
        }
        else
        {
            GameObject tileObj = Instantiate(tilePrefab);
            return tileObj.GetComponent<Tile>();
        }
    }

    public void ReturnTile(Tile tile)
    {
        tile.gameObject.SetActive(false);
        tilePool.Enqueue(tile);
    }
}

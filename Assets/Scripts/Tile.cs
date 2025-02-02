using TMPro;
using UnityEngine;

/// <summary>
/// Represents a single tile in the game grid.
/// Handles its position, color, and sprite updates based on group size.
/// </summary>
public class Tile : MonoBehaviour
{
    public int x, y;
    public int colorID;
    private SpriteRenderer spriteRenderer;
    private GridManager gridManager;
    private Vector3 targetPosition;
    private float moveSpeed = 10f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gridManager = FindFirstObjectByType<GridManager>();
        targetPosition = gameObject.transform.position;
    }

    void Update()
    {
        // Slowly changes position to avoid sudden relocation for the player.
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    public void Init(int _x, int _y, int _colorID)
    {
        x = _x;
        y = _y;
        colorID = _colorID;
        UpdateSprite(1);
    }

    private void OnMouseDown()
    {
        if (gridManager != null)
        {
            // To see if it can be blastable.
            var connectedTiles = gridManager.GetConnectedTiles(x, y);
            if (connectedTiles.Count >= 2)
            {
                gridManager.BlastTiles(connectedTiles);
            }
        }
    }

    public void UpdateSprite(int groupSize)
    {
        if (gridManager == null || gridManager.tileSprites == null) return;
        if (gridManager.tileSprites.tileAtlas == null)
        {
            Debug.LogError("Sprite Atlas not loaded!");
            return;
        }

        Sprite newSprite = gridManager.tileSprites.GetSprite(
            colorID, groupSize,
            gridManager.A_Threshold,
            gridManager.B_Threshold,
            gridManager.C_Threshold
        );

        if (newSprite == null)
        {
            Debug.LogError($"No sprite found for {colorID} in atlas!");
            return;
        }

        spriteRenderer.sprite = newSprite;
    }

    public void SetNewPosition(int newX, int newY)
    {
        x = newX;
        y = newY;
        targetPosition = gridManager.GetBoardCenter() + new Vector3(newX, newY-0.2f, 0);
        spriteRenderer.sortingOrder = newY;
    }
}

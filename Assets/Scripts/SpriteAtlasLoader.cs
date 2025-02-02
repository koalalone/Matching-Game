using UnityEngine;
using UnityEngine.U2D; // For Sprite Atlas

/// <summary>
/// Loads and manages the game's Sprite Atlas at runtime.
/// Ensures all tiles use optimized sprite rendering.
/// </summary>
public class SpriteAtlasLoader : MonoBehaviour
{
    public SpriteAtlas tileAtlas;

    void Awake()
    {
        if (tileAtlas != null)
        {
            Debug.Log("Sprite Atlas successfully loaded!");
            SpriteAtlasManager.atlasRequested += RequestAtlas;
        }
    }

    private void RequestAtlas(string tag, System.Action<SpriteAtlas> callback)
    {
        if (tileAtlas != null && tag == tileAtlas.tag)
        {
            callback(tileAtlas);
        }
    }
}

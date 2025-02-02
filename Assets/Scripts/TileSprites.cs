using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D; // For Sprite Atlas

/// <summary>
/// Stores and manages tile sprites for different colors and group sizes.
/// Uses a Sprite Atlas for performance optimization.
/// </summary>
[CreateAssetMenu(fileName = "TileSprites", menuName = "Game/TileSprites")]
public class TileSprites : ScriptableObject
{
    [System.Serializable]
    public class TileSpriteSet
    {
        public string colorName;
        public string defaultSpriteName; // No longer Sprite directly, we keep the name.
        public string aGroupSpriteName;
        public string bGroupSpriteName;
        public string cGroupSpriteName;
    }

    public List<TileSpriteSet> tileSprites;
    public string atlasName = "Tiles"; // Sprite Atlas name in Resources.
    [HideInInspector] public SpriteAtlas tileAtlas;

    private void OnEnable()
    {
        // Automatically loads Sprite Atlas at game start.
        tileAtlas = Resources.Load<SpriteAtlas>(atlasName);

        if (tileAtlas == null)
        {
            Debug.LogError("Sprite Atlas failed to load! Check file name: " + atlasName);
        }
        else
        {
            Debug.Log("Sprite Atlas successfully loaded: " + atlasName);
        }
    }

    public Sprite GetSprite(int colorID, int groupSize, int aThreshold, int bThreshold, int cThreshold)
    {
        if (tileAtlas == null || colorID >= tileSprites.Count)
            return null;

        string spriteName = tileSprites[colorID].defaultSpriteName;

        if (groupSize >= cThreshold) spriteName = tileSprites[colorID].cGroupSpriteName;
        else if (groupSize >= bThreshold) spriteName = tileSprites[colorID].bGroupSpriteName;
        else if (groupSize >= aThreshold) spriteName = tileSprites[colorID].aGroupSpriteName;

        return tileAtlas.GetSprite(spriteName); // We are pulling sprites from Sprite Atlas.
    }
}

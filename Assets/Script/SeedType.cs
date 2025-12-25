using UnityEngine;

/// <summary>
/// Enum định nghĩa các loại hạt giống trong game
/// </summary>
public enum SeedType
{
    Apple,      // Táo
    Tomato,     // Cà chua
    Ornamental  // Cây cảnh
}

/// <summary>
/// Class đại diện cho một hạt giống trong inventory
/// </summary>
[System.Serializable]
public class SeedItem
{
    public SeedType seedType;
    public int quantity;
    public Sprite seedSprite;

    public SeedItem(SeedType type, int qty = 1)
    {
        seedType = type;
        quantity = qty;
    }

    public string GetSeedName()
    {
        return seedType.ToString();
    }
}


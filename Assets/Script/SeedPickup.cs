using UnityEngine;

public class SeedPickup : MonoBehaviour
{
    [Header("Cài đặt hạt giống")]
    public SeedType seedType; // Loại hạt (Apple, Tomato, Ornamental)
    public int amount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu người chơi chạm vào
        if (collision.CompareTag("Player"))
        {
            // Cộng vào kho đồ
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddSeed(seedType, amount);

                // Báo cho Manager biết 1 hạt đã biến mất khỏi map
                //if (WorldSeedManager.Instance != null)
                //    WorldSeedManager.Instance.OnSeedPickedUp();

                Destroy(gameObject);    // sau khi nhặt khi seed biến mất khỏi map
            }
        }
    }
}
using UnityEngine;

/// <summary>
/// GameManager để khởi tạo các managers cần thiết
/// Có thể gắn vào một GameObject trong scene để đảm bảo InventoryManager được tạo
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameObject inventoryManagerPrefab; // Optional: prefab của InventoryManager

    private void Awake()
    {
        // Đảm bảo InventoryManager tồn tại
        if (InventoryManager.Instance == null)
        {
            GameObject inventoryObj;
            
            if (inventoryManagerPrefab != null)
            {
                inventoryObj = Instantiate(inventoryManagerPrefab);
            }
            else
            {
                // Tạo InventoryManager mới nếu chưa có
                inventoryObj = new GameObject("InventoryManager");
                inventoryObj.AddComponent<InventoryManager>();
            }
            
            Debug.Log("[GameManager] InventoryManager initialized");
        }
    }
}


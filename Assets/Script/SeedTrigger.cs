using UnityEngine;

public class SeedTrigger : MonoBehaviour
{
    // Hàm này tự động chạy khi có vật thể khác chạm vào hạt giống
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Gọi hàm trong SeedsManager để hiện chữ +1 và cộng điểm
            if (SeedsManager.Instance != null)
            {
                SeedsManager.Instance.OnSeedPickedUp(transform.position, gameObject.name);
            }

            // Xóa hạt giống khỏi bản đồ sau khi nhặt
            Destroy(gameObject);
        }
    }
}
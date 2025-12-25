using UnityEngine;

public class TreeSpawning : MonoBehaviour
{
    // WorldSeedManager gọi tới hàm này
    public void SpawnSeed(GameObject seedPrefab)
    {
        // Tạo vị trí ngẫu nhiên quanh gốc cây (1.0f là khoảng cách văng ra)
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 1.0f;

        // Tạo hạt giống từ Prefab
        Instantiate(seedPrefab, spawnPos, Quaternion.identity);

        Debug.Log(gameObject.name + " đã sinh ra hạt giống!");
    }
}
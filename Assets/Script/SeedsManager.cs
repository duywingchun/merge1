using UnityEngine;
using UnityEngine.Tilemaps; // Thêm thư viện này
using System.Collections.Generic;

public class SeedsManager : MonoBehaviour
{
    public static SeedsManager Instance;

    [Header("Tilemap Config")]
    public Tilemap treeTilemap; // Kéo Tilemap chứa cây vào đây
    [Header("Seed type list")]
    public GameObject[] seedPrefabs; // Kéo 3 loại hạt giống từ thư mục Seed vào đây
    [Header("Spawn config")]
    public float minSpawnDelay = 5f;
    public float maxSpawnDelay = 15f;
    public int maxSeedsOnMap = 10;
    [Header("UI Feedback")]
    public GameObject floatingTextPrefab;
    // Thay đổi từ List<TreeSpawning> sang List<Vector3> để lưu tọa độ thế giới của cây
    private List<Vector3> treeWorldPositions = new List<Vector3>();
    private int currentSeedsCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        // 1. Quét tọa độ tất cả các cây trên Tilemap
        UpdateTreePositions();

        Debug.Log($"[Manager] Đã kết nối với {treeWorldPositions.Count} vị trí cây.");

        // 2. Bắt đầu chu kỳ spawn
        ScheduleNextSpawn();
    }
    public void OnSeedPickedUp(Vector3 seedPosition, string seedName)
    {
        currentSeedsCount--;

        if (floatingTextPrefab != null)
        {
            GameObject textObj = Instantiate(floatingTextPrefab, seedPosition, Quaternion.identity);
            var textMesh = textObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();

            if (textMesh != null)
            {
                string cleanName = seedName.Replace("(Clone)", "").Trim();
                textMesh.text = "+1 " + cleanName;

                if (cleanName.Contains("Apple") || cleanName.Contains("Tomato"))
                    textMesh.color = Color.green;
                else if (cleanName.Contains("Ornamental"))
                {
                    textMesh.color = Color.red;
                }
            }
        }
    }

    // Hàm quét Tilemap để tìm vị trí có gạch (cây)
    void UpdateTreePositions()
    {
        treeWorldPositions.Clear();
        if (treeTilemap == null) return;

        BoundsInt bounds = treeTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (treeTilemap.HasTile(pos))
            {
                // Chuyển tọa độ ô gạch sang tọa độ thực trong game
                Vector3 worldPos = treeTilemap.GetCellCenterWorld(pos);
                treeWorldPositions.Add(worldPos);
            }
        }
    }

    void ScheduleNextSpawn()
    {
        float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
        Invoke("ExecuteSpawn", delay);
    }

    void ExecuteSpawn()
    {
        if (treeWorldPositions.Count > 0 && currentSeedsCount < maxSeedsOnMap)
        {
            // 1. Chọn 1 vị trí cây ngẫu nhiên từ danh sách đã quét
            int randomIndex = Random.Range(0, treeWorldPositions.Count);
            Vector3 spawnPos = treeWorldPositions[randomIndex];

            // 2. Cộng thêm độ lệch để hạt nằm ở gốc cây (tùy chỉnh Y)
            spawnPos += new Vector3(0, -0.4f, 0);

            // 3. Chọn 1 loại hạt ngẫu nhiên
            int randomSeedIndex = randomidx();

            // 4. Tạo hạt giống tại vị trí đó
            GameObject newSeed = Instantiate(seedPrefabs[randomSeedIndex], spawnPos, Quaternion.identity);

            // 5. Đảm bảo hạt giống hiện lên trên (Sorting Layer)
            if (newSeed.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
            {
                sr.sortingOrder = 10;
            }

            currentSeedsCount++;
        }

        ScheduleNextSpawn();
    }
    int randomidx()
    {
        // index: 0, 1 là thường (80%)
        // index: 2 là hiếm (20%)
        int random_num = UnityEngine.Random.Range(0, 100);
        if (random_num < 80)
        {
            return UnityEngine.Random.Range(0, 2);
        }
        else
        {
            return 2;
        }
    }
}
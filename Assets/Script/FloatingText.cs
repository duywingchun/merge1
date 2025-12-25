using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1f;       // Tốc độ bay lên
    public float destroyTime = 1f;     // Thời gian biến mất (1 giây)
    private TextMeshProUGUI textMesh;
    private Color originalColor;
    private float timer;

    void Start()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        originalColor = textMesh.color;
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // Làm chữ bay lên trên
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);
        // Tính toán mờ dần (Alpha giảm dần theo thời gian)
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1, 0, timer / destroyTime);
        textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
    }
}
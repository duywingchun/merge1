using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Đối tượng nhân vật để camera theo dõi (để trống sẽ tự tìm Player)")]
    public Transform target;

    [Header("Camera Settings")]
    [Tooltip("Tốc độ di chuyển camera (2-10 là giá trị tốt, càng lớn càng nhanh)")]
    [SerializeField] private float smoothSpeed = 6f;

    [Tooltip("Khoảng cách camera với nhân vật (offset)")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    [Tooltip("Có giới hạn phạm vi di chuyển camera không?")]
    [SerializeField] private bool useBounds = false;

    [Header("Camera Bounds (nếu sử dụng giới hạn)")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;

    [SerializeField] private string playerTag;

    private Camera cam;

    void Awake()
    {
        Vector3 pos = transform.position;
        pos.z = -20f;
        transform.position = pos;
    }

    void Start()
    {
        cam = GetComponent<Camera>();

        Vector3 pos = transform.position;
        pos.z = -20f;
        transform.position = pos;
        Camera.main.orthographicSize = 10f;
    }

    void LateUpdate()
    {
        Vector3 currentPos = transform.position;
        if (currentPos.z != -20f)
        {
            currentPos.z = -20f;
            transform.position = currentPos;
        }

        // Nếu chưa có target, cố gắng tìm Player (để xử lý trường hợp Player được tạo sau khi Start() chạy)
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }

            if (player != null)
            {
                playerTag = player.tag; // Dòng này để test

                target = player.transform;
            }
            else
            {
                return;
            }
        }

        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, -20f);

        // Áp dụng giới hạn nếu có
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}

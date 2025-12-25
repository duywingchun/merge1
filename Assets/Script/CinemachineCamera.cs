
using Unity.Cinemachine;
using UnityEngine;

public class CameraTargetAssigner : MonoBehaviour
{
    // Kéo thả Virtual Camera vào trường này trong Inspector
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    // Tên (Tag hoặc tên GameObject) của Player mà bạn muốn tìm
    // Thường Player sẽ có Tag là "Player"
    [SerializeField] private string playerTag = "Player";

    void Start()
    {
        // Kiểm tra xem Virtual Camera đã được gán chưa
        if (virtualCamera == null)
        {
            // Thử tìm Component trên chính GameObject này
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        // Bắt đầu Coroutine hoặc InvokeRepeating để đợi Player xuất hiện
        // Dùng Coroutine là cách tối ưu hơn.
        StartCoroutine(WaitForPlayerAndAssign());
    }

    private System.Collections.IEnumerator WaitForPlayerAndAssign()
    {
        GameObject player = null;

        // Vòng lặp đợi Player xuất hiện
        // Thiết lập timeout để tránh vòng lặp vô tận nếu Player không bao giờ xuất hiện
        float maxWaitTime = 10f; // Chờ tối đa 10 giây
        float startTime = Time.time;

        while (player == null && (Time.time - startTime < maxWaitTime))
        {
            // Tìm đối tượng Player đầu tiên với Tag đã định
            player = GameObject.FindGameObjectWithTag(playerTag);

            // Chờ một Frame trước khi tìm lại (giảm tải cho CPU)
            yield return null;
        }

        if (player != null && virtualCamera != null)
        {
            // Gán Transform của Player tìm được vào thuộc tính Follow và LookAt của VCam
            virtualCamera.Follow = player.transform;
            virtualCamera.LookAt = player.transform;

            Debug.Log("Camera đã tìm thấy và bắt đầu theo dõi Player: " + player.name);
        }
        else
        {
            Debug.LogError("Không tìm thấy Player hoặc Virtual Camera bị thiếu sau khi chờ.");
        }
    }
}
using UnityEngine;

public class Init : MonoBehaviour
{
    private void Awake()
    {
        FixCameraPosition();
    }

    void Start()
    {
        Debug.Log("[Init] Start() called. Spawn position from TeleporterData: " + TeleporterData.spawnPosition);
        
        GameObject selectedCharacter = CharacterSelect.selectedCharacter;
        
        if (selectedCharacter == null)
        {
            Debug.LogError("[Init] ERROR: CharacterSelect.selectedCharacter is null! Cannot spawn player.");
            return;
        }
        
        Debug.Log("[Init] Selected character: " + selectedCharacter.name);
        
        // Sử dụng vị trí từ TeleporterData nếu có, nếu không dùng mặc định
        Vector3 spawnPos = TeleporterData.spawnPosition != Vector3.zero 
            ? TeleporterData.spawnPosition 
            : new Vector3(10f, 5f, 0f);
        
        Debug.Log("[Init] Spawning player at position: " + spawnPos);
        
        GameObject playerObject = Instantiate(selectedCharacter, spawnPos, Quaternion.identity);
        playerObject.name = "Player";

        if (!playerObject.CompareTag("Player"))
        {
            playerObject.tag = "Player";
        }

        Debug.Log("[Init] Player spawned successfully at: " + playerObject.transform.position);

        // Reset TeleporterData sau khi dùng
        TeleporterData.spawnPosition = Vector3.zero;

        SetupCamera(playerObject.transform);
    }

    void SetupCamera(Transform playerTransform)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindAnyObjectByType<Camera>();
        }

        if (mainCamera != null)
        {
            Vector3 camPos = new Vector3(playerTransform.position.x, playerTransform.position.y, -20f);
            mainCamera.transform.position = camPos;

            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }

            cameraFollow.SetTarget(playerTransform);
        }
    }

    void FixCameraPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera= FindAnyObjectByType<Camera>();
        }

        if (mainCamera)
        {
            Vector3 camPos = mainCamera.transform.position;
            camPos.z = -20;
            mainCamera.transform.position = camPos;
        }
    }

}

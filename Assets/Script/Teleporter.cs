using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private string targetSceneName; 
    [SerializeField] private Vector3 spawnPosition;   
    [SerializeField] private bool requireInteract = true; 

    private bool playerInRange = false;
    private GameObject player;

    void Update()
    {
        if (playerInRange && requireInteract)
        {
            // Kiểm tra Input
            if (Input.GetKeyDown(KeyCode.E))
            {
                Teleport();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Teleporter] OnTriggerEnter2D: {other.name}, Tag: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("[Teleporter] Player detected! Require Interact: " + requireInteract);
            playerInRange = true;
            player = other.gameObject;

            if (!requireInteract)
            {
                Debug.Log("[Teleporter] Auto teleporting...");
                Teleport();
            }
            else
            {
                Debug.Log("[Teleporter] Waiting for E key press...");
            }
        }
        else
        {
            Debug.LogWarning($"[Teleporter] Object {other.name} has wrong tag: {other.tag}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }

    void Teleport()
    {
        Debug.Log($"[Teleporter] Teleporting to: {targetSceneName}, Spawn at: {spawnPosition}");
        
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[Teleporter] ERROR: Target Scene Name is empty!");
            return;
        }
        
        TeleporterData.spawnPosition = spawnPosition;
        TeleporterData.targetScene = targetSceneName;

        Debug.Log($"[Teleporter] Loading scene: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }
    
    void Start()
    {
        Debug.Log($"[Teleporter] Initialized. Target: {targetSceneName}, Require Interact: {requireInteract}, Spawn: {spawnPosition}");
        
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("[Teleporter] ERROR: No Collider2D found!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError("[Teleporter] ERROR: Collider2D is not a Trigger!");
        }
        else
        {
            Debug.Log($"[Teleporter] Collider2D OK. Size: {col.bounds.size}");
        }
    }
}

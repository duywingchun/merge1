using UnityEngine;

// Script này gắn vào Canvas chứa ChatPanel để giữ nó qua tất cả scene
public class ChatCanvasManager : MonoBehaviour
{
    void Awake()
    {
        // Giữ Canvas này qua tất cả scene
        DontDestroyOnLoad(gameObject);
    }
}



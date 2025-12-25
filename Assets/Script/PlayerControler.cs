using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField] private float movespeed = 7f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;        
    private Animator animator;

    private PlayerSaveManager saveManager;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();      
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        animator = GetComponent<Animator>();
    }
    
    void Start()
    {
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
        // Tạo PlayerSaveManager trên GameObject riêng để tránh lỗi inactive
        GameObject saveManagerObj = GameObject.Find("PlayerSaveManager");
        if (saveManagerObj == null)
        {
            saveManagerObj = new GameObject("PlayerSaveManager");
            DontDestroyOnLoad(saveManagerObj);
            saveManager = saveManagerObj.AddComponent<PlayerSaveManager>();
        }
        else
        {
            saveManager = saveManagerObj.GetComponent<PlayerSaveManager>();
        }
        
        // Link Player transform để lưu vị trí
        if (saveManager != null)
        {
            // PlayerSaveManager sẽ tự lấy vị trí từ Player khi cần
            Debug.Log("[Player] PlayerSaveManager đã được setup");
        }
    }

    void Update() 
    {
        MovePlayer(); 
    }

    void MovePlayer() 
    {
        // Không cho phép di chuyển khi đang chat
        if (ChatUI.IsChatting)
        {
            rb.linearVelocity = Vector2.zero;
            if (animator != null && HasParameter(animator, "IsRun"))
            {
                animator.SetBool("IsRun", false);
            }
            return;
        }
      
        Vector2 playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        rb.linearVelocity = playerInput.normalized * movespeed;
       
        if (playerInput.x < 0)
        {
            spriteRenderer.flipX = true;  
        }
        else if (playerInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }

      
        if (playerInput != Vector2.zero)
        {
            animator.SetBool("IsRun", true);  
        }
        else
        {
            animator.SetBool("IsRun", false); 
        }

        // Kiểm tra xem Animator có tồn tại và có parameter "IsRun" không
        if (animator != null && HasParameter(animator, "IsRun"))
        {
            if (playerInput != Vector2.zero)
            {
                animator.SetBool("IsRun", true);
            }
            else
            {
                animator.SetBool("IsRun", false);
            }
        }
    }

    // Hàm helper để kiểm tra xem Animator có parameter với tên cụ thể không
    private bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}

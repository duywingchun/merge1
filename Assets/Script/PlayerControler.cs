using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField] private float movespeed = 7f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;        
    private Animator animator;

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
    }

    void Update() 
    {
        MovePlayer(); 
    }

    void MovePlayer() 
    {
      
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

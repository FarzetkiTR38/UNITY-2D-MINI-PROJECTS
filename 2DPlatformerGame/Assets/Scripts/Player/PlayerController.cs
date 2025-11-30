using System.Collections;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem; // yeni movement & input sistem için gerekli kütüphane

public class PlayerController : MonoBehaviour
{   
    [Header("Movement")]

    [SerializeField]
    float moveSpeed = 5f;
    Vector2 inputVector ;
    
    
    [Header("Jump")]
    [SerializeField] float groundDistance = .2f;
    [SerializeField] float jumpForce = 15f;
    [SerializeField] bool isGrounded;

    [SerializeField] bool canJumpDouble;
    [SerializeField] Transform groundPos;
    [SerializeField] private LayerMask groundLayer;


    GameInput gameInput;
    private Rigidbody2D rb;

    private Animator anim;

    bool canMove;

    
    void Start()
    {
        isGrounded = true;
        
        RespawnPlayer(false);
    }

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
        gameInput = GetComponent<GameInput>();
        anim = GetComponentInChildren<Animator>();
    }
    
    void Update()
    {

        if(!canMove) return;

        CheckGround();
        MoveFNC();
        FlipFNC();
        JumpFNC();
        UpdateAnimation();

    }
    

    private void MoveFNC()
    {
        inputVector = gameInput.GetMovementValue();

        rb.linearVelocity = new Vector2(inputVector.x * moveSpeed, rb.linearVelocity.y);

    }

    private void CheckGround()
    {
        isGrounded = Physics2D.Raycast(groundPos.position, Vector2.down, groundDistance, groundLayer);

        if (isGrounded)
        {
            canJumpDouble = true;
        }
    }

    private void FlipFNC()
    {
        if(inputVector.x > 0.01f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        } else if (inputVector.x < -0.01f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);    
        }
    }


    private void JumpFNC()
    {
        if (gameInput.IsJumpPressed())
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            
            }
            else if (canJumpDouble)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                canJumpDouble = false;
            }
        }
    }

    private void UpdateAnimation()
    {
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(groundPos.position, groundPos.position + Vector3.down * groundDistance);
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void RespawnPlayer(bool isFinished)
    {
        if (isFinished)
        {
            rb.gravityScale = 5f;
            canMove = true;
        }
        else
        {
            rb.gravityScale = 0f;
            canMove = false;
        }
    }

    


}

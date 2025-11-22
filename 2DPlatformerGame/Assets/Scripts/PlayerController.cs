using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem; // yeni movement & input sistem için gerekli kütüphane

public class PlayerController : MonoBehaviour
{   

    public GameInput gameInput;

    [SerializeField]
    float moveSpeed = 5f;

    Rigidbody2D rb;

    public Animator anim;

    public float jumpForce = 15f;

    
    void Start()
    {
        
    }

    private void Awake() 
    {

        rb = GetComponent<Rigidbody2D>();
        gameInput = GetComponent<GameInput>();

        anim = GetComponentInChildren<Animator>();
    
    }
    
    void Update()
    {


        Vector2 inputVector = gameInput.GetMovementValue();

        rb.linearVelocity = new Vector2(inputVector.x * moveSpeed, rb.linearVelocity.y);

        if(inputVector.x > 0.01f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        } else if (inputVector.x < -0.01f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);    
        }

        if (gameInput.IsJumpPressed())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));


    }

}

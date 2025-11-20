using UnityEngine;
using FishNet.Object;

[RequireComponent(typeof(Rigidbody2D))]
public class SimplePlayer : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    private Rigidbody2D _rb;
    private bool _isGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Sadece owner input işlesin
        if (!IsOwner) return;

        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");  
        // horizontal a/d
        // vertical s/w

        Vector2 vel = new Vector2(moveX, moveY) * moveSpeed;

        _rb.linearVelocity = vel;
    }


    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _isGrounded = false;

            // Sadece server'a haber verelim (gerekirse)
            JumpServerRpc();
        }
    }

    [ServerRpc]
    private void JumpServerRpc()
    {
        // Server güvenlik kontrolü
        // Eğer teleport / hack'ten korunmak istiyorsan buraya logic koyarsın.
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Ground"))
            _isGrounded = true;
    }


}

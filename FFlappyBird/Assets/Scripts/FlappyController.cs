using System;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlappyController : MonoBehaviour
{
    [SerializeField]
    float _velocity = 1.5f;

    [SerializeField]
    float _rotationSpeed = 10;

    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump") || (Mouse.current.leftButton.wasPressedThisFrame))
        {
            // rb.linearVelocity = new Vector2(rb.linearVelocity.x, _velocity);
            rb.linearVelocity = Vector2.up * _velocity;
        }
    }

    private void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(0, 0, rb.linearVelocity.y * _rotationSpeed);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("engel"))
        {
            GameManager.instance.GameOver();
        }
            
    }
}

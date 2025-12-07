using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float moveDistance = 4f;

    float waitTime = 0.2f;


    Rigidbody2D rb;
    Collider2D col;

    Animator anim;
    float startY;
    float halfDistance;

    bool isMoving = true;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();    
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
    }

    private void Start() 
    {
        startY = rb.position.y;

        halfDistance = moveDistance / 2;



    }

    private void FixedUpdate() 
    {

        if(!isMoving) return;

        float t = Mathf.PingPong(Time.time * moveSpeed, 1f);   

        float targetY = Mathf.Lerp(startY-halfDistance, startY+halfDistance, t);

        Vector2 targetPos = new Vector2(rb.position.x, targetY); 

        rb.MovePosition(targetPos);


    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(PlatformFallRoutine());
        }    
    }

    IEnumerator PlatformFallRoutine()
    {
        isMoving = false;
        yield return new WaitForSeconds(waitTime);

        anim.SetTrigger("Activate");

        rb.bodyType = RigidbodyType2D.Dynamic;
        
        col.enabled = false;

        rb.linearVelocity = new Vector2(0, -5f);
    }

}

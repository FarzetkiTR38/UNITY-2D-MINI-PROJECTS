using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class Trunk : MonoBehaviour
{
    [Header("Ground Settings")]
    public Transform groundCheckPoint;
    public Transform wallCheckPoint;
    public float groundCheckDistance = 0.2f;
    public float wallCheckDistance = 0.2f;
    public LayerMask groundMask;

    [SerializeField] float waitingTime = 2f;

    bool isWaiting = false;


    [SerializeField] float moveSpeed = 5f;

    int direction = 1;
    Rigidbody2D rb;

    Animator anim;

    bool isWall;
    bool isGrounded;

    [Header("Player Signing and Bullet Settings")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float playerDetectDistance = 6f;
    public LayerMask playerMask;
    public float fireCooldown = 1.5f;
    bool playerInSight = false;
    float lastFireTime = 0f;

    
    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();    
        anim = GetComponent<Animator>();
    }

    private void Update() 
    {
        if (!isWaiting)
        {
            if (playerInSight)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(moveSpeed * direction, rb.linearVelocity.y);
            }

            
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        anim.SetBool("Activate", isWaiting);
        

        isGrounded = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            groundCheckDistance,
            groundMask
        );

        isWall = Physics2D.Raycast(
            wallCheckPoint.position,
            Vector2.right,
            wallCheckDistance,
            groundMask
        );


        if ((!isGrounded || isWall) && !isWaiting)
        {

            StartCoroutine(WaitFlipRoutine());

        }
    }

    private void FixedUpdate() 
    {
        if(isWaiting) return;

        playerInSight = false;

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, Vector2.right * direction, playerDetectDistance, playerMask);

        if(hit.collider != null && hit.collider.CompareTag("Player"))
        {
            playerInSight = true;
            TryShoot();
        }       
    }

    void TryShoot()
    {
        if(Time.time < lastFireTime + fireCooldown) return;

        lastFireTime = Time.time;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        if(bullet != null)
        {
            bullet.GetComponent<TrunkBullet>().ChangeDirection(direction);
        }

        anim.SetTrigger("isAttacking");
    }

    IEnumerator WaitFlipRoutine()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitingTime);

        FlipFNC();
        isWaiting = false;
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            groundCheckPoint.position,
            groundCheckPoint.position + Vector3.down * groundCheckDistance
        );
        
        Gizmos.DrawLine(
            wallCheckPoint.position,
            wallCheckPoint.position + Vector3.right * direction * wallCheckDistance
        );
    }

    private void FlipFNC()
    {
        direction *= -1;

        Vector3 scale = transform.localScale;
        scale.x = direction; 
        transform.localScale = scale;
    }
}

using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Rigidbody2D theRB;
    public float moveSpeed;
    private float baseMoveSpeed;
    private Transform target;

    public float damage;

    public float hitWaitTime = 1f;
    public float hitCounter;

    // Slow effect
    private float slowMultiplier = 1f;
    private float slowTimer = 0f;

    void Start()
    {
        target = FindAnyObjectByType<PlayerController>().transform;
        baseMoveSpeed = moveSpeed;
    }

    void Update()
    {
        // Handle slow effect timer
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                slowMultiplier = 1f; // Reset speed
            }
        }

        // Move towards player
        float currentSpeed = baseMoveSpeed * slowMultiplier;
        theRB.linearVelocity = (target.position - transform.position).normalized * currentSpeed;

        if (hitCounter > 0)
        {
            hitCounter -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && hitCounter <= 0f)
        {
            PlayerHealthController.instance.TakeDamage(damage);
            hitCounter = hitWaitTime;
        }
    }

    /// <summary>
    /// Apply slow effect to enemy
    /// </summary>
    /// <param name="slowPercent">Amount to slow (0.5 = 50% slower)</param>
    /// <param name="duration">How long the slow lasts</param>
    public void ApplySlow(float slowPercent, float duration)
    {
        slowMultiplier = 1f - slowPercent;
        slowTimer = duration;
    }
}

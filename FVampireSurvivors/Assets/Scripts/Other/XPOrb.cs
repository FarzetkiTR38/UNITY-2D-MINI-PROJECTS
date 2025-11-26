using UnityEngine;

public class XPOrb : MonoBehaviour
{

    public static XPOrb instance;

    private void Awake() 
    {
        instance = this;
    }

    public int xpValue = 5;            // Orb'un verdiği XP miktarı
    public float attractRadius = 3f;   // Manyetik çekim mesafesi
    public float attractSpeed = 6f;    // Player'a çekilme hızı

    private Transform player;          // Player referansı
    private bool isAttracted = false;  // Çekim aktif mi?

    private void Start()
    {
        // Player'i bul (tag kullanıyorsan "Player")
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Manyetik çekim alanına girdiyse çekmeye başla
        if (distance <= attractRadius)
        {
            isAttracted = true;
        }

        // Çekim başladıysa player'a doğru hareket et
        if (isAttracted)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                attractSpeed * Time.deltaTime
            );

            // Player'a yeterince yaklaştıysa XP ver ve orb'u yok et
            if (distance < 0.2f)
            {
                player.GetComponent<PlayerExperience>().AddXP(xpValue);
                Destroy(gameObject);
            }
        }
    }
}

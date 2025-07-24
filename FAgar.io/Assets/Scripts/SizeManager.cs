using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SizeManager : MonoBehaviour
{


    public float currentScale;

    void Start()
    {
        currentScale = 2f;

        bestScore = currentScale;
    }

    public float scaleSpeed = 5f;

    // her şeyi 10x hesaplayacağız !
    // başlangıç kütlesi 2 değil 20, yem yiyince 0.1 gelmiyor 1 geliyor şeklinde.

    private bool yemiyedimi;

    void OnTriggerEnter2D(Collider2D other)
    {
        currentScale += .1f;

        GameManager.instance.SpawnFood();

        Destroy(other.gameObject);

        yemiyedimi = true;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(currentScale, currentScale, 1), Time.deltaTime * scaleSpeed);
        // bir anda değil animasyonlu bir şekilde büyümesini sağlıyor
        if (yemiyedimi)
        {
            gameManager.massText.text = "Mass:" + currentScale * 10;

            yemiyedimi = false;
        }
        FixUI();
        MassMelting();
    }


    GameManager gameManager;

    private float azalacakMass;

    private float massTimer = 0;

    private float bestScore;

    void Awake()
    {
        gameManager = Object.FindAnyObjectByType<GameManager>();
    }

    public void MassMelting()
    {
        if (massTimer >= 1)
        {
            azalacakMass = currentScale / 100;

            currentScale -= azalacakMass;

            massTimer = 0;
        }
        else
        {
            massTimer += Time.deltaTime;
        }

    }

    public void FixUI()
    {
        gameManager.massText.text = "Mass:" + Mathf.Floor(currentScale * 10);

        if (currentScale > bestScore)
        {
            bestScore = currentScale;

            gameManager.scoreText.text = "Score:" + Mathf.Floor(bestScore * 10);
        }


    }

    // buraya 25dk falan uğraştım.. massmelting ve fixui gamemanager'in içindeydi currentscale değeri bi 15 bi 21 oluyordu ... fonksiyonları buraya taşıyınca gamemanagerdeki tmp'ye de burdan erişince düzeldi.




}

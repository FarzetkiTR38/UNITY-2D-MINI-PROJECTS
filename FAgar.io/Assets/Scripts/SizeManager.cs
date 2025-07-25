using UnityEngine;

public class SizeManager : MonoBehaviour
{
    public float currentScale = 2f;
    public float scaleSpeed = 5f;

    private bool yemiyedimi;
    private GameManager gameManager;

    private CellManager cellManager;

    private float azalacakMass;
    private float massTimer = 0;
    private float bestScore;

    void Awake()
    {
        gameManager = Object.FindAnyObjectByType<GameManager>();
        cellManager = Object.FindAnyObjectByType<CellManager>();
        bestScore = currentScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            currentScale += .1f;
            GameManager.instance.SpawnFood();
            Destroy(other.gameObject);
            yemiyedimi = true;
        }
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(currentScale, currentScale, 1), Time.deltaTime * scaleSpeed);

        if (yemiyedimi)
        {
            gameManager.massText.text = "Mass: " + Mathf.Floor(currentScale * 10);
            yemiyedimi = false;
        }

        FixUI();
        MassMelting();
    }

    void MassMelting()
    {
        if (massTimer >= 1)
        {
            azalacakMass = currentScale / 100f;
            currentScale -= azalacakMass;
            massTimer = 0;
        }
        else
        {
            massTimer += Time.deltaTime;
        }
    }

    void FixUI()
    {
        gameManager.massText.text = "Mass: " + Mathf.Floor(currentScale * 10);
        if (currentScale > bestScore)
        {
            bestScore = currentScale;
            gameManager.scoreText.text = "Score: " + Mathf.Floor(bestScore * 10);
        }

        gameManager.pingText.text = "Ping: " + gameManager.ping + "<color=green> ms </color>";
        gameManager.fpsText.text = "Fps: " + Mathf.Floor(gameManager.fps) + "<color=blue> fps </color>";
        gameManager.serverResetText.text = "Server Reset: " + Mathf.Floor(gameManager.serverShutDownTimer_dakika) + ":" + Mathf.Floor(gameManager.serverShutDownTimer_saniye);
        gameManager.cellText.text = "Cells: " + cellManager.cells.Count.ToString() + " / " + cellManager.maxCells.ToString(); 
    }
}

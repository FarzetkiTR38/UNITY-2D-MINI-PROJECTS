using UnityEngine;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Player Spawning")]
    public GameObject player; // Oyuncu prefabı (Photon ile spawnlanacak)
    public GameObject cam;

    [Header("Food Settings")]
    public static GameManager instance;
    public GameObject foodPrefab;

    public Vector2 xRange;
    public Vector2 yRange;

    [Header("Stats")]
    public TMP_Text massText;
    public TMP_Text scoreText;
    public TMP_Text pingText;
    public TMP_Text fpsText;
    public TMP_Text cellText;

    [Header("ServerDownTime")]
    public TMP_Text serverResetText;

    [Header("-------------")]
    SizeManager sizeManager;

    public int ping;
    float deltaTime = 0.0f;
    public float fps;

    public float serverShutDownTimer = 3600f;
    public float serverShutDownTimer_dakika;
    public float serverShutDownTimer_saniye;

    void Awake()
    {
        instance = this;
        sizeManager = Object.FindAnyObjectByType<SizeManager>();
    }

    void Start()
    {
        for (int i = 0; i < 2500; i++)
        {
            SpawnFood();
        }
    }

    void Update()
    {
        ping = PhotonNetwork.GetPing();
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fps = 1.0f / deltaTime;

        serverShutDownTimer -= Time.deltaTime;
        serverShutDownTimer_dakika = serverShutDownTimer / 60f;
        serverShutDownTimer_saniye = serverShutDownTimer % 60f;
    }

    public void SpawnFood()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(xRange.x, xRange.y),
            Random.Range(yRange.x, yRange.y),
            1f
        );

        GameObject _food = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
        _food.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0f, 1f, 0.9f, 1f, 0.9f, 1f);
    }

    public void SpawnPlayer()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(xRange.x, xRange.y),
            Random.Range(yRange.x, yRange.y),
            1f
        );

        // Photon ile oyuncu prefabını spawnla
        GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPosition, Quaternion.identity);

        // Kamera takibi başlat
        cam.SetActive(true);
        cam.GetComponent<CameraController>().target = _player.transform;

        // Oyuncu scriptlerini aktif et
        _player.GetComponent<SizeManager>().enabled = true;
        _player.GetComponent<Movement>().enabled = true;

        // CellManager’a hücreyi bildir
        CellManager cellManager = FindAnyObjectByType<CellManager>();
        if (cellManager != null)
        {
            cellManager.RegisterInitialCell(_player);
        }
    }
}

using UnityEngine;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviour
{

    [Header("Player Spawninng")]
    public GameObject player;
    public GameObject cam;



    [Header("Food Settings")]
    public static GameManager instance;
    public GameObject foodPrefab;


    public Vector2 xRange;
    public Vector2 yRange;

    void Start()
    {
        for (int i = 0; i < 2500; i++)
        {
            SpawnFood();
        }
    }


    public void SpawnFood()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(xRange.x, xRange.y), Random.Range(yRange.x, yRange.y), 1);

        GameObject _food = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);

        _food.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0f, 1f, 0.9f, 1f, 0.9f, 1f);
    }

    public void SpawnPlayer()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(xRange.x, xRange.y), Random.Range(yRange.x, yRange.y), 1);

        GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPosition, Quaternion.identity);

        cam.SetActive(true);
        cam.GetComponent<CameraController>().target = _player.transform;

        _player.GetComponent<SizeManager>().enabled = true;
        _player.GetComponent<Movement>().enabled = true;
    }



    public TMP_Text massText;
    public TMP_Text scoreText;

    SizeManager sizeManager;

    private void Awake()
    {

        instance = this;

        sizeManager = Object.FindAnyObjectByType<SizeManager>();
    }




}

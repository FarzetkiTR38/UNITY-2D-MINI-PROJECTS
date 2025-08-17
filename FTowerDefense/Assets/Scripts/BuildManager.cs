using UnityEngine;

public class BuildManager : MonoBehaviour
{

    public static BuildManager instance;



    [SerializeField]
    GameObject[] towerPrefabs;


    int selectedTower = 0;


    void Awake()
    {
        instance = this;
    }

    public GameObject GetSelectedTower()
    {
        return towerPrefabs[selectedTower];
    }

}

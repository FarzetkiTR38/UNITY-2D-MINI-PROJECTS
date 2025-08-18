using UnityEngine;

public class BuildManager : MonoBehaviour
{

    public static BuildManager instance;


    /*
        [SerializeField]
        GameObject[] towerPrefabs;
    */
    [SerializeField]
    Tower[] towers;

    int selectedTower = 0;


    void Awake()
    {
        instance = this;
    }

    public Tower GetSelectedTower()
    {
        return towers[selectedTower];
    }

    public void SetSelectedTower(int _selectedTower)
    {
        selectedTower = _selectedTower;
    }

}

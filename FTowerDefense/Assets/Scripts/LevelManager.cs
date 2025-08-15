using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public Transform startPoint;
    public Transform[] path;

    void Awake()
    {
        instance = this;
    }
}

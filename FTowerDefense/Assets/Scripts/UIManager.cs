using UnityEngine;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;

    bool isHoveringUI;

    void Awake()
    {
        instance = this;
    }

    public void SetHoveringState(bool state)
    {
        isHoveringUI = state;
    }

    public bool IsHoveringUI()
    {
        return isHoveringUI;   
    } 




}

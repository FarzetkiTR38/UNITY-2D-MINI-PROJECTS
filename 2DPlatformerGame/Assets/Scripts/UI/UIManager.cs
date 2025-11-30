using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fruitText;

    public static UIManager instance;

    private void Awake() 
    {
        instance = this;
    }

    public void UpdateText(int fruitCount)
    {
        fruitText.text = fruitCount.ToString();
    } 
}

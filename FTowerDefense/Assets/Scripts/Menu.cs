using UnityEngine;
using TMPro;
public class Menu : MonoBehaviour
{

    // referanslar
    [SerializeField]
    TextMeshProUGUI currencyUI;

    [SerializeField]
    Animator anim;

    bool isMenuOpen = true;

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        anim.SetBool("MenuOpen", isMenuOpen);
        //print("isMenuOpen" + isMenuOpen);

    }

    void OnGUI()
    {
        currencyUI.text = LevelManager.instance.currency.ToString();
    }

    public void SetSelected()
    {
        //,
    }

}

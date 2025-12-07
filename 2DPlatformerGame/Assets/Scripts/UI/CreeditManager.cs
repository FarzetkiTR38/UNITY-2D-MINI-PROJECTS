using Unity.VisualScripting;
using UnityEngine;

public class CreeditManager : MonoBehaviour
{
    [SerializeField] RectTransform endPanel;
    [SerializeField] float moveSpeed = 2f;
    
    
    private void Update() 
    {
        endPanel.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;    

        if(endPanel.anchoredPosition.y > 900f)
        {
            // ana menüye döndüreceğiz
        }   
    }




}

using Unity.VisualScripting;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    Rigidbody2D rb;

    public float startForce = 5f;

    public bool isRight = true;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() 
    {
        if(isRight)
        rb.AddForce(Vector2.right * startForce, ForceMode2D.Impulse);    
        else if (!isRight)
        rb.AddForce(Vector2.left * startForce, ForceMode2D.Impulse);    
    }




}

using UnityEngine;

public class ScoreUpController : MonoBehaviour
{
    public float speed;


    void Update()
    {
        transform.Translate(Vector2.down * Time.deltaTime * speed);
    }
}

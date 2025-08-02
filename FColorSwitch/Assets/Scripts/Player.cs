using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {

	public float jumpForce = 10f;

	public Rigidbody2D rb;
	public SpriteRenderer sr;

	public string currentColor;

	public Color colorCyan;
	public Color colorYellow;
	public Color colorMagenta;
	public Color colorPink;

	void Start ()
	{
		SetRandomColor();
	}
	
	void Update () {
		if (Input.GetButtonDown("Jump") || Input.GetMouseButtonDown(0))
		{
			rb.linearVelocity = Vector2.up * jumpForce;
		}
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.tag == "ColorChanger")
		{
			SetRandomColor();
			Destroy(other.gameObject);
			return;
		}

		if (other.tag != currentColor)
		{
			Debug.Log("GAME OVER!");
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}

	void SetRandomColor()
	{
        int index = Random.Range(0, 4);

        if (sr.color == colorCyan)
        {
            do
            {
                index = Random.Range(0, 4);
            } while (index == 0);
        }
        else if(sr.color == colorYellow)
        {
            do
            {
                index = Random.Range(0, 4);
            } while (index == 1);
        }
        else if(sr.color == colorMagenta)
        {
            do
            {
                index = Random.Range(0, 4);
            } while (index == 2);
        }
        else if(sr.color == colorPink)
        {
            do
            {
                index = Random.Range(0, 4);
            } while (index == 3);
        }

        
		switch (index)
        {
            case 0:
                currentColor = "Cyan";
                sr.color = colorCyan;
                break;
            case 1:
                currentColor = "Yellow";
                sr.color = colorYellow;
                break;
            case 2:
                currentColor = "Magenta";
                sr.color = colorMagenta;
                break;
            case 3:
                currentColor = "Pink";
                sr.color = colorPink;
                break;
        }
	}
}

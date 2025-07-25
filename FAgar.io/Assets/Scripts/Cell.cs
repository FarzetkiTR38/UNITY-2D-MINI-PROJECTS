using UnityEngine;

public class Cell : MonoBehaviour
{
    private float mass;
    private Rigidbody2D rb;

    SizeManager sizeManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sizeManager = Object.FindAnyObjectByType<SizeManager>();
    }

    public void SetMass(float newMass)
    {
        Debug.Log("SetMass called! New mass: " + newMass);
        mass = newMass;
        UpdateScale();
    }

    public float GetMass()
    {
        return mass;
    }

    void UpdateScale()
    {
        float scale = Mathf.Sqrt(mass) / 5f;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    void Update()
    {
        mass = sizeManager.currentScale;
    }
    
}

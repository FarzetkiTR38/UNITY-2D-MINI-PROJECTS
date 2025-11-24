using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{

    public Transform sprite;

    public float animationSpeed = 1;

    public float minSize, maxSize;

    private float activeSize;



    void Start()
    {
        activeSize = maxSize;

        animationSpeed = animationSpeed * Random.Range(1.75f, 1.25f);
    }

    void Update()
    {
        sprite.localScale = Vector3.MoveTowards(sprite.localScale, Vector3.one * activeSize, animationSpeed * Time.deltaTime);

        if(sprite.localScale.x == activeSize)
        {
            if(activeSize == maxSize)
            {
                activeSize = minSize;
            }
            else
            {
                activeSize = maxSize;
            }
        }
    }
}

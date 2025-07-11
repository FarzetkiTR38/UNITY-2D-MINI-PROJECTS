using UnityEngine;

public class CameraController : MonoBehaviour
{


    [SerializeField]
    Transform hedefTransform;

    [SerializeField]
    float minY, maxY;

    Vector2 sonPos;

    [SerializeField]
    Transform altZemin;

    [SerializeField]
    Transform ortaZemin;

    private void Start()
    {
        sonPos = transform.position;
    }

    private void Update()
    {
        KamerayiSinirlaFNC();
        ZeminleriHareketEttirFNC();
    }

    void KamerayiSinirlaFNC()
    {
        transform.position = new Vector3(hedefTransform.position.x,
        Mathf.Clamp(hedefTransform.position.y, minY, maxY),
        transform.position.z);

        /*        if (hedefTransform.position.y > maxY)
                {
                    transform.position = new Vector3(hedefTransform.position.x, maxY, transform.position.z);
                }
                if (hedefTransform.position.y < minY)
                {
                    transform.position = new Vector3(hedefTransform.position.x, minY, transform.position.z);
                }
                // bu da olur ama bunun yerine Mathf.Clamp fonksiyonunu kullancağız.
        */
    }

    void ZeminleriHareketEttirFNC()
    {
        Vector2 aradakiMiktar = new Vector2(transform.position.x - sonPos.x, transform.position.y - sonPos.y);

        altZemin.position += new Vector3(aradakiMiktar.x, aradakiMiktar.y, 0f);
        ortaZemin.position += new Vector3(aradakiMiktar.x, aradakiMiktar.y, 0f)*.5f;

        sonPos = transform.position;
    }
}

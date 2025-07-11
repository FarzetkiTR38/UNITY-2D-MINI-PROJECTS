using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField]
    float effectSuresi;


    private void Start()
    {
        Destroy(gameObject, effectSuresi);
    }
}

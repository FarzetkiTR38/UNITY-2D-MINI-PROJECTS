using Mono.Cecil.Cil;
using UnityEngine;

public class SesController : MonoBehaviour
{
    public static SesController instance;

    public AudioSource[] sesEffectleri;

    public AudioSource[] muzikler;

    private void Awake()
    {
        instance = this;
    }

    public void SesEffectiCikar(int hangiSes)
    {
        sesEffectleri[hangiSes].Stop();
        sesEffectleri[hangiSes].Play();
    }

    public void KarisikSesEffectiCikar(int hangiSes)
    {
        sesEffectleri[hangiSes].Stop();

        sesEffectleri[hangiSes].pitch = Random.Range(0.8f, 1.3f);

        sesEffectleri[hangiSes].Play();
    }




}

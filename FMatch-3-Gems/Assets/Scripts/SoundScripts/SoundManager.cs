using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;



    public AudioSource mucevherSesi, patlamaSesi, oyunBittiSesi;


    void Awake()
    {
        instance = this;
    }

    public void MucevherSesiCikar()
    {
        mucevherSesi.Stop();

        mucevherSesi.pitch = Random.Range(0.8f, 1.2f);

        mucevherSesi.Play();
    }

    public void PatlamaSesiCikar()
    {
        patlamaSesi.Stop();

        patlamaSesi.pitch = Random.Range(0.8f, 1.2f);

        patlamaSesi.Play();
    }

    public void OyunBittiSesiCikar()
    {

        oyunBittiSesi.Play();
    }
    






}

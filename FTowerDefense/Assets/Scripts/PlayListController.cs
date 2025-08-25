using UnityEngine;

public class PlayListController : MonoBehaviour
{
    public static PlayListController instance;

    public AudioClip[] playList;        // Çalınacak müzikler

    public AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }
    
    int currentIndex = 0;

    void Start()
    {
        if (playList.Length > 0)
        {
            PlayMusic();
        }
    }

    void Update()
    {
        // Eğer şarkı bittiğinde yeni bir şarkı çalmasını istiyorsak:
        if (!audioSource.isPlaying)
        {
            NextMusic();
        }
    }

    void PlayMusic()
    {
        audioSource.clip = playList[currentIndex];
        audioSource.Play();
    }

    void NextMusic()
    {
        currentIndex++;
        if (currentIndex >= playList.Length)
        {
            currentIndex = 0; // Baştan başlasın
        }

        PlayMusic();
    }

    
}






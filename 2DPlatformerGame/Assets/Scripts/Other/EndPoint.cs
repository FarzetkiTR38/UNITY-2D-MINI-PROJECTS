using MaskTransitions;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPoint : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] bool isActivated = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(isActivated) return;

        PlayerController playerController = other.GetComponent<PlayerController>();

        if(playerController != null)
        {
            isActivated = true;

            anim.SetTrigger("Activate");

            TransitionManager.Instance.LoadLevel("MainScene2");

        }
    }

}

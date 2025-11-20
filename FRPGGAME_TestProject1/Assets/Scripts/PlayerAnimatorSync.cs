using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public enum AnimState
{
    Idle,
    Run,
    Jump,
    Hurt
}

public class PlayerAnimatorSync : NetworkBehaviour
{
    [Header("References")]
    public SpriteRenderer sr;
    public Animator animator;

    // V4 STİLİ: SyncVar<T> kullanıyoruz
    private readonly SyncVar<AnimState> _animState = new SyncVar<AnimState>();
    private readonly SyncVar<bool> _facingRight = new SyncVar<bool>();

    public bool isGrounded = false;

    private void Awake()
    {
        // SyncVar değişince çağrılacak callback'lere abone oluyoruz
        _animState.OnChange += OnAnimStateChanged;
        _facingRight.OnChange += OnFacingRightChanged;
    }

    private void OnDestroy()
    {
        // İyi pratik: abonelikleri kaldır
        _animState.OnChange -= OnAnimStateChanged;
        _facingRight.OnChange -= OnFacingRightChanged;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        HandleMovementInput();
        HandleJumpInput();
    }

    private void HandleMovementInput()
    {
        float move = Input.GetAxisRaw("Horizontal");

        // ⭐ Animator'a move parametresini set ETMEK ZORUNDASIN
        animator.SetFloat("move", Mathf.Abs(move));

        // Animasyon state sync
        if (move != 0)
            CmdSetAnim(AnimState.Run);
        else
            CmdSetAnim(AnimState.Idle);

        // Flip sync
        if (move > 0) CmdSetFlip(true);
        if (move < 0) CmdSetFlip(false);
    }


    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isGrounded = false; // jump → havadayız
            animator.SetBool("isGrounded", false);  // ANİMATORA BİLDİR
            CmdSetAnim(AnimState.Jump);
        }
    }


    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("isGrounded", true);   // ANİMATORA BİLDİR
        }
    }



    // ===== ServerRpc'ler =====

    [ServerRpc]
    private void CmdSetAnim(AnimState s)
    {
        // SyncVar<T> kullanırken .Value ile değeri set ediyorsun
        _animState.Value = s;
    }

    [ServerRpc]
    private void CmdSetFlip(bool v)
    {
        _facingRight.Value = v;
    }

    // ===== SyncVar OnChange callback'leri =====
    // İmza: (prev, next, asServer)

    private void OnAnimStateChanged(AnimState prev, AnimState next, bool asServer)
    {
        if (animator != null)
            animator.Play(next.ToString());
    }

    private void OnFacingRightChanged(bool prev, bool next, bool asServer)
    {
        if (sr != null)
            sr.flipX = !next; // true = sağ, flipX false olsun
    }


}

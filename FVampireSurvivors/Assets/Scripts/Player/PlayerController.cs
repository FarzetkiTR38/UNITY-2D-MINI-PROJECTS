using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;

    public Animator anim;



    void Update()
    {

        Vector3 moveInput = new Vector3(0f,0f,0f);

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        moveInput.Normalize();
        
        transform.position += moveInput * moveSpeed * Time.deltaTime;
        
        if(moveInput != Vector3.zero)
        {
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }

    }
}

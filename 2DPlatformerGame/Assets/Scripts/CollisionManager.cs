using Unity.VisualScripting;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            // zemine temas ettiğinde çalışacak
            print("CompareTag");
            
        }

        if(other.gameObject.tag == "Ground")
        {
            print(".tag");
        }    
    }

    
    private void OnCollisionExit2D(Collision2D other) 
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            // zeminden teması kesildiğinde çalışacak
        }    

    }

    private void OnCollisionStay2D(Collision2D other) 
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            // zeminle teması olduğu sürece çalışacak
        }    

    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            // zeminin içine girdiğinde çalıştı
            Destroy(other.gameObject);
        } 
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            // zeminin içinden çıktığında çalıştı
        } 
    }

    private void OnTriggerStay2D(Collider2D other) 
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            // zeminin içindeyken çalıştı
        } 
    }

}

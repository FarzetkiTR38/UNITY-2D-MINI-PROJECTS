using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SawTrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform sawTransform;
    [SerializeField] Transform pathHolder;
    
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float waitTime = 1f;

    Transform[] wayPoint;
    int currentIndex = 0;
    int direction = 1; // pozisyonların artış ya da azalış değeri

    bool isWaiting = false;

    Animator anim;

    private void Awake() 
    {
        wayPoint = new Transform[pathHolder.childCount];    

        for (int i = 0; i < pathHolder.childCount; i++)
        {
            wayPoint[i] = pathHolder.GetChild(i);
        }

        anim = sawTransform.GetComponent<Animator>();
    } 

    private void Start() 
    {

        sawTransform.position = wayPoint[0].position;

    }

    private void Update() 
    {
        if (!isWaiting)
        {
            MoveBetweenPoint();
        }
        
    }
    
    void MoveBetweenPoint()
    {
        anim.SetBool("Activate", true);

        if(direction == 1)
        {
            sawTransform.GetComponent<SpriteRenderer>().flipX = true;
    
        }
        else if(direction == -1)
        {
            sawTransform.GetComponent<SpriteRenderer>().flipX = false;
    
        }

        sawTransform.position = Vector3.MoveTowards(sawTransform.position, wayPoint[currentIndex].position, moveSpeed * Time.deltaTime);

        if(Vector3.Distance(sawTransform.position, wayPoint[currentIndex].position) < 0.0001f)
        {

            if(currentIndex == 0 || currentIndex == wayPoint.Length - 1)
            {
                StartCoroutine(WaitThenRoutine());
            }
            else
            {
                currentIndex += direction;
            }


            // currentIndex++;

            // if(currentIndex == wayPoint.Length)
            // {
            //     currentIndex = 0;
            // }
        }
    }

    IEnumerator WaitThenRoutine()
    {
        isWaiting = true;
        anim.SetBool("Activate", false);

        yield return new WaitForSeconds(waitTime);

        if(currentIndex == 0)
        {
            direction = 1;
        }
        else if(currentIndex == wayPoint.Length-1)
        {
            direction = -1;
        }

        currentIndex += direction;

        isWaiting = false;
    }










}

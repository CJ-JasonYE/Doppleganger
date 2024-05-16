using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutDown : MonoBehaviour
{
    public static bool isPuttingDown;
    private Animator animator;


    private void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    private void StartPutDown()
    {
        isPuttingDown = true;
    }

    private void FinishPutDown()
    {
        PutDown.isPuttingDown = false;
        animator.SetBool("PutDown", false);
    }

    

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    private Animator animator;
    public static bool isLeavingTrigger;
    public bool doorAniComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReverseDoorAnimation()
    {
        //if (isLeavingTrigger)
        //{
        //    animator.speed = -1f;
        //}
        //else
        //{
        //    animator.speed = 1f;
        //}
    }

    public bool DoorAniComplete()
    {
        return doorAniComplete = true;
    }

}

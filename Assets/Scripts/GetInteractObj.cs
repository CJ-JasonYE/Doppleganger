using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetInteractObj : MonoBehaviour
{
    private GameObject interactObj;
    public GameObject InteractObj { get { return interactObj; } }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Wheel"))
        {
            if (interactObj == null)
            {
                interactObj = collision.gameObject;
            }

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Wheel"))
        {
            if (interactObj!=null)
            {
                interactObj = null;
            }
            
        }
    }
}

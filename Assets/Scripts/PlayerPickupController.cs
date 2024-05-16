using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickupController : MonoBehaviour
{
    public float liftOffsetY = 1.35f;
    public float pickupRange = 1f;

    public GameObject liftedBox = null;

    private Vector3 pickupOriginalPosition;

    private void Update()
    {

    }

    public void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);

        foreach (var hit in hits)
        {
            if (hit.tag == "Box")
            {
                pickupOriginalPosition = hit.gameObject.transform.position;
                liftedBox = hit.gameObject;
                liftedBox.transform.parent = transform;
                liftedBox.transform.localPosition = new Vector3(0, liftOffsetY, 0);
                break;
            }
        }
        
    }
    
    public void DropBox()
    {
        liftedBox.transform.position = this.GetComponent<PlayerController>().PutDir * Vector2.right * 1.4f + (Vector2)transform.position;
        liftedBox.transform.parent = null;
        liftedBox = null;
    }

    public void RestBoxPosition()
    {
        if (liftedBox != null)
        {
            liftedBox.transform.parent = null;
            liftedBox.transform.position = pickupOriginalPosition;
            liftedBox = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Vector2.right *
            this.GetComponent<PlayerController>().PutDir * 2.1f);
    }
}

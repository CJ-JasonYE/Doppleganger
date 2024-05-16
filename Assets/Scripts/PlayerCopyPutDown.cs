using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCopyPutDown : MonoBehaviour
{
    private PlaybackController playerCopy;
    private Animator playerCopyanimator;
    public static bool playerCopyisPuttingDown;

    // Start is called before the first frame update
    void Start()
    {
        playerCopy = GameObject.Find("rpp2_MC").GetComponent<PlaybackController>();
        if (playerCopy.playerCopyGameobject != null)
        {
            playerCopyanimator = playerCopy.playerCopyGameobject.GetComponent<Animator>();
        }
    }

    private void CopyStartPutDown()
    {
        playerCopyisPuttingDown = true;
    }

    private void CopyFinishPutDown()
    {
        playerCopyisPuttingDown = false;
        playerCopyanimator.SetBool("PutDown", false);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class PlaybackController : MonoBehaviour
{
    [SerializeField] GameObject playerCopyPrefab;
    [SerializeField] private float horizontalMoveSpeed = 5f;

    public GameObject playerCopyGameobject;
    private Rigidbody2D playerCopyRigidbody;

    private bool isPlaying = false;
    private int activeCoroutinesCount = 0;

    // Lift
    public float liftOffsetY = 1.35f;
    public float pickupRange = 1f;
    public float jumpForce;

    public GameObject liftedBox = null;
    private bool isGrounded;
    private LayerMask groundLayer;
    private Animator playerCopyAnimator;
    PlayerController playerController;

    private int putDir;
    private bool isHoldWalk = false;
    bool isInteracting;
    private GameObject interactObject;
    bool isRotating;
    Coroutine wheelCoroutine;
    float elapsedRotationTime = 0;
    float rotationDuration = 5;
    Animator doorAni;

    private Quaternion initialWheelRotation;

    private bool wheel2Rotated = false;
    public event Action<bool> Wheel2Rotated;

    private AnimatorController aniController;

    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        playerController=GetComponent<PlayerController>();
        doorAni = GameObject.Find("RPP2_door_01_f1")?.GetComponent<Animator>();
        if (doorAni != null)
        {
            aniController = doorAni.GetComponent<AnimatorController>();
        }
        
        //interactObject = GameObject.FindGameObjectWithTag("Wheel").gameObject;
        //initialWheelRotation = interactObject.transform.rotation;
    }

    private void Update()
    {
        if (isPlaying == false && Input.GetKeyDown(KeyCode.Return))
        {
            if (playerCopyGameobject != null)
            {
                Destroy(playerCopyGameobject);
                liftedBox = null;
            }

            isPlaying = true;

            activeCoroutinesCount = PlayerRecorder.pairedActions.Count;

            playerCopyGameobject = Instantiate(playerCopyPrefab, PlayerRecorder.originalPosition, Quaternion.identity);
            playerCopyRigidbody = playerCopyGameobject.GetComponent<Rigidbody2D>();
            playerCopyAnimator= playerCopyRigidbody.GetComponent<Animator>();
            putDir = playerController.PutDir;

            foreach (var pairedAction in PlayerRecorder.pairedActions)
            {   
                if (pairedAction.key == "Wheel")
                {
                    StartCoroutine(ExecuteWheelAction(pairedAction));
                }
                else
                {
                    StartCoroutine(ExecutePairedAction(pairedAction));
                }
            }

            foreach (var singleAction in PlayerRecorder.singleActions)
            {
                StartCoroutine(ExecuteSingleAction(singleAction));
            }

            //Other Side Animation
            SideAni();
        }

    }

    private IEnumerator ExecutePairedAction(PairedAction pairedAction)
    {
        float endTime = Time.time + pairedAction.releaseTime;

        yield return new WaitForSeconds(pairedAction.pressTime);

        while (Time.time < endTime)
        {
            //�ƶ�����
            if (pairedAction.key == "A")
            {
                playerCopyRigidbody.velocity = new Vector2(-horizontalMoveSpeed, playerCopyRigidbody.velocity.y);
                putDir = -1;
                playerCopyAnimator.SetBool("StartWalk", true);
            }
            else if (pairedAction.key == "D")
            {
                playerCopyRigidbody.velocity = new Vector2(horizontalMoveSpeed, playerCopyRigidbody.velocity.y);
                putDir = 1;
                playerCopyAnimator.SetBool("StartWalk", true);
            }

            //Walk Animation
            WalkAni();
            yield return null;
        }
        playerCopyRigidbody.velocity = new Vector2(0f, playerCopyRigidbody.velocity.y);

        activeCoroutinesCount--;
        if (activeCoroutinesCount <= 0)
        {
            isPlaying = false;
        }
    }

    private IEnumerator ExecuteWheelAction(PairedAction pairedAction)
    {
        yield return new WaitForSeconds(pairedAction.pressTime);

        if (doorAni!=null)
        {
            doorAni.speed = 1f;
            doorAni.Play("Open");
        }

        if (interactObject == null)
        {
            interactObject = playerCopyGameobject.GetComponent<GetInteractObj>().InteractObj;
            initialWheelRotation = interactObject.transform.rotation;
        }
        
        StartCoroutine(StartWheel());

        yield return new WaitForSeconds(pairedAction.releaseTime - pairedAction.pressTime);
       
        if (doorAni != null)
        {
            doorAni.speed = 0f;
            doorAni.Play("Open", 0, 0);
        }
        
        wheel2Rotated = false;
        if (Wheel2Rotated != null)
        {
            Wheel2Rotated(wheel2Rotated);
        }
        StopCoroutine(StartWheel());
        StartCoroutine(RotateBackToInitialRotation());

        activeCoroutinesCount--;
        if (activeCoroutinesCount <= 0)
        {
            isPlaying = false;
        }
    }

    private void WalkAni()
    {
        if (!isHoldWalk)
        {
            if (playerCopyRigidbody.velocity.x > 0)
            {
                playerCopyAnimator.SetFloat("Walk", 1);
            }
            else if (playerCopyRigidbody.velocity.x < 0)
            {
                playerCopyAnimator.SetFloat("Walk", -1);
            }
            else
            {
                playerCopyAnimator.SetFloat("Walk", 0);
            }
        }
        else
        {
            if (playerCopyRigidbody.velocity.x > 0)
            {
                playerCopyAnimator.SetFloat("Dir", 1);
            }
            else if (playerCopyRigidbody.velocity.x < 0)
            {
                playerCopyAnimator.SetFloat("Dir", -1);
            }
            else
            {
                isHoldWalk = false;
            }
        }
    }

    private IEnumerator ExecuteSingleAction(SingleAction singleAction)
    {
        yield return new WaitForSeconds(singleAction.pressTime);

        //��������
        print("bb");
        print("bb");
            if (singleAction.key == "F" /*&& PlayerCopyPutDown.playerCopyisPuttingDown == false*/)
            {
                isHoldWalk = true;
                playerCopyAnimator.SetBool("HoldUp", true);
                playerCopyAnimator.SetBool("PutDown", false);
            print("outsideaa");
                if (liftedBox == null)
                {
                    TryLiftBox();
                print("aa");
            }
            }
            //else if (singleAction.key == "F" && PlayerCopyPutDown.playerCopyisPuttingDown == true)
            //{
            //    if (liftedBox == null)
            //    {
            //        playerCopyAnimator.SetBool("HoldUp", false);
            //        playerCopyAnimator.SetBool("PutDown", true);
            //        isHoldWalk = false;
            //    }
            //    else
            //    {
            //        playerCopyAnimator.SetBool("HoldUp", false);
            //        playerCopyAnimator.SetBool("PutDown", true);
            //        DropBox();
            //    }
            //}
        
        
        if (singleAction.key == "Space")
        {
            isGrounded = Physics2D.Raycast(playerCopyGameobject.transform.position, Vector2.down, 0.15f, groundLayer);

            if (isGrounded)
            {
                playerCopyRigidbody.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            }
        }

    }
    public IEnumerator StartWheel()
    {
        //elapsedRotationTime = 0f;
        //isRotating = true;
        //if (interactObject != null)
        //{
        //    while (elapsedRotationTime < rotationDuration)
        //    {
        //        interactObject.transform.Rotate(Vector3.forward * 90 * Time.deltaTime);

        //        elapsedRotationTime += Time.deltaTime;
        //        yield return null;
        //    }
        //    yield return new WaitForSeconds(100);
        //    if (interactObject != null)
        //    {
        //        if (elapsedRotationTime >= rotationDuration)
        //        {
        //            //finish the wheel
        //            interactObject.GetComponent<Collider2D>().tag = "Finish";

        //        }
        //    }
        //}
        //interactObject = null;
        //isInteracting = false;
        //isRotating = false;

        elapsedRotationTime = 0f;
        isRotating = true;
        if (interactObject != null)
        {
            Transform outerGear = interactObject.transform.GetComponent<Wheel>().outerGear;
            Transform middleGear = interactObject.transform.GetComponent<Wheel>().middleGear;
            Transform outgear = interactObject.transform.GetComponent<Wheel>().outGear;
            interactObject.GetComponent<AudioSource>().Play();
            while (elapsedRotationTime < rotationDuration / 3)
            {
                outerGear.transform.Rotate(Vector3.forward * 90 * Time.deltaTime, Space.Self);
                elapsedRotationTime += Time.deltaTime;
                yield return null;
            }

            while (elapsedRotationTime < rotationDuration / 3 * 2)
            {
                middleGear.Rotate(Vector3.forward * 120 * Time.deltaTime, Space.Self);
                elapsedRotationTime += Time.deltaTime;
                yield return null;
            }
            while (elapsedRotationTime < rotationDuration)
            {
                outgear.Rotate(Vector3.forward * 180 * Time.deltaTime, Space.Self);
                elapsedRotationTime += Time.deltaTime;
                yield return null;
            }
            interactObject.GetComponent<AudioSource>().Stop();
        }
        aniController.doorAniComplete = true;
        wheel2Rotated = true;
        if (Wheel2Rotated != null)
        {
            Wheel2Rotated(wheel2Rotated);
            print(wheel2Rotated + "DG");
        }
    }

    private IEnumerator RotateBackToInitialRotation()
    {
        //float elapsedBackTime = 0f;

        //while (elapsedBackTime < rotationDuration)
        //{
        //    float t = elapsedBackTime / rotationDuration;
        //    interactObject.transform.rotation = Quaternion.Slerp(interactObject.transform.rotation, initialWheelRotation, t);
        //    elapsedBackTime += 3 * Time.deltaTime;
        //    yield return null;
        //}

        //interactObject.transform.rotation = initialWheelRotation;

        float elapsedBackTime = 0f;
        Transform outerGear = interactObject.transform.GetComponent<Wheel>().outerGear;
        Transform middleGear = interactObject.transform.GetComponent<Wheel>().middleGear;
        Transform outgear = interactObject.transform.GetComponent<Wheel>().outGear;
        while (elapsedBackTime < rotationDuration / 3)
        {
            float t = elapsedBackTime / rotationDuration / 3;
            outgear.transform.Rotate(Vector3.forward * -180 * Time.deltaTime, Space.Self);
            elapsedBackTime += 1.5f * Time.deltaTime;
            yield return null;
        }
        while (elapsedBackTime < rotationDuration / 3 * 2)
        {
            float t = elapsedBackTime / rotationDuration / 3 * 2;

            middleGear.Rotate(Vector3.forward * -120 * Time.deltaTime, Space.Self);
            elapsedBackTime += 1.5f * Time.deltaTime;
            yield return null;
        }
        while (elapsedBackTime < rotationDuration)
        {
            float t = elapsedBackTime / rotationDuration;
            outerGear.transform.Rotate(Vector3.forward * -90 * Time.deltaTime, Space.Self);

            elapsedBackTime += 1.5f * Time.deltaTime;
            yield return null;
        }

        isRotating = false;
        if (interactObject != null)
        {
            interactObject = null;
        }
        
    }

    void TryLiftBox()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(playerCopyGameobject.transform.position, pickupRange);

        foreach (var hit in hits)
        {
            if (hit.tag == "Box")
            {
                //hit.GetComponent<Rigidbody2D>().gravityScale = 0f;
                //hit.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                liftedBox = hit.gameObject;
                //liftedBox.GetComponent<BoxCollider2D>().isTrigger = true;
                liftedBox.transform.parent = playerCopyGameobject.transform;
                liftedBox.transform.localPosition = new Vector3(0, liftOffsetY, 0);
                break;
            }
        } 
    }

    void DropBox()
    {
        liftedBox.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        //liftedBox.GetComponent<BoxCollider2D>().isTrigger = false;
        liftedBox.transform.position = this.GetComponent<PlayerController>().PutDir * Vector2.right * 1.4f + (Vector2)transform.position;
        liftedBox.transform.parent = null;
        liftedBox = null;
    }

    private void SideAni()
    {
        playerCopyAnimator.SetFloat("HoldDir", putDir);
        playerCopyAnimator.SetBool("isPuttingDown", PutDown.isPuttingDown);
    }
}

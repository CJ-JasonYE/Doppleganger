using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using System.Numerics;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public enum PlayerAudioType
    {
        Doppel,
        DropBox,
        Walk,
        Gear,
        Jump,
        LiftBox
    }
    [System.Serializable]
    public class PlayerClip
    {
        public PlayerAudioType type;
        public AudioClip audioClip;
        public PlayerClip(PlayerAudioType type, AudioClip audioClip)
        {
            this.type = type;
            this.audioClip = audioClip;
        }
    }

    [SerializeField] private float horizontalMoveSpeed = 5;
    //public static float HorizontalMoveSpeed { get; private set; }

    private Rigidbody2D rigidBody2D;
    private Animator animator;
    private bool isInputEnable = true;
    public float jumpForce = 5f;
    
    public bool isGrounded;
    private LayerMask groundLayer;
    PlayerPickupController playerPickupController;
    private int putDir;
    public int PutDir { get { return putDir; } private set { putDir = value; } }
    public bool isInteracting = false;
    public GameObject interactObject = null;
    private Coroutine wheelCoroutine1;
    private Coroutine wheelCoroutine2;
    private Animator doorAni;
    private bool isHoldWalk = false;
    float rotationDuration = 2.5f;
    public float elapsedRotationTime = 0f;
    public bool isRotating = false;
    private Quaternion initialWheelRotation;

    private AudioSource playerAudio;
    PlayerAudioType playerAudioType;
    
    public PlayerClip[] playerClipsArray = new PlayerClip[Enum.GetNames(typeof(PlayerAudioType)).Length];
    public AudioClip[] playerAudioClips = new AudioClip[Enum.GetNames(typeof(PlayerAudioType)).Length];

    private Vector3 pickupBoxOriginalPosition;
    private AnimatorController aniController;

    private float wheelTimer = 0f;
    public event Action<float> onWheelEvent;

    private bool wheel1Rotated = false;
    public event Action<bool> Wheel1Rotated;
    public event Action<GameObject> GetInteractObj;


    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        groundLayer = LayerMask.GetMask("Ground");
        playerPickupController = GetComponent<PlayerPickupController>();
        putDir = 1;
        playerAudio= GetComponent<AudioSource>();

        playerAudioClips = Resources.LoadAll<AudioClip>("Sound/PlayerClips");
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
        for (int i = 0; i < playerAudioClips.Length; i++)
        {
            PlayerClip temp = new PlayerClip((PlayerAudioType)i, playerAudioClips[i]);
            playerClipsArray[i] = temp;
        }
        doorAni = GameObject.Find("RPP2_door_01_f1")?.GetComponent<Animator>();
        if (doorAni != null)
        {
            aniController = doorAni.GetComponent<AnimatorController>();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isInputEnable)
        {
            Move();
            HoldAndPut();
            Jump();

            if (!playerAudio.isPlaying)
            {
                if (rigidBody2D.velocity.x != 0)
                {
                    if (isGrounded)
                    {
                        playerAudio.PlayOneShot(playerAudioClips[(int)PlayerAudioType.Walk]);
                    }
                }
            }
            StopCoroutine(DisableInput());
        }

        SideAni();

        //keep box alway above player's head
        if (playerPickupController.liftedBox != null)
        {
            playerPickupController.liftedBox.transform.position = transform.position + new Vector3(0, playerPickupController.liftOffsetY, 0);
        }

    }

    private void Move()
    {
        // Player Movement Control
        if (Input.GetKey(KeyCode.A))
        {
            rigidBody2D.velocity = new Vector2(-horizontalMoveSpeed, rigidBody2D.velocity.y);
            PutDir = -1;
            
            animator.SetBool("StartWalk", true);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rigidBody2D.velocity = new Vector2(horizontalMoveSpeed, rigidBody2D.velocity.y);
            PutDir = 1;
            
            animator.SetBool("StartWalk", true);
        }
        else
        {
            rigidBody2D.velocity = new Vector2(0,rigidBody2D.velocity.y);
            
            animator.SetBool("StartWalk", false);
        }

        WalkAni();
    }

    private void WalkAni()
    {
        if (!isHoldWalk)
        {
            if (rigidBody2D.velocity.x > 0)
            {
                animator.SetFloat("Walk", 1);
            }
            else if (rigidBody2D.velocity.x < 0)
            {
                animator.SetFloat("Walk", -1);
            }
            else
            {
                animator.SetFloat("Walk", 0);
            }
        }
        else
        {
            if (rigidBody2D.velocity.x > 0)
            {
                animator.SetFloat("Dir", 1);
            }
            else if (rigidBody2D.velocity.x < 0)
            {
                animator.SetFloat("Dir", -1);
            }
            else
            {
                isHoldWalk = false;
            }
        }
    }

    private void HoldAndPut()
    {
        if (!isInteracting)
        {
            if (Input.GetKeyDown(KeyCode.F) && PutDown.isPuttingDown == false)
            {
                isHoldWalk = true;
                animator.SetBool("HoldUp", true);
                animator.SetBool("PutDown", false);
                if (playerPickupController.liftedBox == null)
                {
                    playerPickupController.TryInteract();
                    playerAudio.PlayOneShot(playerAudioClips[(int)PlayerAudioType.LiftBox]);
                }
            }
            else if (Input.GetKeyDown(KeyCode.F) && PutDown.isPuttingDown == true)
            {
                if (playerPickupController.liftedBox == null)
                {
                    //If Realse F, Put Down items
                    animator.SetBool("HoldUp", false);
                    animator.SetBool("PutDown", true);
                    StartCoroutine(DisableInput());
                    isHoldWalk = false;
                }
                else
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position,
                    this.GetComponent<PlayerController>().PutDir * Vector2.right * 1.9f, 1.9f, LayerMask.GetMask("Ground"));

                    if (hit.collider == null)
                    {
                        if (playerPickupController.liftedBox != null)
                        {
                            animator.SetBool("HoldUp", false);
                            animator.SetBool("PutDown", true);
                            playerAudio.PlayOneShot(playerAudioClips[(int)PlayerAudioType.DropBox]);
                            StartCoroutine(DisableInput());
                            playerPickupController.DropBox();
                            isHoldWalk = false;
                        }
                    }
                }

               
            }

        }
        else
        {
            if (Input.GetKey(KeyCode.F) && interactObject.tag == "Wheel")
            {
                wheelTimer += Time.deltaTime;

                if (!isRotating)
                {
                    AnimatorController.isLeavingTrigger = false;
                    
                    wheelCoroutine1 = StartCoroutine(StartWheel());
                    if (wheelCoroutine2 != null)
                    {
                        StopCoroutine(wheelCoroutine2);
                    }
                    if(doorAni != null)
                    {
                        doorAni.speed = 1f;
                        doorAni.Play("Open");
                    }
                    
                }
            }
            else if (Input.GetKeyUp(KeyCode.F) && interactObject.tag == "Wheel")
            {
                onWheelEvent?.Invoke(wheelTimer);
                wheelTimer = 0;

                isRotating = false;
                elapsedRotationTime = 0;
                
                
                //if (interactObject != null)
                //{
                //    interactObject.transform.GetComponent<Animator>().StopPlayback();
                //}

                wheel1Rotated = false;
                if (Wheel1Rotated != null)
                {
                    Wheel1Rotated(wheel1Rotated);
                }

                if (wheelCoroutine1 != null)
                {
                    StopCoroutine(wheelCoroutine1);
                }
                if (doorAni != null)
                {
                    doorAni.speed = 0f;
                    doorAni.Play("Open", 0, 0);
                    aniController.doorAniComplete = false;
                }
                if (interactObject != null)
                {
                    wheelCoroutine2 = StartCoroutine(RotateBackToInitialRotation());
                    interactObject.GetComponent<AudioSource>().Stop();

                    interactObject.GetComponent<AudioSource>().PlayOneShot(CreateTempClip(playerClipsArray[(int)PlayerAudioType.Gear].audioClip, 3));

                }
            }
        }
        
    }

    public AudioClip CreateTempClip(AudioClip clip, int time)
    {
        float[] audioData = new float[clip.samples];
        clip.GetData(audioData, 0);
        int newClipLengthInSamples = time * clip.frequency;
        float[] newClipData = new float[newClipLengthInSamples];
        System.Array.Copy(audioData, newClipData, newClipLengthInSamples);
        AudioClip newAudioClip = AudioClip.Create("NewAudioClip", newClipLengthInSamples, clip.channels, clip.frequency, false);
        newAudioClip.SetData(newClipData, 0);
        return newAudioClip;
    }

    public void Jump()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.15f, groundLayer);
        
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rigidBody2D.mass = 1;
            //rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x, jumpForce);
            rigidBody2D.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);

            playerAudio.PlayOneShot(playerAudioClips[(int)PlayerAudioType.Jump]);
        }
    }



    IEnumerator DisableInput()
    {
        isInputEnable = false; 
        yield return new WaitForSeconds(0.5f);
        isInputEnable = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down*0.15f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wheel"))
        {
            interactObject = other.gameObject;
            initialWheelRotation = interactObject.transform.rotation;
            isInteracting = true;
        }
        else if (other.CompareTag("Door"))
        {
            
            LevelController.Instance.ToNextLevel(aniController.doorAniComplete);
            
        }
        else if (other.name == "Door2")
        {
            //LevelController.Instance.ToNextLevel(PlatformController.ToThirdLevel);
            GameObject win = UIcontroller.instance.transform.GetChild(0).gameObject;
            win.SetActive(true);
            win.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/rpp2_victory_ending_ui");

        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag=="Door")
        {
            LevelController.Instance.ToNextLevel(aniController.doorAniComplete);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //Transform outerGear = interactObject.transform.GetComponent<Wheel>().outerGear;
        //Transform middleGear = interactObject.transform.GetComponent<Wheel>().middleGear;
        //Transform outgear = interactObject.transform.GetComponent<Wheel>().outGear;
        if (other.CompareTag("Wheel"))
        {
            if (doorAni != null)
            {
                doorAni.speed = 0f;
                doorAni.Play("Open", 0, 0);
                aniController.doorAniComplete = false;
            }
            
            if (wheelCoroutine1 != null)
            {
                StopCoroutine(wheelCoroutine1);
                if (wheelCoroutine2 != null)
                {
                    StopCoroutine(wheelCoroutine2);
                }
            }
            if (!isRotating)
            {
                // ��������ת�س�ʼ״̬
                if (interactObject != null)
                {
                    interactObject.GetComponent<AudioSource>().Stop();
                    //outerGear.rotation = initialWheelRotation;
                    //middleGear.rotation = initialWheelRotation;
                    //outgear.rotation = initialWheelRotation;
                }
                interactObject = null;
                isInteracting = false;
                elapsedRotationTime = 0;
                AnimatorController.isLeavingTrigger = true;
            }
            else
            {
                StartCoroutine(RotateBackToInitialRotation());
                interactObject = null;
                isInteracting = false;
                elapsedRotationTime = 0;
                AnimatorController.isLeavingTrigger = true;
                isRotating = false;
            }
            
        }
    }

    public IEnumerator StartWheel()
    {
        elapsedRotationTime = 0f;
        isRotating = true;
        if (interactObject != null)
        {
            Transform outerGear = interactObject.transform.GetComponent<Wheel>().outerGear;
            Transform middleGear = interactObject.transform.GetComponent<Wheel>().middleGear;
            Transform outgear = interactObject.transform.GetComponent<Wheel>().outGear;
            interactObject.GetComponent<AudioSource>().Play();
            while (elapsedRotationTime < rotationDuration/3)
            {
                outerGear.transform.Rotate(Vector3.forward * 90 * Time.deltaTime, Space.Self);
                
                //interactObject.transform.Rotate(Vector3.forward * 90 * Time.deltaTime);


                //interactObject.transform.GetComponent<Animator>().Play("WheelAni");


                //outerGear.rotation = Quaternion.Lerp(outerGear.rotation, Quaternion.Euler(0, 0, 90), t);
                ////yield return new WaitForSeconds(rotationDuration / 3);
                //middleGear.rotation = Quaternion.Lerp(middleGear.rotation, Quaternion.Euler(0, 0, 120), t);
                ////yield return new WaitForSeconds(rotationDuration / 3);
                //outgear.rotation = Quaternion.Lerp(outgear.rotation, Quaternion.Euler(0, 0, 180), t);
                ////yield return new WaitForSeconds(rotationDuration / 3);
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
            //yield return new WaitForSeconds(1);
            //if (interactObject != null)
            //{
            //    if (elapsedRotationTime >= rotationDuration)
            //    {
            //        //finish the wheel
            //        interactObject.GetComponent<Collider2D>().tag = "Finish";
            //    }
            //}
        }
        aniController.doorAniComplete = true;
        wheel1Rotated = true;
        if (Wheel1Rotated != null)
        {
            Wheel1Rotated(wheel1Rotated);
        }
        //isInteracting = false;
        //isRotating = false;
    }

    private IEnumerator RotateBackToInitialRotation()
    {
        float elapsedBackTime = 0f;
        Transform outerGear = interactObject.transform.GetComponent<Wheel>().outerGear;
        Transform middleGear = interactObject.transform.GetComponent<Wheel>().middleGear;
        Transform outgear = interactObject.transform.GetComponent<Wheel>().outGear;
        while (elapsedBackTime < rotationDuration/3)
        {
            float t = elapsedBackTime / rotationDuration/3;
            //interactObject.transform.rotation = Quaternion.Slerp(interactObject.transform.rotation, initialWheelRotation, t);

            //outerGear.rotation = Quaternion.Slerp(outerGear.rotation, initialWheelRotation, t);
            outgear.transform.Rotate(Vector3.forward * -180 * Time.deltaTime, Space.Self);
            //yield return new WaitForSeconds(rotationDuration / 3);
            //middleGear.rotation = Quaternion.Slerp(middleGear.rotation, initialWheelRotation, t);
            //yield return new WaitForSeconds(rotationDuration / 3);
            //outgear.rotation = Quaternion.Slerp(outgear.rotation, initialWheelRotation, t);
            //yield return new WaitForSeconds(rotationDuration / 3);
            elapsedBackTime +=1.5f* Time.deltaTime;
            yield return null;
        }
        while (elapsedBackTime < rotationDuration / 3 * 2)
        {
            float t = elapsedBackTime / rotationDuration / 3 * 2;
            //middleGear.rotation = Quaternion.Slerp(middleGear.rotation, initialWheelRotation, t);
            
            middleGear.Rotate(Vector3.forward * -120 * Time.deltaTime, Space.Self);
            elapsedBackTime += 1.5f * Time.deltaTime;
            yield return null;
        }
        while (elapsedBackTime < rotationDuration)
        {
            float t = elapsedBackTime / rotationDuration;
            //outgear.rotation = Quaternion.Slerp(outgear.rotation, initialWheelRotation, t);
            outerGear.transform.Rotate(Vector3.forward * -90 * Time.deltaTime, Space.Self);
            
            elapsedBackTime += 1.5f * Time.deltaTime;
            yield return null;
        }

        //interactObject.transform.rotation = initialWheelRotation;
        //outerGear.rotation = initialWheelRotation;
        //middleGear.rotation = initialWheelRotation;
        //outgear.rotation = initialWheelRotation;
        isRotating = false;
    }


    private void SideAni()
    {
        animator.SetFloat("HoldDir", PutDir);
        animator.SetBool("isPuttingDown", PutDown.isPuttingDown);
    }
}

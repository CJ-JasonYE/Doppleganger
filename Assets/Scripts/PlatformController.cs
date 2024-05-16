using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public class PlatformController : MonoBehaviour
{
    public Transform platform;
    public Transform wheel1;  
    public Transform wheel2;
    public float moveSpeed = 2.0f; // upward and downward speed

    private Vector3 initialPosition; // 初始位置
    private Vector3 targetPosition;  // 目标位置
    private bool wheel1Pulled = false;
    private bool wheel2Pulled = false;

    private AudioSource audioS;
    private Animator door2Ani;
    private static bool toThirdLevel = false;
    public static bool ToThirdLevel { get { return toThirdLevel; } }

    // Start is called before the first frame update
    void Start()
    {
        platform= GetComponent<Transform>();
        audioS= GetComponent<AudioSource>();
        initialPosition = platform.position;
        targetPosition = new Vector3(19.18558f, 0.66f, 3.407607f);
        platform.position = targetPosition;
        PlayerController playerController = FindObjectOfType<PlayerController>();
        PlaybackController playBackController = FindObjectOfType<PlaybackController>();
        if (playerController != null && playBackController!=null)
        {
            playerController.Wheel1Rotated += HandleWheel1Rotated;
            playBackController.Wheel2Rotated += HandleWheel2Rotated;
        }
        AudioClip temp = playerController.CreateTempClip(playerController.playerClipsArray[(int)PlayerAudioType.Gear].audioClip, 4);
        
        audioS.PlayOneShot(temp);

        door2Ani = GameObject.Find("Door2")?.GetComponent<Animator>();
        print(door2Ani);
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckBothWheelsRotated();
    }

    private void HandleWheel1Rotated(bool wheel1Rotated)
    {
        print(wheel1Rotated);
        if (wheel1Rotated)
        {
            wheel1Pulled= wheel1Rotated;
        }
    }
    private void HandleWheel2Rotated(bool wheel2Rotated)
    {
        print (wheel2Rotated);
        if (wheel2Rotated)
        {
            wheel2Pulled= wheel2Rotated;
        }
    }

    private void CheckBothWheelsRotated()
    {
        // 检查两个轮子是否都完成旋转
        if (wheel1Pulled && wheel2Pulled)
        {
            // 执行平台升起的操作，例如：
            MovePlatformUp();
            if (door2Ani != null)
            {
                door2Ani.Play("Open");
                toThirdLevel = true;
            }
        }
        else
        {
            MovePlatformDown();
        }
    }


    private void MovePlatformUp()
    {
        platform.position = Vector3.MoveTowards(platform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    private void MovePlatformDown()
    {
        if(platform.position != initialPosition)
        {
            platform.position = Vector3.MoveTowards(platform.position, initialPosition, moveSpeed * Time.deltaTime);
        }
        
    }

}

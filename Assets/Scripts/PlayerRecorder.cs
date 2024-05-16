using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static PlayerController;

public class PlayerRecorder : MonoBehaviour
{
    // Final List
    public static List<PairedAction> pairedActions = new List<PairedAction>();
    public static List<SingleAction> singleActions = new List<SingleAction>();

    public static Vector3 originalPosition;
    private AudioSource audioSource;
    private PlayerController playerController;
    private Canvas canvas;
    private Animator uiAnimator;
    private Image closeup_Bg;
    private bool bgStartLerp;
    private Color tempC;
    private AudioClip exitClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start() 
    {
        playerController.onWheelEvent += WheelEvent;
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        uiAnimator=canvas.GetComponentInChildren<Animator>(); 
        closeup_Bg = canvas.transform.GetChild(1).GetComponent<Image>();
        tempC = closeup_Bg.color;
        exitClip = Resources.Load<AudioClip>("Sound/Doppel 4");
    }

    struct RecordedAction
    {
        public float time;
        public string action;

        public RecordedAction(float t, string a)
        {
            time = t;
            action = a;
        }
    }

    List<RecordedAction> recordedActions = new List<RecordedAction>();
    bool recording = false;
    float startTime;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartRecording();
            audioSource.PlayOneShot(playerController.playerAudioClips[(int)PlayerController.PlayerAudioType.Doppel]);
            uiAnimator.SetTrigger("CloseUp");
            bgStartLerp=true;
            uiAnimator.SetBool("Reset", false);
        }
        // Make TimeZone and lerpBackground color a value
        if (bgStartLerp)
        {
            closeup_Bg.color = Color.Lerp(closeup_Bg.color, new Color(tempC.r, tempC.g, tempC.b, 0.6f), 0.02f);
            //print(closeup_Bg.color);
        }
        else
        {
            if (Mathf.Approximately(closeup_Bg.color.a, 0))
            {
                closeup_Bg.color = tempC;
            }
            else
            {
                closeup_Bg.color = Color.Lerp(closeup_Bg.color, new Color(tempC.r, tempC.g, tempC.b, 0), 0.04f);
            }
        }

        if (recording)
        {
            RecordActions();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            StopRecording();
            audioSource.PlayOneShot(exitClip);
            bgStartLerp = false;
            uiAnimator.SetBool("Reset", true);
        }
    }

    void StartRecording()
    {
        Debug.Log("Start Recording");

        originalPosition = gameObject.transform.position;

        pairedActions.Clear();
        singleActions.Clear();
        recordedActions.Clear();

        recording = true;
        startTime = Time.time;
    }

    void RecordActions()
    {
        float currentTime = Time.time - startTime;

        if (Input.GetKeyDown(KeyCode.A))
        {
            recordedActions.Add(new RecordedAction(currentTime, "A Pressed"));
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            recordedActions.Add(new RecordedAction(currentTime, "A Released"));
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            recordedActions.Add(new RecordedAction(currentTime, "D Pressed"));
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            recordedActions.Add(new RecordedAction(currentTime, "D Released"));
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            recordedActions.Add(new RecordedAction(currentTime, "F Pressed"));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            recordedActions.Add(new RecordedAction(currentTime, "Space Pressed"));
        }
    }

    void StopRecording()
    {
        Debug.Log("Stop Recording");

        // // Drop the Box
        // if (gameObject.GetComponent<PlayerPickupController>().liftedBox != null)
        // {
        //     gameObject.GetComponent<PlayerPickupController>().liftedBox.transform.parent = null;
        //     gameObject.GetComponent<PlayerPickupController>().DropBox();
        // }

        // Reset Box to original Position
        gameObject.GetComponent<PlayerPickupController>().RestBoxPosition();

        // Back to original position
        gameObject.transform.position = originalPosition;

        recording = false;
        MatchActions();

        // foreach (var sa in pairedActions)
        // {
        //     Debug.Log(sa.key);
        //     Debug.Log(sa.pressTime);
        //     Debug.Log(sa.releaseTime);
        // }
    }

    private void MatchActions()
    {
        for (int i = 0; i < recordedActions.Count; i++)
        {
            RecordedAction currentAction = recordedActions[i];
            string currentKey = currentAction.action.Split(" ")[0];
            float pressTime = currentAction.time;
            float releaseTime = -1;

            if (currentKey == "F" || currentKey == "Space")
            {
                singleActions.Add(new SingleAction(currentKey, pressTime));
                continue;
            }

            if (currentAction.action.EndsWith("Pressed"))
            {
                for (int j = i + 1; j < recordedActions.Count; j++)
                {
                    RecordedAction nextAction = recordedActions[j];
                    if (nextAction.action == currentKey + " Released")
                    {
                        releaseTime = nextAction.time;
                        pairedActions.Add(new PairedAction(currentKey, pressTime, releaseTime));
                        break;
                    }
                }
            }
        }
    }

    public void WheelEvent(float pressTime)
    {
        if (recording)
        {
            float currentTime = Time.time - startTime;
            pairedActions.Add(new PairedAction("Wheel", currentTime, currentTime + pressTime));
        }
    }

}

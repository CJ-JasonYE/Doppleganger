using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIcontroller : MonoBehaviour
{
    public GameObject text;
    public Button button;
    public static UIcontroller instance;

    private void Awake()
    {
        instance = this;
        
    }
    // Start is called before the first frame update
    void Start()
    {
        if (text != null)
        {
            text.SetActive(true);
            button.onClick.AddListener(CloseCanvas);
        }
        
    }
    void CloseCanvas()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }
    public void victoryCanvas()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

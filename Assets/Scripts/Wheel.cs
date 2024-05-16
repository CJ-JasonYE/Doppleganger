using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    private Rigidbody2D wheelRB;
    public Transform middleGear;
    public Transform outGear;
    public Transform outerGear;

    // Start is called before the first frame update
    void Start()
    {
        middleGear=transform.GetChild(0);
        outGear=transform.GetChild(1);
        outerGear = transform.GetChild(2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}

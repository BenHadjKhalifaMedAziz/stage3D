using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class details : MonoBehaviour
{
    [TextArea]
    [Tooltip("This is the door data string.")]
    public string doorData1 = "";

    public string doorData = "";


    [TextArea]
    [Tooltip("This is the wall data string.")]
    public string wallData1 = "";

    public string wallData = "";

    private void Update()
    {
       if (doorData != doorData1)
        {
            doorData1 = doorData;

            Debug.Log("change");
        }

        if (wallData != wallData1)
        {
            wallData1 = wallData;
            Debug.Log("change");
        }

    }
}

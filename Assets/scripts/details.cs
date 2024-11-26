using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class details : MonoBehaviour
{
    [TextArea]
    [Tooltip("This is the door data string.")]
    public bool doorData1 = false;

    public bool doorData = false;


    [TextArea]
    [Tooltip("This is the wall data string.")]
    public string wallData1 = "";

    public string wallData = "";


    [TextArea]
    [Tooltip("This is the tag string.")]
    public string tag1 = "";

    public string tag = "";



    [Tooltip("This is the layer data int.")]
    public int layer1 = 0;

    public int layer = 0;






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


        if (layer != layer1)
        {
            layer1 = layer;
            Debug.Log("change");
        }

        if (tag != tag1)
        {
            tag1 = tag;
            Debug.Log("change");
        }



    }
}

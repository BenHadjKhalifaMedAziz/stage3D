using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class details : MonoBehaviour
{
    
    [Tooltip("This is the door data string.")]
    public bool doorData1 = false;

    [HideInInspector]
    public bool doorData = false;

    [Tooltip("This is the reserved.")]
    public bool reserved1 = false;

    [HideInInspector]
    public bool reserved = false;

    [Tooltip("This is the reserved.")]
    public bool frameON1 = false;

    [HideInInspector]
    public bool frameON = false;



    [TextArea]
    [Tooltip("This is the wall data string.")]
    public string wallData1 = "";
    [HideInInspector]
    public string wallData = "";


    [TextArea]
    [Tooltip("This is the tag string.")]
    public string tag1 = "";
    [HideInInspector]
    public string tag = "";



    [Tooltip("This is the layer data int.")]
    public int layer1 = 0;
    [HideInInspector]
    public int layer = 0;

   






    private void Update()
    {
       if (doorData != doorData1)
        {
            doorData1 = doorData;

        }

        if (frameON != frameON1)
        {
            frameON1 = frameON;

        }

        if (wallData != wallData1)
        {
            wallData1 = wallData;
        }


        if (layer != layer1)
        {
            layer1 = layer;
        }

        if (tag != tag1)
        {
            tag1 = tag;
        }

        if (reserved != reserved1)
        {
            reserved1 = reserved;
        }



    }
}

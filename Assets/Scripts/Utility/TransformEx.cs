using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformEx
{


    public static Transform SetXLocalPos(this Transform t, float x)
    {
        t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
        return t;
    }
    public static Transform SetYlocalPos(this Transform t, float y)
    {
        t.localPosition = new Vector3(t.localPosition.x, y, t.localPosition.z);
        return t;
    }
    public static Transform SetZLocalPos(this Transform t, float z)
    {
        t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, z);
        return t;
    }
   

    public static Transform SetXPos(this Transform t, float x)
    {
        t.position = new Vector3(x, t.position.y, t.position.z);
        return t;
    }
    public static Transform SetYPos(this Transform t, float y)
    {
        t.position = new Vector3(t.position.x, y, t.position.z);
        return t;
    }
    public static Transform SetZPos(this Transform t, float z)
    {
        t.position = new Vector3(t.position.x, t.position.y, z);
        return t;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VitoBarra.Utils.TextVerifier
{
    public static class TransformEx
    {
        public static Transform SetXLocalPos(this Transform t, float x)
        {
            t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
            return t;
        }

        public static Transform SetYLocalPos(this Transform t, float y)
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

    public static class VectorEx
    {
        public static Vector2 Offset(this Vector2 v, float x = 0, float y = 0)
        {
            return new Vector2(v.x + x, v.y + y);
        }


        public static Vector3 Offset(this Vector3 v, float x, float y = 0, float z = 0)
        {
            return new Vector3(v.x + x, v.y + y, v.z + z);
        }
    }
}
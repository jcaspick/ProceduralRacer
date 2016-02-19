using UnityEngine;
using System.Collections;

public static class VectorExtensions {

    public static Vector3 getFlat(this Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector2 Vector2(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.y);
    }
}

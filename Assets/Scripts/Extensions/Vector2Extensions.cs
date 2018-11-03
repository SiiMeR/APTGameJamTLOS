using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extensions
{
    public static Vector3Int ToVector3Int(this Vector2 vector2)
    {
        return new Vector3Int((int) vector2.x, (int) vector2.y, 0);
    }

    public static Vector2 Add(this Vector2 vector2, float toAdd)
    {
        return new Vector2(vector2.x + toAdd, vector2.y + toAdd);
    }
}

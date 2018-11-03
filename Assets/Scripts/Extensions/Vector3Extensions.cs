using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector2 Vector2(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.y);
    }

    public static Vector3Int Vector3Int(this Vector3 vector3)
    {
        return new Vector3Int((int) vector3.x, (int) vector3.y, (int) vector3.z);
    }
    
    public static Vector3 AddX(this Vector3 vector3, float toAdd)
    {
        return new Vector3(vector3.x + toAdd, vector3.y, vector3.z);
    }

    public static Vector3 RoundX(this Vector3 vector3)
    {
        return new Vector3(Mathf.Round(vector3.x), vector3.y, vector3.z);
    } 
    
    public static Vector3 AddY(this Vector3 vector3, float toAdd)
    {
        return new Vector3(vector3.x, vector3.y + toAdd, vector3.z);
    }

    public static Vector3 RoundY(this Vector3 vector3)
    {
        return new Vector3(vector3.x, Mathf.Round(vector3.y), vector3.z);
    }
    
    public static Vector3 Add(this Vector3 vector3, float toAdd)
    {
        return new Vector3(vector3.x + toAdd, vector3.y + toAdd, vector3.z + toAdd);
    }

    public static Vector3 Round(this Vector3 vector3)
    {
        return new Vector3(Mathf.Round(vector3.x), Mathf.Round(vector3.y), Mathf.Round(vector3.z));
    }
    
}

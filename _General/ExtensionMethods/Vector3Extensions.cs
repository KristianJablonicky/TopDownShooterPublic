using System;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 WithXY(this Vector3 vec, Vector3 newVector)
    {
        return new(newVector.x, newVector.y, vec.z);
    }
}
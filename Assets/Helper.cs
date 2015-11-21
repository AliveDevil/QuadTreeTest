using System;
using UnityEngine;

public static class Helper
{
    public static int LevelsOfDetail { get { return 7; } }

    public static int ViewDistance { get { return 1024; } }

    public static T _<T>(this T t, Action<T> _)
    {
        _(t);
        return t;
    }

    public static int Map(this int i, int c)
    {
        if (i < 0) i += c * (-i / c + 1);
        return i % c;
    }

    public static Vector3 X0Z(this Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    public static Vector3 X0Z(this Vector3 v)
    {
        v.y = 0;
        return v;
    }

    public static Vector3 XYZ(this Vector2 v, float y)
    {
        return new Vector3(v.x, y, v.y);
    }
}

using UnityEngine;

public struct Triangle
{
    private Vector3 v1, v2, v3;

    public Vector3 V1
    {
        get { return v1; }
    }

    public Vector3 V2
    {
        get { return v2; }
    }

    public Vector3 V3
    {
        get { return v3; }
    }

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
    }
}

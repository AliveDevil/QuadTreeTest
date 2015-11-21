using System.Collections.Generic;
using UnityEngine;

public class TriangleMesh
{
    private List<Triangle> triangles = new List<Triangle>();

    public void AddTriangle(Triangle triangle)
    {
        triangles.Add(triangle);
    }

    public void Clear()
    {
        triangles.Clear();
    }

    public void Mesh(out Vector3[] vertices, out int[] indices)
    {
        vertices = new Vector3[triangles.Count * 3];
        indices = new int[triangles.Count * 3];

        for (int i = 0; i < triangles.Count; i++)
        {
            vertices[i * 3] = triangles[i].V1;
            vertices[i * 3 + 1] = triangles[i].V2;
            vertices[i * 3 + 2] = triangles[i].V3;
            indices[i * 3] = i * 3;
            indices[i * 3 + 1] = i * 3 + 1;
            indices[i * 3 + 2] = i * 3 + 2;
        }
    }
}

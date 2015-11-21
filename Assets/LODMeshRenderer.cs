using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class LODMeshRenderer : MonoBehaviour
{
    public Material Material;
    public LODMeshObject MeshObject;
    private MeshCollider lodMeshCollider;
    private MeshFilter lodMeshFilter;
    private MeshRenderer lodMeshRenderer;
    private Mesh mesh;
    private TriangleMesh triangleMesh;

    private void Awake()
    {
        mesh = new Mesh();
        triangleMesh = new TriangleMesh();
        lodMeshFilter = GetComponent<MeshFilter>();
        lodMeshRenderer = GetComponent<MeshRenderer>();
        lodMeshCollider = GetComponent<MeshCollider>();
        lodMeshRenderer.sharedMaterial = Material;
        lodMeshFilter.mesh = mesh;
    }

    private void OnDestroy()
    {
        Destroy(mesh);
    }

    private void OnDisable()
    {
        mesh.Clear();
        triangleMesh.Clear();
        lodMeshRenderer.enabled = false;
        lodMeshCollider.enabled = false;
    }

    private void OnEnable()
    {
        lodMeshRenderer.enabled = true;
        lodMeshCollider.enabled = true;
    }

    private void Update()
    {
        MeshObject.Update();
        //MeshObject.StoreOldValuesSync();
        //MeshObject.LoDSync();
        //MeshObject.ActivateSync();
        if (MeshObject != null && MeshObject.Invalid)
        {
            triangleMesh.Clear();
            Stack<LODMeshObject> meshObjects = new Stack<LODMeshObject>();
            meshObjects.Push(MeshObject);
            while (meshObjects.Count > 0)
            {
                LODMeshObject meshObject = meshObjects.Pop();
                if (meshObject == null) continue;
                meshObject.Invalid = false;
                meshObjects.Push(meshObject.Children[0]);
                meshObjects.Push(meshObject.Children[1]);
                meshObjects.Push(meshObject.Children[2]);
                meshObjects.Push(meshObject.Children[3]);
                meshObject.Triangulate(triangleMesh, MeshObject.Center.Position);
            }
            Vector3[] vertices;
            int[] triangles;
            triangleMesh.Mesh(out vertices, out triangles);
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            //lodMeshCollider.sharedMesh = null;
            //lodMeshCollider.sharedMesh = mesh;
        }
    }
}

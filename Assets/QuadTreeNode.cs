using UnityEngine;

public class QuadTreeNode
{
    public AxisAlignedRectangle AAR;
    public bool Active;
    public QuadTreeNode[] Children;
    public int Depth;
    public LODMeshObject MeshObject;
    public float MinSize;
    public QuadTreeNode Parent;
    public Vector2 Position;
    public GameObject QuadNodeObject;
    public QuadTree QuadTree;
    public float SideLength;

    public QuadTreeNode(QuadTree quadTree, QuadTreeNode parent, Vector2 position, float length, float minSize, int depth)
    {
        Active = true;
        Parent = parent;
        QuadTree = quadTree;
        Position = position;
        SideLength = length;
        MinSize = minSize;
        Depth = depth;
        AAR = new AxisAlignedRectangle(Position, SideLength * 2);
        MeshObject = new LODMeshObject(null, this, AAR, 0);
        CreateGameobject();
    }

    public static bool Encapsulates(QuadTreeNode node, Vector2 v)
    {
        return node.AAR.Contains(v);
    }

    public bool CanMerge()
    {
        return false;
        //for (int i = 0; i < Children.Length; i++)
        //{
        //    if (!Children[i].Empty()) return false;
        //    if (!Children[i].MeshObject.Empty()) return false;
        //}
        //return true;
    }

    public bool CanSplit()
    {
        return Empty() && SideLength > MinSize;
    }

    public bool Empty()
    {
        return Children == null || Children.Length == 0;
    }

    public Corner FindCorner(Vector2 v)
    {
        if (Active)
        {
            return MeshObject.FindCorner(v);
        }
        return null;
    }

    public void Merge()
    {
        Active = true;
        MeshObject.Invalidate();
        MeshObject.SetChildren(Children[0].MeshObject, Children[1].MeshObject, Children[2].MeshObject, Children[3].MeshObject);
        for (int i = 0; i < Children.Length; i++)
        {
            Children[i].DestroyGameObject();
        }
        Children = null;
        EnableRenderer();
    }

    public void Split()
    {
        Active = false;
        float halfSideLength = SideLength / 2;
        Children = new QuadTreeNode[4];
        Children[0] = CreateNode(Position + new Vector2(-halfSideLength, -halfSideLength), halfSideLength);
        Children[1] = CreateNode(Position + new Vector2(-halfSideLength, halfSideLength), halfSideLength);
        Children[2] = CreateNode(Position + new Vector2(halfSideLength, halfSideLength), halfSideLength);
        Children[3] = CreateNode(Position + new Vector2(halfSideLength, -halfSideLength), halfSideLength);

        Children[0].UseLODMesh(MeshObject.Children[0]);
        Children[1].UseLODMesh(MeshObject.Children[1]);
        Children[2].UseLODMesh(MeshObject.Children[2]);
        Children[3].UseLODMesh(MeshObject.Children[3]);

        MeshObject.Active = false;

        DisableRenderer();
    }

    public void Update()
    {
        UpdateChildren();
    }

    public void UseLODMesh(LODMeshObject lodMesh)
    {
        MeshObject.ImportLODMesh(lodMesh);
    }

    private void CreateGameobject()
    {
        QuadNodeObject = QuadTree.Pool.Get();
        QuadNodeObject.transform.SetParent(QuadTree.transform, false);
        QuadNodeObject.transform.position = Position.X0Z();
        QuadNodeObject.GetComponent<QuadTreeNodeBehaviour>().Node = this;
        QuadNodeObject.GetComponent<QuadTreeMergeBehaviour>().Node = this;
        QuadNodeObject.GetComponent<LODMeshRenderer>().MeshObject = MeshObject;
    }

    private QuadTreeNode CreateNode(Vector2 position, float size)
    {
        return new QuadTreeNode(QuadTree, this, position, size, MinSize, Depth + 1);
    }

    private void DestroyGameObject()
    {
        QuadTree.Pool.Put(ref QuadNodeObject);
    }

    private void DisableRenderer()
    {
        QuadNodeObject.GetComponent<LODMeshRenderer>().enabled = false;
        QuadNodeObject.GetComponent<QuadTreeNodeBehaviour>().enabled = false;
    }

    private void EnableRenderer()
    {
        QuadNodeObject.GetComponent<LODMeshRenderer>().enabled = true;
        QuadNodeObject.GetComponent<QuadTreeNodeBehaviour>().enabled = true;
    }

    private void UpdateChildren()
    {
        if (Children == null) return;

        for (int i = 0; i < Children.Length; i++)
        {
            Children[i].Update();
        }
    }
}

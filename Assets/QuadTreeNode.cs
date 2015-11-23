using System;
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

    public bool CanSplit()
    {
        return Children == null && SideLength > MinSize && GetDepth(MeshObject) > 3;
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

    public bool ShouldMerge()
    {
        if (Children == null) return false;

        bool anyNonEmpty = false;
        for (int i = 0; i < Children.Length; i++)
        {
            if (!Children[i].Active)
            {
                anyNonEmpty = true;
            }
            else if (!Children[i].MeshObject.Empty())
            {
                anyNonEmpty = true;
            }
        }

        return !anyNonEmpty;
        //if (Children == null) return false;

        //if (Active)
        //    if (MeshObject.Empty()) return true;

        //bool allEmpty = true;
        //bool anyNonEmpty = false;
        //for (int i = 0; i < Children.Length; i++)
        //{
        //    if (Children[i] == null) continue;
        //    allEmpty = false;
        //    if ((Children[i].Active || Children[i].Children != null) || (MeshObject.Active || MeshObject.CornerActive))
        //        anyNonEmpty = true;
        //}
        //return !(anyNonEmpty || allEmpty);
    }

    public void SetChildren(QuadTreeNode nodeBottomLeft, QuadTreeNode nodeTopLeft, QuadTreeNode nodeTopRight, QuadTreeNode nodeBottomRight)
    {
        Active = false;
        float halfSideLength = SideLength / 2;
        Children = new QuadTreeNode[4];
        Children[0] = nodeBottomLeft ?? CreateNode(Position + new Vector2(-halfSideLength, -halfSideLength), halfSideLength);
        Children[1] = nodeTopLeft ?? CreateNode(Position + new Vector2(-halfSideLength, halfSideLength), halfSideLength);
        Children[2] = nodeTopRight ?? CreateNode(Position + new Vector2(halfSideLength, halfSideLength), halfSideLength);
        Children[3] = nodeBottomRight ?? CreateNode(Position + new Vector2(halfSideLength, -halfSideLength), halfSideLength);

        MeshObject.SetChildren(Children[0].MeshObject, Children[1].MeshObject, Children[2].MeshObject, Children[3].MeshObject);

        DisableRenderer();

        ApplyValues();
    }

    public void ApplyValues()
    {
        ApplyValuesInternal();
        MeshObject.ApplyValues();
    }

    private void ApplyValuesInternal()
    {
        if (Children == null) return;

        for (int i = 0; i < Children.Length; i++)
        {
            Children[i].Depth = Depth + 1;
            Children[i].Parent = this;
            Children[i].ApplyValues();
        }
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

        DisableRenderer();
    }

    public void Update()
    {
        UpdateChildren();
        MeshObject.Update();
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
        QuadNodeObject.GetComponent<QuadTreeSplitBehaviour>().Node = this;
        QuadNodeObject.GetComponent<LODMeshRenderer>().MeshObject = MeshObject;
    }

    private QuadTreeNode CreateNode(Vector2 position, float size)
    {
        return new QuadTreeNode(QuadTree, this, position, size, MinSize, Depth + 1);
    }

    public void DestroyGameObject()
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

    private int GetDepth(LODMeshObject meshObject)
    {
        int depth = meshObject.Depth;
        for (int i = 0; i < meshObject.Children.Length; i++)
        {
            if (meshObject.Children[i] == null) continue;
            depth = Mathf.Max(depth, GetDepth(meshObject.Children[i]));
        }
        return depth;
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

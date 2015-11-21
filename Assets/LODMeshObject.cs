using System;
using UnityEngine;

public class LODMeshObject
{
    public bool Active;
    public AxisAlignedRectangle Bounds;
    public Corner Center;
    public LODMeshObject[] Children;
    public Corner[] Corner;
    public bool CornerActive;
    public bool[] CornerReference;
    public int Depth;
    public bool Invalid;
    public bool LastActive;
    public bool LastCornerActive;
    public int LoD;
    public Mesh Mesh;
    public LODMeshObject Parent;
    public QuadTreeNode QuadNode;
    public bool Redraw;
    public int TotalDepth;

    public LODMeshObject(LODMeshObject parent, QuadTreeNode quadTreeNode, AxisAlignedRectangle bounds, int depth)
    {
        Invalid = true;
        Parent = parent;
        QuadNode = quadTreeNode;
        Bounds = bounds;
        Depth = depth;
        TotalDepth = QuadNode.Depth + Depth;
        Children = new LODMeshObject[4];
        CornerReference = new bool[5];

        Center = FindOrCreateCorner(Bounds.Position, true);

        Vector2 halfSize = Bounds.Size / 2;
        Corner = new Corner[8];
        Corner[0] = FindOrCreateCorner(Bounds.Position + new Vector2(-halfSize.x, -halfSize.y), Parent == null); // bottom left
        Corner[1] = FindOrCreateCorner(Bounds.Position + new Vector2(-halfSize.x, 0), true); // left

        Corner[2] = FindOrCreateCorner(Bounds.Position + new Vector2(-halfSize.x, halfSize.y), Parent == null); // top left
        Corner[3] = FindOrCreateCorner(Bounds.Position + new Vector2(0, halfSize.y), true); // top

        Corner[4] = FindOrCreateCorner(Bounds.Position + new Vector2(halfSize.x, halfSize.y), Parent == null); // top right
        Corner[5] = FindOrCreateCorner(Bounds.Position + new Vector2(halfSize.x, 0), true); // right

        Corner[6] = FindOrCreateCorner(Bounds.Position + new Vector2(halfSize.x, -halfSize.y), Parent == null); // bottom right
        Corner[7] = FindOrCreateCorner(Bounds.Position + new Vector2(0, -halfSize.y), true); // bottom
    }

    public Corner FindCorner(Vector2 v)
    {
        if (Center.Position == v) return Center;
        if (Corner[0].Position == v) return Corner[0];
        if (Corner[1].Position == v) return Corner[1];
        if (Corner[2].Position == v) return Corner[2];
        if (Corner[3].Position == v) return Corner[3];
        if (Corner[4].Position == v) return Corner[4];
        if (Corner[5].Position == v) return Corner[5];
        if (Corner[6].Position == v) return Corner[6];
        if (Corner[7].Position == v) return Corner[7];

        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i] == null) continue;
            Corner corner = Children[i].FindCorner(v);
            if (corner != null) return corner;
        }
        return null;
    }

    public void ImportLODMesh(LODMeshObject lodMesh)
    {
        lodMesh.Children.CopyTo(Children, 0);
        lodMesh.Children[0] = null;
        lodMesh.Children[1] = null;
        lodMesh.Children[2] = null;
        lodMesh.Children[3] = null;
        ApplyValues();
    }

    public void Invalidate()
    {
        Invalid = true;
        if (Parent != null)
            Parent.Invalidate();
    }

    public void SetChildren(LODMeshObject southWest, LODMeshObject northWest, LODMeshObject southEast, LODMeshObject northEast)
    {
        Children[0] = southWest;
        Children[1] = northWest;
        Children[2] = southEast;
        Children[3] = northEast;
    }

    public void Triangulate(TriangleMesh mesh, Vector2 reference)
    {
        if (!CornerReference[0]) return;

        Corner lowerLeft = Corner[0];
        Corner left = Corner[1];
        Corner upperLeft = Corner[2];
        Corner top = Corner[3];
        Corner upperRight = Corner[4];
        Corner right = Corner[5];
        Corner lowerRight = Corner[6];
        Corner bottom = Corner[7];

        bool refLowerLeft = CornerReference[1];
        bool refUpperLeft = CornerReference[2];
        bool refUpperRight = CornerReference[3];
        bool refLowerRight = CornerReference[4];

        Evaluate(mesh, lowerLeft, bottom, lowerRight, refLowerLeft, bottom.Enabled, refLowerRight, reference);
        Evaluate(mesh, upperLeft, left, lowerLeft, refUpperLeft, left.Enabled, refLowerLeft, reference);
        Evaluate(mesh, upperRight, top, upperLeft, refUpperRight, top.Enabled, refUpperLeft, reference);
        Evaluate(mesh, lowerRight, right, upperRight, refLowerRight, right.Enabled, refUpperRight, reference);
    }

    public void Update()
    {
        StoreOldValues();
        UpdateLoD();
        UpdateSplit();
        UpdateReferences();
    }

    private void AddReferenceIfNotContained(Corner corner, ref bool b)
    {
        if (b) return;
        b = true;
        corner.References++;
    }

    private void ApplyValues()
    {
        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i] == null) continue;
            Children[i].Parent = this;
            Children[i].QuadNode = QuadNode;
            Children[i].Depth = Depth + 1;
            Children[i].TotalDepth = QuadNode.Depth + Children[i].Depth;
            Children[i].ApplyValues();
        }
    }

    private int CornerCount()
    {
        int c = 0;
        for (int i = 0; i < Children.Length; i++)
        {
            if (Corner[i * 2].Enabled) c++;
        }
        return c;
    }

    private void DestroyChildren()
    {
        RemoveReferences();

        for (int i = 0; i < 4; i++)
        {
            Children[i] = null;
        }
    }

    private bool Empty()
    {
        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i] == null) continue;
            if (!Children[i].Empty()) return false;
        }
        return true;
    }

    private void Evaluate(TriangleMesh triangleMesh, Corner start, Corner mid, Corner end, bool startEnabled, bool midEnabled, bool endEnabled, Vector2 reference)
    {
        if (startEnabled && endEnabled && midEnabled)
        {
            triangleMesh.AddTriangle(new Triangle(
                (Center.Position - reference).XYZ(Center.Height),
                (mid.Position - reference).XYZ(mid.Height),
                (start.Position - reference).XYZ(start.Height)));
            triangleMesh.AddTriangle(new Triangle(
                (end.Position - reference).XYZ(end.Height),
                (mid.Position - reference).XYZ(mid.Height),
                (Center.Position - reference).XYZ(Center.Height)));
        }
        else if (startEnabled && endEnabled)
        {
            triangleMesh.AddTriangle(new Triangle(
                (start.Position - reference).XYZ(start.Height),
                (Center.Position - reference).XYZ(Center.Height),
                (end.Position - reference).XYZ(end.Height)));
        }
        else if (startEnabled && !endEnabled)
        {
            triangleMesh.AddTriangle(new Triangle(
                (Center.Position - reference).XYZ(Center.Height),
                (mid.Position - reference).XYZ(mid.Height),
                (start.Position - reference).XYZ(start.Height)));
        }
        else if (!startEnabled && endEnabled)
        {
            triangleMesh.AddTriangle(new Triangle(
                (end.Position - reference).XYZ(end.Height),
                (mid.Position - reference).XYZ(mid.Height),
                (Center.Position - reference).XYZ(Center.Height)));
        }
    }

    private Corner FindOrCreateCorner(Vector2 position, bool createNew)
    {
        Corner corner = QuadNode.QuadTree.FindCorner(position);
        if (corner == null)
        {
            corner = new Corner() { Position = position, QuadTree = QuadNode.QuadTree };
        }
        return corner;
    }

    private void Merge()
    {
        Invalidate();
        Center.HeightChanged += HeightChanged;
        Center.EnabledChanged += EnabledChanged;
        //Center.ReferencesChanged += ReferencesChanged;
        for (int i = 0; i < Corner.Length; i++)
        {
            Corner[i].HeightChanged += HeightChanged;
            Corner[i].EnabledChanged += EnabledChanged;
            //Corner[i].ReferencesChanged += ReferencesChanged;
        }
        RemoveReferences();
    }

    private void RemoveReferenceIfContained(Corner corner, ref bool b)
    {
        if (!b) return;
        b = false;
        corner.References--;
    }

    private void RemoveReferences()
    {
        for (int i = 0; i < Children.Length; i++)
        {
            RemoveReferenceIfContained(Corner[i * 2], ref CornerReference[i + 1]);
        }
        RemoveReferenceIfContained(Center, ref CornerReference[0]);

        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i] == null) continue;
            Children[i].DestroyChildren();
        }
    }

    private void Split()
    {
        Invalidate();
        Center.HeightChanged += HeightChanged;
        Center.EnabledChanged += EnabledChanged;
        //Center.ReferencesChanged += ReferencesChanged;
        for (int i = 0; i < Corner.Length; i++)
        {
            Corner[i].HeightChanged += HeightChanged;
            Corner[i].EnabledChanged += EnabledChanged;
            //Corner[i].ReferencesChanged += ReferencesChanged;
        }
        for (int i = 0; i < Children.Length; i++)
        {
            Children[i] = new LODMeshObject(this, QuadNode, Bounds.Split((Quadrant)i), Depth + 1);
        }
    }

    private void EnabledChanged(object sender, EventArgs e)
    {
        Invalidate();
    }

    private void HeightChanged(object sender, EventArgs e)
    {
        Invalidate();
    }

    private void StoreOldValues()
    {
        LastActive = Active;
        LastCornerActive = CornerActive;

        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i] == null) continue;
            Children[i].StoreOldValues();
        }
    }

    private void UpdateLoD()
    {
        LoD = QuadNode.QuadTree.LODReference.LoD(Bounds);

        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i] == null) continue;
            Children[i].UpdateLoD();
        }
    }

    private void UpdateReferences()
    {
        int c = 0;
        for (int i = 0; i < 4; i++)
        {
            if (Children[i] == null || !(Children[i].Active || Children[i].CornerActive))
                AddReferenceIfNotContained(Corner[i * 2], ref CornerReference[i + 1]);
            else
            {
                Children[i].UpdateReferences();
                RemoveReferenceIfContained(Corner[i * 2], ref CornerReference[i + 1]);
                c++;
            }
        }
        if (c == 4)
            RemoveReferenceIfContained(Center, ref CornerReference[0]);
        else
            AddReferenceIfNotContained(Center, ref CornerReference[0]);
    }

    private void UpdateSplit()
    {
        if (Depth <= LoD)
        {
            Active = Parent == null || (Parent.Active || Parent.CornerActive);
            CornerActive = false;
        }
        else
        {
            Active = false;
            CornerActive =
                (
                    Corner[1].Enabled || Corner[3].Enabled || Corner[5].Enabled || Corner[7].Enabled
                ) ||
                (
                    Children[0] != null && (Children[0].Active || Children[0].CornerActive) ||
                    Children[1] != null && (Children[1].Active || Children[1].CornerActive) ||
                    Children[2] != null && (Children[2].Active || Children[2].CornerActive) ||
                    Children[3] != null && (Children[3].Active || Children[3].CornerActive)
                );
        }

        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i] == null) continue;
            Children[i].UpdateSplit();
        }

        if (Active || CornerActive)
        {
            if (!(LastActive || LastCornerActive))
            {
                Invalidate();
                Split();
            }
        }
        else
        {
            if (LastActive || LastCornerActive)
            {
                Invalidate();
                Merge();
            }
        }

        //if (Active || CornerActive)
        //{
        //    EvaluateReferences();

        //    if (!(LastActive || LastCornerActive))
        //    {
        //        OnInvalidate();
        //        Center.HeightChanged += HeightChanged;
        //        Center.EnabledChanged += EnabledChanged;
        //        //Center.ReferencesChanged += ReferencesChanged;
        //        for (int i = 0; i < Corner.Length; i++)
        //        {
        //            Corner[i].HeightChanged += HeightChanged;
        //            Corner[i].EnabledChanged += EnabledChanged;
        //            //Corner[i].ReferencesChanged += ReferencesChanged;
        //        }

        //        for (int i = 0; i < 4; i++)
        //        {
        //            Children[i] = new ChunkTree(Terrain, Chunk, this, (Center.Position + Corner[i * 2].Position) / 2, EdgeLength / 2, Depth + 1);
        //        }
        //    }
        //}
        //else
        //{
        //    if (LastActive || LastCornerActive)
        //    {
        //        OnInvalidate();
        //        RemoveReferences();
        //    }
        //}

        //for (int i = 0; i < Children.Length; i++)
        //{
        //    if (Children[i] == null) continue;
        //    Children[i].UpdateSplit();
        //}

        //if (TotalDepth <= LoD)
        //{
        //    if (!(Active || CornerActive))
        //    {
        //        Split();
        //    }
        //}
        //else
        //{
        //    /*(
        //                Corner[1].Enabled || Corner[3].Enabled || Corner[5].Enabled || Corner[7].Enabled
        //            ) ||
        //            (
        //                Children[0] != null && (Children[0].Active || Children[0].CornerActive) ||
        //                Children[1] != null && (Children[1].Active || Children[1].CornerActive) ||
        //                Children[2] != null && (Children[2].Active || Children[2].CornerActive) ||
        //                Children[3] != null && (Children[3].Active || Children[3].CornerActive)
        //            );*/
        //    if (
        //        (
        //            Corner[1].Enabled || Corner[3].Enabled || Corner[5].Enabled || Corner[7].Enabled
        //        ) || (
        //            Children[0] != null && (Children[0].Active || Children[0].CornerActive) ||
        //            Children[1] != null && (Children[1].Active || Children[1].CornerActive) ||
        //            Children[2] != null && (Children[2].Active || Children[2].CornerActive) ||
        //            Children[3] != null && (Children[3].Active || Children[3].CornerActive)
        //        ))
        //    {
        //        if (!(Active || CornerActive))
        //        {
        //            Split();
        //        }
        //    }
        //    else if (!Active && CornerCount() > 2)
        //    {
        //        Split();
        //        CornerActive = true;
        //        Active = false;
        //    }
        //    else
        //    {
        //        if (Active || CornerActive)
        //        {
        //            Active = false;
        //            CornerActive = false;
        //            Merge();
        //        }
        //    }
        //}
    }
}

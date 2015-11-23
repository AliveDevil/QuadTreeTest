using System.Collections.Generic;
using UnityEngine;

public class QuadTree : MonoBehaviour
{
    public LODReference LODReference;
    public SimplexNoise Noise;
    public QuadTreeNodePool Pool;
    public QuadTreeNode Root;

    public Corner FindCorner(Vector2 v)
    {
        Stack<QuadTreeNode> tree = new Stack<QuadTreeNode>(4);
        tree.Push(Root);
        while (tree.Count > 0)
        {
            QuadTreeNode node = tree.Pop();
            if (node == null || !QuadTreeNode.Encapsulates(node, v)) continue;
            Corner corner = node.FindCorner(v);
            if (corner != null) return corner;
            if (node.Active) continue;
            tree.Push(node.Children[0]);
            tree.Push(node.Children[1]);
            tree.Push(node.Children[2]);
            tree.Push(node.Children[3]);
        }
        return null;
    }

    public void Grow(Vector2 direction)
    {
        QuadTreeNode tempRoot = null;
        if (direction == new Vector2(1, 1))
        {
            tempRoot = new QuadTreeNode(this, null, Root.AAR.Position + new Vector2(Root.AAR.Size.x / 2, Root.AAR.Size.y / 2), (Root.AAR.Size.x + Root.AAR.Size.y) / 2, Root.MinSize, 0);
            tempRoot.MeshObject.Children[0] = Root.MeshObject;
        }
        if (direction == new Vector2(-1, -1))
        {
            tempRoot = new QuadTreeNode(this, null, Root.AAR.Position + new Vector2(-Root.AAR.Size.x / 2, -Root.AAR.Size.y / 2), (Root.AAR.Size.x + Root.AAR.Size.y) / 2, Root.MinSize, 0);
            tempRoot.MeshObject.Children[2] = Root.MeshObject;
        }
        if (direction == new Vector2(1, -1))
        {
            tempRoot = new QuadTreeNode(this, null, Root.AAR.Position + new Vector2(Root.AAR.Size.x / 2, -Root.AAR.Size.y / 2), (Root.AAR.Size.x + Root.AAR.Size.y) / 2, Root.MinSize, 0);
            tempRoot.MeshObject.Children[1] = Root.MeshObject;
        }
        if (direction == new Vector2(-1, 1))
        {
            tempRoot = new QuadTreeNode(this, null, Root.AAR.Position + new Vector2(-Root.AAR.Size.x / 2, Root.AAR.Size.y / 2), (Root.AAR.Size.x + Root.AAR.Size.y) / 2, Root.MinSize, 0);
            tempRoot.MeshObject.Children[3] = Root.MeshObject;
        }

        Root.DestroyGameObject();
        Root = tempRoot;
        Root.ApplyValues();
    }

    public float Sample(Vector2 v)
    {
        float result = 0;
        float frequency = 1f / 2048;
        float amplitude = 150;
        float lacunarity = 3.8729833462074168851792653997824f;
        float invLacunarity = 1 / lacunarity;
        for (int i = 0; i < 8; i++)
        {
            result += Noise.Noise(v * frequency, amplitude);
            frequency *= lacunarity;
            amplitude *= invLacunarity;
        }
        return result;
    }

    public void Start()
    {
        Noise = new SimplexNoise();
        Noise.Initialize();
        Root = new QuadTreeNode(this, null, Vector2.zero, Helper.ViewDistance, 4, 0);
    }

    public void Update()
    {
        if (!Root.AAR.Inner((LODReference.Position / (Helper.ViewDistance)).Round() * Helper.ViewDistance))
        {
            Grow((LODReference.Position - Root.Position).One());
        }
        //Root.Update();
    }
}

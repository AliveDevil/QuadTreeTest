using UnityEngine;

public class QuadTreeMergeBehaviour : MonoBehaviour
{
    public QuadTreeNode Node;

    private void Update()
    {
        if (!Node.Active && !Node.Empty() && Node.CanMerge())
        {
            Node.Merge();
        }
    }
}

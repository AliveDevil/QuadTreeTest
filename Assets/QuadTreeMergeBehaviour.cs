using UnityEngine;

public class QuadTreeMergeBehaviour : MonoBehaviour
{
    public QuadTreeNode Node;

    private void Update()
    {
        if (Node.ShouldMerge())
        {
            //Node.Merge();
        }
    }
}

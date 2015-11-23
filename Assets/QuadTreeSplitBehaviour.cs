using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class QuadTreeSplitBehaviour : MonoBehaviour
{
    public QuadTreeNode Node;

    private void Update()
    {
        if (Node.Active && Node.CanSplit())
        {
            //Node.Split();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class QuadTreeNodeBehaviour : MonoBehaviour
{
    public QuadTreeNode Node;

    private void Update()
    {
        Node.Update();
    }
}

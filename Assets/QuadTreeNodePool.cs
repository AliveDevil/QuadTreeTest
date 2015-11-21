using System.Collections.Generic;
using UnityEngine;

public class QuadTreeNodePool : MonoBehaviour
{
    public GameObject Prefab;
    private Stack<GameObject> gameObjects;

    public GameObject Get()
    {
        EnsureItem();
        GameObject gameObject = gameObjects.Pop();
        gameObject.SetActive(true);
        return gameObject;
    }

    public void Put(ref GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(transform, false);
        gameObjects.Push(gameObject);
        gameObject = null;
    }

    private void Awake()
    {
        gameObjects = new Stack<GameObject>();
    }

    private void EnsureItem()
    {
        if (gameObjects.Count > 0) return;
        gameObjects.Push(Instantiate(Prefab)._(_ =>
        {
            _.SetActive(false);
            _.transform.SetParent(transform, false);
        }));
    }
}

using UnityEngine;

public class SingletonTool : MonoBehaviour
{
    private static SingletonTool _instance;

    private void Awake()
    {
        var existingInstance = GameObject.Find(gameObject.name)?.GetComponent<SingletonTool>();

        if (existingInstance != null && existingInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
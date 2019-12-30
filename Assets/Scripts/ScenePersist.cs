using UnityEngine;

public class ScenePersist : MonoBehaviour
{
    private void Awake()
    {
        int scenePersistNumber = FindObjectsOfType<ScenePersist>().Length;

        if (scenePersistNumber > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }
}
using UnityEngine;

public static class GameManager
{
    public static GameUIManager UI { get { return GameUIManager.Instance; } }
    public static GameSceneManager Scene { get { return GameSceneManager.Instance; } }
    public static GameSoundManager Sound { get { return GameSoundManager.Instance; } }
}

public class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>() ?? new GameObject(typeof(T).Name).AddComponent<T>();
                DontDestroyOnLoad(instance.gameObject);
            }

            return instance;
        }
    }
}
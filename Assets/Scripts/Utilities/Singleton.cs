using UnityEngine;

/// <summary>
/// Inherit from this base class to create a singleton.
/// e.g. public class MyClassName : Singleton<MyClassName> {}
/// http://wiki.unity3d.com/index.php?title=Singleton
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
#pragma warning disable UDR0001 // Domain Reload Analyzer
    private static object m_Lock = new object();
    private static T m_Instance;
#pragma warning restore UDR0001 // Domain Reload Analyzer

    public bool dontDestroyOnLoad = true;

    /// <summary>
    /// Access singleton instance through this propriety.
    /// </summary>
    public static T Instance
    {
        get
        {
            lock (m_Lock)
            {
                if (m_Instance == null)
                {
                    // Search for existing instance.
                    m_Instance = (T)FindObjectOfType(typeof(T));

                    // Create new instance if one doesn't already exist.
                    if (m_Instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        m_Instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";

                        //make sure its not getting called during the editor mode
                        if ((Application.isPlaying) && (m_Instance as Singleton<T>).dontDestroyOnLoad)
                        {
                            // Make instance persistent.
                            DontDestroyOnLoad(singletonObject);
                        }

                    }
                }

                return m_Instance;
            }
        }
    }
}
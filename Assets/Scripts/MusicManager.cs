using UnityEngine;

public class MusicManager : MonoBehaviour
{
    void Awake()
    {
        // Check if there is already an instance of MusicManager
        if (FindObjectsByType<MusicManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Set this object to not be destroyed on load
        DontDestroyOnLoad(gameObject);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Thin navigation helper. Validates the target scene before loading so a missing or misnamed scene
/// fails with a clear, actionable log instead of a hard runtime error (or a silent no-op).
/// Kept in the global namespace on purpose — buttons reference navigation methods via UnityEvent by
/// type name, and a namespace change would break those bindings.
/// </summary>
public class NavigationController : MonoBehaviour
{
    /// <summary>Loads a scene by name if it exists in Build Settings.</summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[Navigation] LoadScene called with an empty scene name.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"[Navigation] Scene '{sceneName}' is not in Build Settings — add it to load it.", this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}

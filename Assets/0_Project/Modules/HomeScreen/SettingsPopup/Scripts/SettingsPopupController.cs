using UnityEngine;

/// <summary>
/// Drives the Settings popup's close flow: plays the close animation, then deactivates on the
/// animation-event callback. Kept in the global namespace on purpose — the close button's UnityEvent and
/// an animation event reference this type/method by name, and a namespace change would break those
/// bindings. Method names are preserved for the same reason.
/// </summary>
public class SettingsPopupController : MonoBehaviour
{
    // Hashed once instead of re-hashing the "Close" string on every click.
    private static readonly int CloseTrigger = Animator.StringToHash("Close");

    [SerializeField] private Animator animator;

    public void OnCloseButtonClicked()
    {
        if (animator != null)
            animator.SetTrigger(CloseTrigger);
    }

    // Called via animation event when the close animation completes.
    public void OnClosedAnimationCompleted()
    {
        gameObject.SetActive(false);
    }
}

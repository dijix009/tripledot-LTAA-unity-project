using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TripleDot.HomeScreen.Footer
{
    /// <summary>
    /// A single button in the <see cref="BottomBarView"/>.
    /// Owns only its own visual state (selected / locked) and the optional content panel it toggles.
    /// It raises <see cref="Clicked"/> upward and lets the <see cref="BottomBarView"/> decide selection
    /// logic — the button never knows about its siblings or about navigation.
    /// </summary>
    [DisallowMultipleComponent]
    public class BottomBarButton : MonoBehaviour
    {
        // Cached once for the whole type — avoids the per-call string hashing the old code did with
        // animator.SetBool("Selected", ...) / SetBool("Locked", ...).
        private static readonly int SelectedParam = Animator.StringToHash("Selected");
        private static readonly int LockedParam = Animator.StringToHash("Locked");

        [Header("Components")]
        [SerializeField] private Animator animator;
        [FormerlySerializedAs("footerBtn")]
        [SerializeField] private Button button;

        [Header("Content")]
        [Tooltip("Panel this button toggles on when selected. May be left empty for buttons that only " +
                 "navigate without owning a content panel.")]
        [SerializeField] private GameObject content;

        [Header("State")]
        [FormerlySerializedAs("lockOnAwake")]
        [SerializeField] private bool lockedOnAwake;

        /// <summary>Raised when the user taps this button (only fires while unlocked).</summary>
        public event Action<BottomBarButton> Clicked;

        public bool IsLocked { get; private set; }
        public bool IsSelected { get; private set; }
        public GameObject Content => content;

        private void Awake()
        {
            SetLocked(lockedOnAwake);
            SetSelected(false);
            ShowContent(false);
        }

        private void OnEnable()
        {
            if (button != null)
                button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick() => Clicked?.Invoke(this);

        public void SetLocked(bool locked)
        {
            IsLocked = locked;

            if (button != null)
                button.interactable = !locked;

            if (animator != null)
                animator.SetBool(LockedParam, locked);
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;

            if (animator != null)
                animator.SetBool(SelectedParam, selected);

            ShowContent(selected);
        }

        private void ShowContent(bool visible)
        {
            if (content != null)
                content.SetActive(visible);
        }
    }
}

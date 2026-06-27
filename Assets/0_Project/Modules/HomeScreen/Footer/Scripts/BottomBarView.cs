using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace TripleDot.HomeScreen.Footer
{
    /// <summary>
    /// The bottom navigation bar required by the brief.
    /// Owns selection logic for its <see cref="BottomBarButton"/>s, drives the selection indicator, and
    /// exposes the two contract events the brief asks for:
    /// <list type="bullet">
    ///   <item><see cref="ContentActivated"/> — a button toggled ON its content.</item>
    ///   <item><see cref="Closed"/> — the active button toggled OFF, so no content is shown.</item>
    /// </list>
    /// Both are surfaced as C# events (for code) and serialized <see cref="UnityEvent"/>s (for designers).
    /// The bar knows nothing about <em>what</em> the content is — listeners decide, which keeps it reusable
    /// across screens. All timings are serialized for designers to tune.
    /// </summary>
    [DisallowMultipleComponent]
    public class BottomBarView : MonoBehaviour
    {
        // Concrete subclass so the generic UnityEvent is serialized and shows in the Inspector.
        [Serializable] public class BottomBarButtonEvent : UnityEvent<BottomBarButton> { }

        [Header("Buttons")]
        [FormerlySerializedAs("footerButtons")]
        [SerializeField] private List<BottomBarButton> buttons = new();
        [Tooltip("Optional button selected when the bar first appears. Leave empty to start with all off.")]
        [FormerlySerializedAs("startSelected")]
        [SerializeField] private BottomBarButton initiallySelected;

        [Header("Selection indicator")]
        [SerializeField] private GameObject indicator;

        [Header("Animation")]
        [SerializeField] private Animator barAnimator;
        [Min(0f)] [SerializeField] private float indicatorMoveDuration = 0.25f;
        [SerializeField] private Ease indicatorMoveEase = Ease.OutSine;
        [SerializeField] private string appearState = "Footer_Entrance";
        [SerializeField] private string disappearState = "Footer_Exit";

        [Header("Events")]
        [SerializeField] private BottomBarButtonEvent onContentActivated;
        [SerializeField] private UnityEvent onClosed;

        /// <summary>Raised when a button toggles ON its content.</summary>
        public event Action<BottomBarButton> ContentActivated;

        /// <summary>Raised when the active button is toggled off and nothing is shown.</summary>
        public event Action Closed;

        private BottomBarButton _active;
        private Tween _indicatorTween;

        private void OnEnable()
        {
            foreach (var button in buttons)
            {
                if (button != null)
                    button.Clicked += HandleButtonClicked;
            }
        }

        private void OnDisable()
        {
            _indicatorTween?.Kill();

            foreach (var button in buttons)
            {
                if (button != null)
                    button.Clicked -= HandleButtonClicked;
            }
        }

        private void Start()
        {
            if (initiallySelected != null)
                Select(initiallySelected, animateIndicator: false);
            else
                Deselect();
        }

        /// <summary>Plays the bar's entrance animation.</summary>
        public void Appear()
        {
            if (barAnimator != null)
                barAnimator.Play(appearState);
        }

        /// <summary>Plays the bar's exit animation.</summary>
        public void Disappear()
        {
            if (barAnimator != null)
                barAnimator.Play(disappearState);
        }

        private void HandleButtonClicked(BottomBarButton button)
        {
            if (button == null || button.IsLocked || !buttons.Contains(button))
                return;

            // Tapping the already-active button closes the bar's content.
            if (_active == button)
                Deselect();
            else
                Select(button, animateIndicator: true);
        }

        private void Select(BottomBarButton button, bool animateIndicator)
        {
            _active = button;

            foreach (var b in buttons)
            {
                if (b != null)
                    b.SetSelected(b == button);
            }

            MoveIndicatorTo(button, animateIndicator);

            ContentActivated?.Invoke(button);
            onContentActivated?.Invoke(button);
        }

        private void Deselect()
        {
            _active = null;
            _indicatorTween?.Kill();

            foreach (var b in buttons)
            {
                if (b != null)
                    b.SetSelected(false);
            }

            if (indicator != null)
                indicator.SetActive(false);

            Closed?.Invoke();
            onClosed?.Invoke();
        }

        // The selected button's width is animated (Selected.anim drives LayoutElement.preferredWidth), so
        // the HorizontalLayoutGroup reflows every button's position over the course of the animation. A
        // one-shot tween to a snapshot therefore lands on a stale position — and the leftover offset
        // depended on which button you came from, which is why the indicator drifted and flipped sides.
        // The tween below re-reads the button's *live* centre every tick and lerps toward it, so it
        // converges on the true final position no matter how the layout reflows mid-animation.
        private void MoveIndicatorTo(BottomBarButton button, bool animate)
        {
            if (indicator == null)
                return;

            indicator.SetActive(true);
            var rt = (RectTransform)indicator.transform;
            _indicatorTween?.Kill();

            if (!animate)
            {
                SetIndicatorLocalX(rt, GetButtonCentreLocalX(button, rt));
                return;
            }

            float startX = rt.localPosition.x;
            _indicatorTween = DOVirtual.Float(0f, 1f, indicatorMoveDuration, t =>
                {
                    float liveTargetX = GetButtonCentreLocalX(button, rt);
                    SetIndicatorLocalX(rt, Mathf.LerpUnclamped(startX, liveTargetX, t));
                })
                .SetEase(indicatorMoveEase)
                .SetTarget(rt)
                .OnComplete(() => SetIndicatorLocalX(rt, GetButtonCentreLocalX(button, rt)));
        }

        // Button's true visual centre (pivot-agnostic), expressed in the indicator's own parent space.
        private static float GetButtonCentreLocalX(BottomBarButton button, RectTransform indicatorRt)
        {
            var buttonRt = (RectTransform)button.transform;
            Vector3 worldCentre = buttonRt.TransformPoint(buttonRt.rect.center);
            return indicatorRt.parent.InverseTransformPoint(worldCentre).x;
        }

        private static void SetIndicatorLocalX(RectTransform rt, float x)
        {
            Vector3 lp = rt.localPosition;
            rt.localPosition = new Vector3(x, lp.y, lp.z);
        }

        /// <summary>Locks or unlocks a button at runtime (e.g. when a feature unlocks).</summary>
        public void SetButtonLocked(BottomBarButton button, bool locked)
        {
            if (button == null)
                return;

            button.SetLocked(locked);

            if (locked && _active == button)
                Deselect();
        }
    }
}

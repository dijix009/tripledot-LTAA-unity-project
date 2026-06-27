using UnityEngine;

namespace TripleDot.HomeScreen.Footer
{
    /// <summary>
    /// Small verification/demo aid (not production UI): logs the <see cref="BottomBarView"/>'s
    /// <see cref="BottomBarView.ContentActivated"/> / <see cref="BottomBarView.Closed"/> events so the
    /// contract is observable in Play mode. Lives on the same GameObject as the view and finds it via
    /// <see cref="RequireComponent"/>, so it needs no Inspector wiring.
    /// </summary>
    [RequireComponent(typeof(BottomBarView))]
    public class BottomBarDemoListener : MonoBehaviour
    {
        private BottomBarView _view;

        private void Awake() => _view = GetComponent<BottomBarView>();

        private void OnEnable()
        {
            if (_view == null)
                return;

            _view.ContentActivated += HandleContentActivated;
            _view.Closed += HandleClosed;
        }

        private void OnDisable()
        {
            if (_view == null)
                return;

            _view.ContentActivated -= HandleContentActivated;
            _view.Closed -= HandleClosed;
        }

        private void HandleContentActivated(BottomBarButton button)
        {
            Debug.Log($"[BottomBar] ContentActivated → {button.name}", button);
        }

        private void HandleClosed()
        {
            Debug.Log("[BottomBar] Closed (all buttons off)", this);
        }
    }
}

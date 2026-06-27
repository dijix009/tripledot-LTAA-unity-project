using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable frosted-glass backdrop for popups (closes the brief's "background darkening AND blur"
/// requirement). On enable it briefly hides the popup, grabs a snapshot of whatever is behind it,
/// runs a few cheap Kawase blur passes on a downsampled copy, and shows the result on its RawImage —
/// tinted darker for the darkening half. Drop it on a full-screen RawImage child of any popup and point
/// <see cref="popupGroup"/> at the popup's CanvasGroup; nothing else is popup-specific, so other popups
/// reuse it as-is.
///
/// Note: capturing the frame requires the popup to be momentarily transparent (one frame) so the blur
/// shows the background and not the popup itself. Tune <see cref="iterations"/>/<see cref="offset"/>/
/// <see cref="downsample"/> to taste.
/// </summary>
[RequireComponent(typeof(RawImage))]
[DisallowMultipleComponent]
public class PopupBlurBackground : MonoBehaviour
{
    [Header("Blur")]
    [Tooltip("Hidden/UI/KawaseBlur material shader.")]
    [SerializeField] private Shader blurShader;
    [Range(1, 8)] [SerializeField] private int iterations = 3;
    [Tooltip("Resolution divisor for the blur buffers — higher = cheaper & softer.")]
    [Range(1, 8)] [SerializeField] private int downsample = 2;
    [Min(0f)] [SerializeField] private float offset = 1.5f;

    [Header("Darkening")]
    [Tooltip("Multiplied over the blurred image — darken with values < 1.")]
    [SerializeField] private Color tint = new Color(0.55f, 0.55f, 0.6f, 1f);

    [Header("Capture")]
    [Tooltip("Popup CanvasGroup, hidden for one frame so the snapshot captures only the background.")]
    [SerializeField] private CanvasGroup popupGroup;

    private RawImage _image;
    private Material _blurMaterial;
    private RenderTexture _result;

    private void Awake()
    {
        _image = GetComponent<RawImage>();
        if (blurShader != null)
            _blurMaterial = new Material(blurShader) { hideFlags = HideFlags.HideAndDontSave };
    }

    private void OnEnable() => StartCoroutine(CaptureAndBlur());

    private void OnDisable()
    {
        StopAllCoroutines();
        ReleaseResult();
    }

    private void OnDestroy()
    {
        ReleaseResult();
        if (_blurMaterial != null)
            Destroy(_blurMaterial);
    }

    private IEnumerator CaptureAndBlur()
    {
        if (_blurMaterial == null)
        {
            Debug.LogWarning("[PopupBlur] No blur shader assigned — skipping.", this);
            yield break;
        }

        // Hide the popup (and our own image) so the snapshot only contains what's behind it.
        float restore = popupGroup != null ? popupGroup.alpha : 1f;
        if (popupGroup != null)
            popupGroup.alpha = 0f;
        _image.enabled = false;

        yield return new WaitForEndOfFrame();

        Texture2D shot = ScreenCapture.CaptureScreenshotAsTexture();

        int w = Mathf.Max(1, shot.width / downsample);
        int h = Mathf.Max(1, shot.height / downsample);
        RenderTexture a = RenderTexture.GetTemporary(w, h, 0);
        RenderTexture b = RenderTexture.GetTemporary(w, h, 0);

        Graphics.Blit(shot, a);
        for (int i = 0; i < iterations; i++)
        {
            _blurMaterial.SetFloat("_Offset", offset + i);
            Graphics.Blit(a, b, _blurMaterial);
            (a, b) = (b, a);
        }

        ReleaseResult();
        _result = new RenderTexture(w, h, 0);
        Graphics.Blit(a, _result);

        RenderTexture.ReleaseTemporary(a);
        RenderTexture.ReleaseTemporary(b);
        Destroy(shot);

        _image.texture = _result;
        _image.color = tint;          // darken via multiply
        _image.enabled = true;

        if (popupGroup != null)
            popupGroup.alpha = restore;
    }

    private void ReleaseResult()
    {
        if (_result == null)
            return;

        if (_image != null && _image.texture == _result)
            _image.texture = null;

        _result.Release();
        Destroy(_result);
        _result = null;
    }
}

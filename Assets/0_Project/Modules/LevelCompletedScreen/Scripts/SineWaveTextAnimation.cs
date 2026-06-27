using TMPro;
using UnityEngine;

/// <summary>
/// Floats each character on a sine wave. Unlike the original, it does NOT call <c>ForceMeshUpdate()</c>
/// (a full text re-layout) or allocate a fresh vertex array every frame — it caches the base vertices
/// once (and only re-caches when the text actually changes) and offsets them into TMP's reused mesh
/// buffers. Tunables are exposed for designers. (Left in the global namespace to match the existing
/// project convention and avoid touching any name-based bindings.)
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class SineWaveTextAnimation : MonoBehaviour
{
    [Tooltip("Vertical float distance, in TMP units.")]
    [SerializeField] private float amplitude = 5f;
    [Tooltip("Wave speed.")]
    [SerializeField] private float frequency = 2f;
    [Tooltip("Phase offset between consecutive characters.")]
    [SerializeField] private float waveOffsetPerChar = 0.2f;

    private TMP_Text _text;
    private TMP_MeshInfo[] _baseMesh;
    private bool _ready;

    private void Awake() => _text = GetComponent<TMP_Text>();

    private void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        _text.ForceMeshUpdate();                       // populate textInfo once, on enable
        CacheBaseVertices();
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }

    // TMP raises this AFTER it has already rebuilt the mesh. We must only COPY here — calling
    // ForceMeshUpdate() in this handler would re-raise the event and recurse infinitely (stack
    // overflow / editor crash), which is exactly what the first version did.
    private void OnTextChanged(Object changed)
    {
        if (changed == _text)
            CacheBaseVertices();
    }

    private void CacheBaseVertices()
    {
        _baseMesh = _text.textInfo.CopyMeshInfoVertexData();
        _ready = _baseMesh != null && _baseMesh.Length > 0;
    }

    private void Update()
    {
        if (!_ready)
            return;

        TMP_TextInfo textInfo = _text.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo c = textInfo.characterInfo[i];
            if (!c.isVisible)
                continue;

            float wave = Mathf.Sin(Time.time * frequency + i * waveOffsetPerChar) * amplitude;
            Vector3 offset = new Vector3(0f, wave, 0f);

            Vector3[] src = _baseMesh[c.materialReferenceIndex].vertices;
            Vector3[] dst = textInfo.meshInfo[c.materialReferenceIndex].vertices;
            int v = c.vertexIndex;
            dst[v + 0] = src[v + 0] + offset;
            dst[v + 1] = src[v + 1] + offset;
            dst[v + 2] = src[v + 2] + offset;
            dst[v + 3] = src[v + 3] + offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            _text.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}

using UnityEngine;

namespace VRCrowdSourcing.BackendIntegration
{
    /// <summary>
    /// Builds a glowing beacon beam + billboard icon entirely in code.
    /// IMPORTANT: Initialize() must be called BEFORE XRSimpleInteractable is
    /// added to the same GameObject, because Initialize() creates the
    /// CapsuleCollider that XRI needs to detect ray hits.
    /// </summary>
    [RequireComponent(typeof(ProposalMarker))]
    public class BeaconMarker : MonoBehaviour
    {
        // ── Inspector tunables ────────────────────────────────────────────────
        [Header("Beam")]
        [SerializeField] private float beamHeight = 100f;
        [SerializeField] private float beamRadius = 0.5f;
        [SerializeField] private float pulseSpeed = 1.8f;
        [SerializeField] private float pulseMinAlpha = 0.20f;
        [SerializeField] private float pulseMaxAlpha = 1f;

        //[Header("Halo ring at base")]
        //[SerializeField] private float haloRadius = 15f;
        //[SerializeField] private float haloHeight = 1f;

        [Header("Ground Pulse")]
        [SerializeField] private Texture2D ringTexture;

        [Header("Pin sphere")]
        [SerializeField] private float pinRadius = 5f;

        [Header("Billboard icon")]
        [SerializeField] private float iconSize = 20f;
        [SerializeField] private float iconHeight = 100f;

        // ── Runtime state ─────────────────────────────────────────────────────
        private BeaconIconConfig _iconConfig;
        private Color _color;
        private Material _beamMat;
       // private Material _haloMat;
        private Transform _billboardRoot;
        private Camera _mainCam;

        // ══════════════════════════════════════════════════════════════════════
        //  Public API
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Call this BEFORE adding XRSimpleInteractable to the same GameObject.
        /// It creates the CapsuleCollider that XRI needs to register ray hits.
        /// </summary>
        public void Initialize(ProposalData data, BeaconIconConfig iconConfig)
        {
            _iconConfig = iconConfig;
            _color = _iconConfig != null ? _iconConfig.GetColor(data.category) : Color.white;
            BuildBeacon(data.category);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Construction
        // ══════════════════════════════════════════════════════════════════════

        private void BuildBeacon(string category)
        {
            // 1. Root collider FIRST — must exist before XRSimpleInteractable is added
            BuildInteractionCollider();

            // 2. All visual children — their colliders are destroyed immediately
            BuildBeam();
            BuildGroundPulse();
            BuildPin();
            BuildBillboard(category);
        }

        /// <summary>
        /// Adds a CapsuleCollider that covers the full beacon height.
        /// Called first inside BuildBeacon so XRI always finds it.
        /// </summary>
        private void BuildInteractionCollider()
        {
            // Don't duplicate if already present (e.g. prefab path)
            if (TryGetComponent<CapsuleCollider>(out _)) return;

            var col = gameObject.AddComponent<CapsuleCollider>();
            col.radius = pinRadius;
            col.height = iconHeight + 20f;
            col.center = new Vector3(0f, (iconHeight + pinRadius) * 0.5f, 0f);

            Debug.Log($"BeaconMarker: CapsuleCollider added — radius={col.radius} height={col.height}");
        }

        // ── Vertical glowing beam ─────────────────────────────────────────────
        private void BuildBeam()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "BeaconBeam";
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, beamHeight * 0.5f, 0f);
            go.transform.localScale = new Vector3(beamRadius * 2f, beamHeight * 0.5f, beamRadius * 2f);

            // DESTROY child collider — must not compete with root CapsuleCollider
            DestroyImmediate(go.GetComponent<Collider>());

            _beamMat = BuildEmissiveMaterial(_color, 0.60f);
            go.GetComponent<Renderer>().sharedMaterial = _beamMat;
        }

        // ── Expanding ground pulse rings ──────────────────────────────────────
        private void BuildGroundPulse()
        {
            for (int i = 0; i < 5; i++)
            {
                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = $"PulseRing_{i}";
                ring.transform.SetParent(transform, false);
                ring.transform.localPosition = Vector3.zero;

                DestroyImmediate(ring.GetComponent<Collider>());

                Material mat = BuildEmissiveMaterial(_color, 0.4f);
                ring.GetComponent<Renderer>().sharedMaterial = mat;

                var pulse = ring.AddComponent<BeaconRingPulse>();
                pulse.StartDelay = i * 0.2f;
                pulse.BaseScale = 5f + (i * 3f);
            }
        }

        // ── Bright sphere at the top of the beam ─────────────────────────────
        private void BuildPin()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "BeaconPin";
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, beamHeight, 0f);
            go.transform.localScale = Vector3.one * pinRadius * 2f;


            DestroyImmediate(go.GetComponent<Collider>());

            go.GetComponent<Renderer>().sharedMaterial = BuildEmissiveMaterial(_color, 1.0f);
        }

        // ── Billboard quad with category icon ─────────────────────────────────
        private void BuildBillboard(string category)
        {
            _billboardRoot = new GameObject("BillboardRoot").transform;
            _billboardRoot.SetParent(transform, false);
            _billboardRoot.localPosition = new Vector3(0f, iconHeight, 0f);

            // Background sphere
            GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bg.name = "BillboardBG";
            bg.transform.SetParent(_billboardRoot, false);
            bg.transform.localScale = Vector3.one * iconSize * 1.3f;

            DestroyImmediate(bg.GetComponent<Collider>());
            bg.GetComponent<Renderer>().sharedMaterial = BuildEmissiveMaterial(_color, 0.55f);

            // Icon quad (flat surface for proper 2D texture display)
            GameObject icon = GameObject.CreatePrimitive(PrimitiveType.Quad);
            icon.name = "BillboardIcon";
            icon.transform.SetParent(_billboardRoot, false);
            icon.transform.localPosition = new Vector3(0f, 0f, 0.01f);
            icon.transform.localScale = Vector3.one * iconSize;

            DestroyImmediate(icon.GetComponent<Collider>());

            Texture2D iconTex = _iconConfig != null ? _iconConfig.GetIcon(category) : null;
            Debug.Log($"BeaconMarker: Category={category}  Icon={(iconTex != null ? iconTex.name : "null (fallback)")}");

            icon.GetComponent<Renderer>().sharedMaterial = iconTex != null
                ? BuildIconMaterial(iconTex, _color)
                : BuildEmissiveMaterial(Color.white, 1.0f);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Per-frame
        // ══════════════════════════════════════════════════════════════════════

        private void Update()
        {
            PulseBeam();
            FaceBillboardToCamera();
        }

        private void PulseBeam()
        {
            if (_beamMat == null) return;

            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float alpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, t);
            float intensity = Mathf.Lerp(1.5f, 3.5f, t);

            Color c = _color;
            c.a = alpha;
            _beamMat.color = c;
            _beamMat.SetColor("_EmissionColor", _color * intensity);

            //if (_haloMat != null)
            //{
            //    Color hc = _color;
            //    hc.a = alpha * 0.6f;
            //    _haloMat.color = hc;
            //    _haloMat.SetColor("_EmissionColor", _color * intensity * 0.5f);
            //}
        }

        private void FaceBillboardToCamera()
        {
            if (_billboardRoot == null) return;
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam == null) return;

            _billboardRoot.LookAt(
                _billboardRoot.position + (_billboardRoot.position - _mainCam.transform.position),
                Vector3.up);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Material helpers
        // ══════════════════════════════════════════════════════════════════════

        private static Material BuildEmissiveMaterial(Color color, float alpha)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Particles/Standard Unlit")
                         ?? Shader.Find("Standard");

            Material mat = new Material(shader);

            Color c = color;
            c.a = alpha;
            mat.color = c;

            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = 3000;

            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2.5f);

            return mat;
        }

        private static Material BuildIconMaterial(Texture2D tex, Color tint)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            Material mat = new Material(shader);

            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", tint);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = 3001;

            return mat;
        }

        private Material BuildRingMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            Material mat = new Material(shader);

            mat.SetTexture("_BaseMap", ringTexture);
            mat.SetColor("_BaseColor", _color);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", _color * 4f);

            // Moved log before return (was unreachable in original)
            Debug.Log($"Ring Texture = {ringTexture}");
            return mat;
        }
    }
}
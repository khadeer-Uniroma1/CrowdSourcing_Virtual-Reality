using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRCrowdSourcing.BackendIntegration
{
    [RequireComponent(typeof(UIDocument))]
    public class XRToUIToolkitBridge : MonoBehaviour
    {
        [Header("XR")]
        public XRRayInteractor rayInteractor; // assign your XRRayInteractor (controller ray)

        [Header("Input")]
        // Assign the controller Activate/Select InputAction (e.g. from your ActionBasedController)
        public InputActionReference selectAction;

        [Header("Camera")]
        // Assign the camera used to render the UI (leave null to fall back to Camera.main)
        public Camera uiCamera;

        // Internal state
        private UIDocument uiDocument;
        private VisualElement root;
        private bool wasPressed;
        private VisualElement pressedElement;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("XRToUIToolkitBridge requires a UIDocument on the same GameObject.");
                enabled = false;
                return;
            }

            root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("UIDocument has no rootVisualElement.");
                enabled = false;
                return;
            }

            if (rayInteractor == null)
                Debug.LogWarning("XRToUIToolkitBridge: rayInteractor not assigned (assign in inspector).");

            if (selectAction != null && selectAction.action != null && !selectAction.action.enabled)
                selectAction.action.Enable();
        }

        private void OnDisable()
        {
            if (selectAction != null && selectAction.action != null && selectAction.action.enabled)
                selectAction.action.Disable();
        }

        private void Update()
        {
            if (rayInteractor == null || uiDocument == null || root == null) return;

            if (!rayInteractor.TryGetCurrent3DRaycastHit(out var hit))
            {
                EndHover();
                return;
            }

            Camera cam = uiCamera != null ? uiCamera : Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("XRToUIToolkitBridge: No camera found to project UI world point to screen.");
                EndHover();
                return;
            }

            // 1) Try screen projection + panel.Pick first
            Vector3 screenPoint3 = cam.WorldToScreenPoint(hit.point);
            if (screenPoint3.z < 0f)
            {
                EndHover();
                return;
            }

            Vector2 screenPoint = new Vector2(screenPoint3.x, screenPoint3.y);
            VisualElement picked = null;

            if (root.panel != null)
            {
                picked = root.panel.Pick(screenPoint);
                if (picked == null)
                {
                    // try inverted Y
                    Vector2 inverted = new Vector2(screenPoint.x, cam.pixelHeight - screenPoint.y);
                    picked = root.panel.Pick(inverted);
                    if (picked != null) screenPoint = inverted;
                }
            }

            // Debug snapshot of transform/localHit (helpful if worldBound mapping is needed)
            var t = uiDocument.transform;
            Vector3 localHit = t.InverseTransformPoint(hit.point);
            Debug.Log($"XRToUIToolkitBridge: transform pos={t.position} rot={t.rotation.eulerAngles} scale={t.lossyScale}");
            Debug.Log($"XRToUIToolkitBridge: hit.point={hit.point} localHit={localHit}");
            Debug.Log($"XRToUIToolkitBridge: initial screen=({screenPoint.x:F1},{screenPoint.y:F1}) picked={(picked != null ? picked.name : "null")}");

            // 2) If pick still null, compute mapping using UI document local space and root.worldBound corners
            if (picked == null)
            {
                Rect wb = root.worldBound;
                // Build 4 world-space corners at same Z as hit (approx)
                var cornersWorld = new Vector3[4];
                cornersWorld[0] = new Vector3(wb.xMin, wb.yMin, hit.point.z);
                cornersWorld[1] = new Vector3(wb.xMax, wb.yMin, hit.point.z);
                cornersWorld[2] = new Vector3(wb.xMin, wb.yMax, hit.point.z);
                cornersWorld[3] = new Vector3(wb.xMax, wb.yMax, hit.point.z);

                // Transform corners into UIDocument local space
                Vector3[] localCorners = new Vector3[4];
                for (int i = 0; i < 4; ++i)
                {
                    localCorners[i] = t.InverseTransformPoint(cornersWorld[i]);
                }

                // Find min/max in local X and Y (local panel extents)
                float minX = localCorners[0].x, maxX = localCorners[0].x;
                float minY = localCorners[0].y, maxY = localCorners[0].y;
                for (int i = 1; i < 4; ++i)
                {
                    if (localCorners[i].x < minX) minX = localCorners[i].x;
                    if (localCorners[i].x > maxX) maxX = localCorners[i].x;
                    if (localCorners[i].y < minY) minY = localCorners[i].y;
                    if (localCorners[i].y > maxY) maxY = localCorners[i].y;
                }

                float spanX = maxX - minX;
                float spanY = maxY - minY;

                // If spans are near-zero, bail (can't map)
                if (spanX == 0f || spanY == 0f)
                {
                    Debug.LogWarning("XRToUIToolkitBridge: computed zero span on panel local axes; cannot map.");
                    EndHover();
                    return;
                }

                // localHit in UIDocument local space:
                // compute normalized uv [0..1] within the local extents
                float ux = (localHit.x - minX) / spanX;
                float uy = (localHit.y - minY) / spanY;

                // Map uv to UI pixel coordinates using resolvedStyle / layout sizes
                float uiW = root.layout.width > 0 ? root.layout.width : root.resolvedStyle.width;
                float uiH = root.layout.height > 0 ? root.layout.height : root.resolvedStyle.height;

                if (uiW <= 0f || uiH <= 0f)
                {
                    Debug.LogWarning($"XRToUIToolkitBridge: root UI size invalid uiW={uiW} uiH={uiH}");
                    EndHover();
                    return;
                }

                Vector2 panelPosA = new Vector2(ux * uiW, uy * uiH);
                Vector2 panelPosB = new Vector2(ux * uiW, (1f - uy) * uiH);

                Debug.Log($"XRToUIToolkitBridge: worldBound={wb} localCorners[0]={localCorners[0]} localHit={localHit} ux={ux:F3} uy={uy:F3} uiSize=({uiW},{uiH}) panelA={panelPosA} panelB={panelPosB}");

                // Try pick using panelPosA or panelPosB
                if (root.panel != null)
                {
                    picked = root.panel.Pick(panelPosA);
                    if (picked != null)
                    {
                        screenPoint = panelPosA;
                    }
                    else
                    {
                        picked = root.panel.Pick(panelPosB);
                        if (picked != null)
                            screenPoint = panelPosB;
                    }
                }
            }

            string pickedName = picked != null ? picked.name : "null";
            Debug.Log($"XRToUIToolkitBridge: final screen=({screenPoint.x:F1},{screenPoint.y:F1}) picked={pickedName}");

            // Determine pressed state via InputAction
            bool pressed = false;
            if (selectAction != null && selectAction.action != null)
            {
                float val = 0f;
                try { val = selectAction.action.ReadValue<float>(); }
                catch { val = 0f; }
                pressed = val > 0.5f;
            }

            if (pressed && !wasPressed)
            {
                pressedElement = picked;
                Debug.Log($"XRToUIToolkitBridge: Press started on {pressedElement?.name ?? "null"}");
            }

            if (!pressed && wasPressed)
            {
                Debug.Log($"XRToUIToolkitBridge: Press released. pressedElement={pressedElement?.name ?? "null"} picked={picked?.name ?? "null"}");
                if (pressedElement != null && picked == pressedElement)
                {
                    var click = ClickEvent.GetPooled();
                    pressedElement.SendEvent(click);
                    Debug.Log($"XRToUIToolkitBridge: Sent ClickEvent to {pressedElement.name}");
                }

                pressedElement = null;
            }

            wasPressed = pressed;
        }

        private void EndHover()
        {
            wasPressed = false;
            pressedElement = null;
        }
    }
}
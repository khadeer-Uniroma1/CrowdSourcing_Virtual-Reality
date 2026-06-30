using System.Collections.Generic;
using CesiumForUnity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRCrowdSourcing.BackendIntegration
{
    public class ProposalMarkerSpawner : MonoBehaviour
    {
        [Header("Marker Mode")]
        [SerializeField] private bool usePrefabMarkers;

        [Header("Prefab Marker")]
        [SerializeField] private GameObject markerPrefab;

        [Header("Hierarchy")]
        [SerializeField] private Transform proposalPinsParent;

        [Header("Marker Settings")]
        [SerializeField] private float markerScale = 150f;
        [SerializeField] private double markerHeight = 100;

        [Header("Beacon Visuals")]
        [Tooltip("Enable to replace plain cubes with glowing beacon beams.")]
        [SerializeField] private bool useBeaconMarkers = true;

        [Header("Beacon Config")]
        [SerializeField] private BeaconIconConfig beaconIconConfig;

        // ── Public API ────────────────────────────────────────────────────────

        public void Initialize(Transform proposalPinsRoot)
        {
            proposalPinsParent = proposalPinsRoot;
            EnsureProposalPinsAnchor();
        }

        public void SpawnMarkers(List<ProposalData> proposals)
        {
            if (proposals == null || proposals.Count == 0)
            {
                Debug.LogWarning("SpawnMarkers called without any proposals.");
                return;
            }

            ResolveProposalPinsParent();
            Debug.Log($"Spawning {proposals.Count} proposal markers.");

            foreach (ProposalData proposal in proposals)
                SpawnMarker(proposal);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private void ResolveProposalPinsParent()
        {
            if (proposalPinsParent != null)
            {
                EnsureProposalPinsAnchor();
                return;
            }

            GameObject go = GameObject.Find("ProposalPins");
            if (go == null)
            {
                Debug.LogWarning("ProposalPins parent not found — markers will spawn without a parent.");
                return;
            }

            proposalPinsParent = go.transform;
            EnsureProposalPinsAnchor();
        }

        private void EnsureProposalPinsAnchor()
        {
            if (proposalPinsParent == null) return;
            if (!proposalPinsParent.TryGetComponent<CesiumGlobeAnchor>(out _))
            {
                proposalPinsParent.gameObject.AddComponent<CesiumGlobeAnchor>();
                Debug.Log("Attached CesiumGlobeAnchor to ProposalPins parent.");
            }
        }

        private void SpawnMarker(ProposalData proposal)
        {
            if (proposal == null) return;

            Debug.Log($"Creating marker for proposal {proposal.id}: {proposal.title}");

            // ── 1. Create root GameObject ─────────────────────────────────────
            GameObject marker;

            if (usePrefabMarkers && markerPrefab != null)
            {
                marker = Instantiate(markerPrefab);
            }
            else if (useBeaconMarkers)
            {
                // Empty root — BeaconMarker builds all visuals as children
                marker = new GameObject();
            }
            else
            {
                marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (marker.TryGetComponent<Renderer>(out var rend))
                    rend.material.color = beaconIconConfig != null
                        ? beaconIconConfig.GetColor(proposal.category)
                        : Color.white;
            }

            marker.name = $"Proposal_{proposal.id}_{proposal.category}";

            if (!useBeaconMarkers)
                marker.transform.localScale = Vector3.one * markerScale;

            if (proposalPinsParent != null)
                marker.transform.SetParent(proposalPinsParent, false);

            // ── 2. Rigidbody (required by XRI before XRSimpleInteractable) ─────
            if (!marker.TryGetComponent<Rigidbody>(out var rb))
                rb = marker.AddComponent<Rigidbody>();

            rb.useGravity = false;
            rb.isKinematic = true;

            // ── 3. BeaconMarker FIRST so its CapsuleCollider exists before XRI ──
            //    XRSimpleInteractable caches colliders at OnEnable time.
            //    The collider MUST be on the GameObject before XRSimpleInteractable
            //    is added, otherwise XRI won't register it for raycasting.
            if (useBeaconMarkers)
            {
                if (!marker.TryGetComponent<BeaconMarker>(out var beacon))
                    beacon = marker.AddComponent<BeaconMarker>();

                // Initialize() calls BuildInteractionCollider() which adds CapsuleCollider
                beacon.Initialize(proposal, beaconIconConfig);
            }
            // Non-beacon cube: BoxCollider already present from CreatePrimitive
            // Prefab:          assumed to have a collider baked in

            // ── 4. XRSimpleInteractable — collider guaranteed present by now ───
            if (!marker.TryGetComponent<XRSimpleInteractable>(out var interactable))
                interactable = marker.AddComponent<XRSimpleInteractable>();

            interactable.interactionLayers = InteractionLayerMask.GetMask("Ray");
            Debug.Log($"Marker '{marker.name}' layer mask = {interactable.interactionLayers.value}");

            // ── 5. ProposalMarker — wires XRI event listeners ─────────────────
            if (!marker.TryGetComponent<ProposalMarker>(out var proposalMarker))
                proposalMarker = marker.AddComponent<ProposalMarker>();

            proposalMarker.Initialize(proposal);

            // ── 6. Cesium Globe Anchor ────────────────────────────────────────
            if (!marker.TryGetComponent<CesiumGlobeAnchor>(out var anchor))
                anchor = marker.AddComponent<CesiumGlobeAnchor>();

            anchor.longitudeLatitudeHeight = new double3(
                proposal.longitude, proposal.latitude, markerHeight);
            anchor.Sync();

            Debug.Log($"Placed {marker.name} at Lat:{proposal.latitude} Lon:{proposal.longitude} H:{markerHeight}");
        }
    }
}
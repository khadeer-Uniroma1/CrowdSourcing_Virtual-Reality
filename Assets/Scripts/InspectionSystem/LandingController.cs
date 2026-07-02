using System.Collections;
using UnityEngine;
using VRCrowdSourcing.BackendIntegration;

namespace VRCrowdSourcing.InspectionSystem
{
    public class LandingController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform xrOrigin;

        [Header("Landing Settings")]
        [SerializeField, Min(0.5f)] private float landingDuration = 2.5f;

        //[Tooltip("Your markers are spawned at 100m. Set this to 97 to land about 3m above the marker location.")]
        //[SerializeField, Min(0f)] private float distanceBelowMarker = 97f;

        [Tooltip("Temporary test offset above the proposal marker.")]
        [SerializeField, Min(0f)] private float heightAboveMarker = 5f;

        private Coroutine landingRoutine;

        private void Awake()
        {
            if (xrOrigin == null)
            {
                Debug.LogError("LandingController: XR Origin is not assigned. Drag the root XR Origin here.");
            }
        }

        public void LandAtMarker(ProposalData proposal, ProposalMarker marker)
        {
            if (proposal == null || marker == null)
            {
                Debug.LogError("LandingController: Proposal or marker is null.");
                return;
            }

            if (xrOrigin == null)
            {
                Debug.LogError("LandingController: XR Origin is missing.");
                return;
            }

            if (landingRoutine != null)
                StopCoroutine(landingRoutine);

            // Vector3 targetPosition = marker.GetLandingPosition(distanceBelowMarker);
            Vector3 targetPosition = marker.GetLandingPosition(heightAboveMarker);

            Debug.Log($"LandingController: Landing at marker for proposal {proposal.id}. " + $"Marker={marker.transform.position}, Target={targetPosition}");

            landingRoutine = StartCoroutine(MoveXRigTo(targetPosition, proposal));
        }

        private IEnumerator MoveXRigTo(Vector3 targetPosition, ProposalData proposal)
        {
            Vector3 startPosition = xrOrigin.position;
            float elapsed = 0f;

            while (elapsed < landingDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / landingDuration);
                t = t * t * (3f - 2f * t);

                xrOrigin.position = Vector3.Lerp(startPosition, targetPosition, t);

                yield return null;
            }

            xrOrigin.position = targetPosition;

            InspectionManager.Instance?.SetStreetInspectionState();

            Debug.Log($"LandingController: Landing completed for proposal {proposal.id}.");
            landingRoutine = null;
        }
    }
}
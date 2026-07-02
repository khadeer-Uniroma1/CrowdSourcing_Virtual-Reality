using UnityEngine;
using VRCrowdSourcing.BackendIntegration;

namespace VRCrowdSourcing.InspectionSystem
{
    public class InspectionManager : MonoBehaviour
    {
        [SerializeField] private LandingController landingController;
        public static InspectionManager Instance { get; private set; }

        public InspectionSession CurrentSession { get; private set; }

        public AppState CurrentState { get; private set; }
            = AppState.Overview;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void StartInspection(ProposalData proposal, ProposalMarker marker)
        {
            if (proposal == null)
            {
                Debug.LogError("InspectionManager: Cannot start inspection with a null proposal.");
                return;
            }

            CurrentSession?.End();
            CurrentSession = new InspectionSession(proposal);
            CurrentState = AppState.Landing;

            Debug.Log($"Inspection Started for Proposal {proposal.id} - {proposal.title}");

            if (landingController == null)
            {
                Debug.LogError("InspectionManager: LandingController is not assigned.");
                return;
            }

            landingController.LandAtMarker(proposal, marker);
        }

        public void SetStreetInspectionState()
        {
            if (CurrentSession == null || !CurrentSession.IsActive)
            {
                Debug.LogWarning("InspectionManager: Cannot enter StreetInspection without an active session.");
                return;
            }

            CurrentState = AppState.StreetInspection;

            Debug.Log($"InspectionManager: Street inspection active for proposal " + $"{CurrentSession.Proposal.id}.");
        }

        public void EndInspection()
        {
            CurrentSession?.End();

            CurrentSession = null;

            CurrentState = AppState.Overview;

            Debug.Log("Inspection Ended");
        }
    }
}
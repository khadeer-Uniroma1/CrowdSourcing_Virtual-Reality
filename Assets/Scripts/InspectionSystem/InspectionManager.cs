using UnityEngine;
using VRCrowdSourcing.BackendIntegration;

namespace VRCrowdSourcing.InspectionSystem
{
    public class InspectionManager : MonoBehaviour
    {
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

        public void StartInspection(ProposalData proposal)
        {
            CurrentSession = new InspectionSession(proposal);

            CurrentState = AppState.Landing;

            Debug.Log($"Inspection Started for Proposal {proposal.id} - {proposal.title}");
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
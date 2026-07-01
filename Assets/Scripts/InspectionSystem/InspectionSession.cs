using UnityEngine;
using VRCrowdSourcing.BackendIntegration;

namespace VRCrowdSourcing.InspectionSystem
{
    public class InspectionSession
    {
        public ProposalData Proposal { get; } // this proposal is coming from BackendIntegration namespace,thats why we used using VRCrowdSourcing.BackendIntegration; at the top

        public bool IsActive { get; private set; }

        public float WorkspaceRadius { get; } = 40f;

        public InspectionSession(ProposalData proposal)
        {
            Proposal = proposal;
            IsActive = true;
        }

        public void End()
        {
            IsActive = false;
        }
    }
}

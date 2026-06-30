using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRCrowdSourcing.BackendIntegration
{
    public class ProposalMarker : MonoBehaviour
    {
        private ProposalData proposalData;
        private XRSimpleInteractable interactable;

        public void Initialize(ProposalData data)
        {
            proposalData = data;

            if (!TryGetComponent(out interactable))
            {
                interactable = gameObject.AddComponent<XRSimpleInteractable>();
            }

            // selectExited fires when the ray RELEASES the object — this is when
            // we show the UI, so the ray is immediately free to point at the panel.
            // Using selectEntered kept the ray "locked" on the marker, blocking UI.
            interactable.selectExited.AddListener(OnSelectExited);

            // activated fires on trigger-press while held — also valid path
            interactable.activated.AddListener(OnActivated);

            gameObject.name = $"Proposal_{data.id}_{data.category}";

            Debug.Log($"ProposalMarker: Initialized '{gameObject.name}'");
        }

        private void OnDestroy()
        {
            if (interactable == null) return;
            interactable.selectExited.RemoveListener(OnSelectExited);
            interactable.activated.RemoveListener(OnActivated);
        }

        // ── XR callbacks ──────────────────────────────────────────────────────

        /// <summary>
        /// Fires when the ray trigger is RELEASED after selecting the marker.
        /// At this point the ray is free — perfect moment to open the UI panel
        /// because the same ray can immediately point at and click the panel buttons.
        /// </summary>
        private void OnSelectExited(SelectExitEventArgs args)
        {
            Debug.Log($"ProposalMarker: SelectExited — opening UI for '{proposalData?.title}'");
            ShowProposalOnUI();
        }

        /// <summary>
        /// Fires on trigger-press while the ray holds the marker (if Select Action
        /// Trigger = "Activate"). Kept as a fallback for different XRI configs.
        /// </summary>
        private void OnActivated(ActivateEventArgs args)
        {
            Debug.Log($"ProposalMarker: Activated — '{proposalData?.title}' '{proposalData?.id}'");
            ShowProposalOnUI();
        }

        // Public overload for editor / mouse fallback
        public void OnSelected()
        {
            Debug.Log($"ProposalMarker: OnSelected (editor) — '{proposalData?.title}'");
            ShowProposalOnUI();
        }

        // ── Editor fallback ───────────────────────────────────────────────────

        private void OnMouseDown()
        {
            Debug.Log("ProposalMarker: OnMouseDown fallback");
            ShowProposalOnUI();
        }

        // ── UI dispatch ───────────────────────────────────────────────────────

        private void ShowProposalOnUI()
        {
            if (proposalData == null) return;

            var controller = ProposalUIToolkitController.Instance
                ?? FindFirstObjectByType<ProposalUIToolkitController>(FindObjectsInactive.Include);
            // var controller = ProposalUIController.Instance
            //     ?? FindFirstObjectByType<ProposalUIController>(FindObjectsInactive.Include);

            if (controller != null)
            {
                controller.SetProposalData(proposalData);
                Debug.Log($"ProposalMarker: Sent '{proposalData.title}' to UI");
            }
            else
            {
                // Debug.LogError("ProposalMarker: ProposalUIToolkitController not found");
                Debug.LogError("ProposalMarker: ProposalUIController not found");
            }
        }
    }
}
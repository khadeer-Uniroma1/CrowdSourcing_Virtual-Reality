using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TeleportationActivator : MonoBehaviour
{

    public XRRayInteractor teleportationInteractor;
    public InputActionProperty teleportationActivateAction;

    void Start()
    {
        teleportationInteractor.gameObject.SetActive(false);
        teleportationActivateAction.action.performed += Action_Performed;
    }

    private void Action_Performed(InputAction.CallbackContext obj)
    {
        teleportationInteractor.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (teleportationActivateAction.action.WasReleasedThisFrame())
        {
            teleportationInteractor.gameObject.SetActive(false);
        }

    }
}

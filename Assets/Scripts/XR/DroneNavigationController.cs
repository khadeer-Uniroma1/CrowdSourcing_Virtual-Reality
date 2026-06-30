using UnityEngine;
using UnityEngine.InputSystem;

namespace VRCrowdSourcing.XR
{
    public class DroneNavigationController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 300f;
        public float verticalSpeed = 20f;
        public float rotationSpeed = 30f;

        [Header("References")]
        public Transform xrCamera;

        private DroneNavigationInputActions inputActions;

        private Vector2 moveInput;
        private Vector2 rotateInput;
        private float elevateInput;

        private void Awake()
        {
            inputActions = new DroneNavigationInputActions();
        }

        private void OnEnable()
        {
            inputActions.Enable();

            inputActions.Drone.Move.performed += ctx =>
                moveInput = ctx.ReadValue<Vector2>();

            inputActions.Drone.Move.canceled += ctx =>
                moveInput = Vector2.zero;

            inputActions.Drone.Rotate.performed += ctx =>
                rotateInput = ctx.ReadValue<Vector2>();

            inputActions.Drone.Rotate.canceled += ctx =>
                rotateInput = Vector2.zero;

            inputActions.Drone.Elevate.performed += ctx =>
                elevateInput = ctx.ReadValue<float>();

            inputActions.Drone.Elevate.canceled += ctx =>
                elevateInput = 0f;
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void Update()
        {
            HandleMovement();
            HandleRotation();
            HandleElevation();
        }

        private void HandleMovement()
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            forward.y = 0;
            right.y = 0;

            Vector3 direction =
                forward.normalized * moveInput.y +
                right.normalized * moveInput.x;

            transform.position +=
                direction * moveSpeed * Time.deltaTime;
        }

        private void HandleRotation()
        {
            transform.Rotate(
                Vector3.up,
                rotateInput.x * rotationSpeed * Time.deltaTime
            );
        }

        private void HandleElevation()
        {
            transform.position +=
                Vector3.up * elevateInput * verticalSpeed * Time.deltaTime;
        }
    }
}
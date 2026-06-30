using UnityEngine;

namespace VRCrowdSourcing.BackendIntegration
{
    public class UIFollowPlayer : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private float distance = 2f;
        [SerializeField] private float recenterDistance = 4f;
        [SerializeField] private float heightOffset = -0.15f;
        [SerializeField] private float positionSmoothSpeed = 5f;
        [SerializeField] private float rotationSmoothSpeed = 8f;

        private Transform targetHead;
        private bool isFollowing;

        public void StartFollowing(Transform head)
        {
            targetHead = head;
            isFollowing = true;

            SnapToHead();
        }

        public void StopFollowing()
        {
            isFollowing = false;
        }

        private void LateUpdate()
        {
            if (!isFollowing || targetHead == null)
                return;

            float distanceToUser = Vector3.Distance(transform.position, targetHead.position);

            if (distanceToUser > recenterDistance)
            {
                SnapToHead();
                return;
            }

            UpdatePosition();
        }

        private void SnapToHead()
        {
            Vector3 forward = targetHead.forward;
            forward.y = 0f;
            forward.Normalize();

            transform.position =
                targetHead.position +
                forward * distance +
                Vector3.up * heightOffset;

            FacePlayer();
        }

        private void UpdatePosition()
        {
            Vector3 forward = targetHead.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 desiredPosition =
                targetHead.position +
                forward * distance +
                Vector3.up * heightOffset;

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                Time.deltaTime * positionSmoothSpeed);

            FacePlayer();
        }

        private void FacePlayer()
        {
            Vector3 lookTarget = targetHead.position;
            lookTarget.y = transform.position.y;

            Quaternion targetRotation =  Quaternion.LookRotation(transform.position - lookTarget);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSmoothSpeed);
        }
    }
}
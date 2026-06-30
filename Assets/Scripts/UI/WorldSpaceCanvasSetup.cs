using System.Collections;
using UnityEngine;

namespace VRCrowdSourcing.UI
{
    [RequireComponent(typeof(Canvas))]
    public class WorldSpaceCanvasSetup : MonoBehaviour
    {
        private Canvas canvas;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            StartCoroutine(AssignWorldCameraWhenReady());
        }

        private IEnumerator AssignWorldCameraWhenReady()
        {
            while (XRReferences.Instance == null || XRReferences.Instance.mainCamera == null)
            {
                yield return null;
            }

            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                canvas.worldCamera = XRReferences.Instance.mainCamera;
            }
        }
    }
}
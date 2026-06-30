using UnityEngine;

public class XRReferences : MonoBehaviour
{
    public static XRReferences Instance;

    [Header("XR References")]
    public Camera mainCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
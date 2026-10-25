
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    public Transform cameraTransform;

    [Range(0f, 1f)]
    public float parallaxFactor = 0.08f;

    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            lastCameraPosition = cameraTransform.position;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
        {
            return;
        }

        Vector3 cameraDelta =
            cameraTransform.position - lastCameraPosition;

        transform.position += cameraDelta * parallaxFactor;

        lastCameraPosition = cameraTransform.position;
    }
}

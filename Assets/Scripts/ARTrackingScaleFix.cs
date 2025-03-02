using UnityEngine;
using UnityEngine.InputSystem.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;

public class ARTrackingScaleFix : MonoBehaviour
{
    public float inverseScaleFactor = 0.1f; // Inverse of the XR Origin scale (1/10)

    private XROrigin xrOrigin;
    private TrackedPoseDriver trackedPoseDriver;
    private ARCameraManager arCameraManager;

    void Start()
    {
        xrOrigin = GetComponent<XROrigin>();
        trackedPoseDriver = GetComponentInChildren<TrackedPoseDriver>();
        arCameraManager = GetComponentInChildren<ARCameraManager>();

        if (xrOrigin != null)
        {
            // Apply inverse scaling to plane detection and tracking calculations
            xrOrigin.transform.localScale = new Vector3(inverseScaleFactor, inverseScaleFactor, inverseScaleFactor);
        }
    }

    void LateUpdate()
    {
        if (trackedPoseDriver != null)
        {
            // Counteract the XR Origin scale for tracking data
            transform.localPosition *= inverseScaleFactor;
        }
    }
}

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class ARPlaneColliderManager : MonoBehaviour
{
    private ARPlaneManager arPlaneManager;
    private HashSet<ARPlane> processedPlanes = new HashSet<ARPlane>();

    void Start()
    {
        arPlaneManager = Object.FindFirstObjectByType<ARPlaneManager>(); // Updated for Unity 6
    }

    void Update()
    {
        if (arPlaneManager == null) return;

        foreach (var plane in arPlaneManager.trackables)
        {
            if (!processedPlanes.Contains(plane))
            {
                AddCollider(plane);
                processedPlanes.Add(plane);
            }
            else
            {
                UpdateCollider(plane);
            }
        }
    }

    private void AddCollider(ARPlane plane)
    {
        if (plane.gameObject.GetComponent<MeshCollider>() == null)
        {
            MeshCollider collider = plane.gameObject.AddComponent<MeshCollider>();
            collider.convex = false; // Keep it non-convex for accurate grounding
            collider.isTrigger = false;
        }
    }

    private void UpdateCollider(ARPlane plane)
    {
        MeshCollider collider = plane.gameObject.GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.sharedMesh = plane.GetComponent<MeshFilter>().mesh;
        }
    }
}

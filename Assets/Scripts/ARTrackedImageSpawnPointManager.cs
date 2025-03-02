#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Opsive.UltimateCharacterController.Game;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARTrackedImageSpawnPointController : MonoBehaviour
{
    public static event Action OnSpawnPointRegistered;

    private ARTrackedImageManager _arTrackedImageManager;
    private readonly Dictionary<Guid, SpawnPoint> _registeredSpawnPoints = new Dictionary<Guid, SpawnPoint>();

    [Header("Editor Simulation Settings")]
    [SerializeField] private GameObject _fiducialSimulationPrefab;

    private void Awake()
    {
        _arTrackedImageManager = GetComponent<ARTrackedImageManager>();

#if UNITY_EDITOR
        SimulateFiducialTracking();
#endif
    }

    private void Update()
    {
        foreach (var trackedImage in _arTrackedImageManager.trackables)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                RegisterSpawnPoint(trackedImage);
            }
            else if (trackedImage.trackingState == TrackingState.None || trackedImage.trackingState == TrackingState.Limited)
            {
                UnregisterSpawnPoint(trackedImage);
            }
        }
    }

#if UNITY_EDITOR
    private void SimulateFiducialTracking()
    {
        if (_fiducialSimulationPrefab != null)
        {
            GameObject simulatedFiducial = Instantiate(_fiducialSimulationPrefab, Vector3.zero, Quaternion.identity);
            SpawnPoint spawnPoint = simulatedFiducial.GetComponentInChildren<SpawnPoint>();

            if (spawnPoint != null)
            {
                SpawnPointManager.AddSpawnPoint(spawnPoint);
                OnSpawnPointRegistered?.Invoke();

                Debug.Log("Simulated spawn point registered in Editor.");
            }
            else
            {
                Debug.LogWarning("No SpawnPoint component found on simulated fiducial.");
            }
        }
    }
#endif

    private void RegisterSpawnPoint(ARTrackedImage image)
    {
        if (image == null || image.referenceImage.guid == Guid.Empty) return;

        if (!_registeredSpawnPoints.ContainsKey(image.referenceImage.guid))
        {
            SpawnPoint spawnPoint = image.GetComponentInChildren<SpawnPoint>();

            if (spawnPoint != null)
            {
                SpawnPointManager.AddSpawnPoint(spawnPoint);
                _registeredSpawnPoints[image.referenceImage.guid] = spawnPoint;
                OnSpawnPointRegistered?.Invoke();

                Debug.Log($"Spawn point registered at AR tracked image: {image.referenceImage.name}");
            }
            else
            {
                Debug.LogWarning($"No SpawnPoint component found on prefab associated with image: {image.referenceImage.name}");
            }
        }
    }

    private void UnregisterSpawnPoint(ARTrackedImage image)
    {
        if (image == null || image.referenceImage.guid == Guid.Empty) return;

        if (_registeredSpawnPoints.TryGetValue(image.referenceImage.guid, out var spawnPoint))
        {
            SpawnPointManager.RemoveSpawnPoint(spawnPoint);
            _registeredSpawnPoints.Remove(image.referenceImage.guid);

            Debug.Log($"Spawn point unregistered at AR tracked image: {image.referenceImage.name}");
        }
    }
}

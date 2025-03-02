using UnityEngine;
using Opsive.UltimateCharacterController.Game;
using System.Collections;

public class GameStartSpawner : MonoBehaviour
{
    [Tooltip("The player prefab to spawn.")]
    public GameObject playerPrefab;

    private bool playerSpawned = false;

    private void OnEnable()
    {
        ARTrackedImageSpawnPointController.OnSpawnPointRegistered += OnSpawnPointRegistered;
    }

    private void OnDisable()
    {
        ARTrackedImageSpawnPointController.OnSpawnPointRegistered -= OnSpawnPointRegistered;
    }

    private void OnSpawnPointRegistered()
    {
        if (!playerSpawned)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (SpawnPointManager.GetPlacement(playerPrefab, -1, ref spawnPosition, ref spawnRotation))
        {
            Instantiate(playerPrefab, spawnPosition, spawnRotation);
            playerSpawned = true;
            Debug.Log("Player spawned successfully.");
        }
        else
        {
            Debug.LogError("Error: No valid spawn point found!");
        }
    }
}

using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.AI.Navigation;

public class DynamicNavMeshBaker : MonoBehaviour
{
    private NavMeshSurface navMeshSurface;

    private void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        StartCoroutine(UpdateNavMesh());
    }

    IEnumerator UpdateNavMesh()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Adjust based on performance needs
            navMeshSurface.BuildNavMesh(); // Rebake the NavMesh dynamically
        }
    }
}

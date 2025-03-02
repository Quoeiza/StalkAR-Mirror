using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class PlayerControlsManager : MonoBehaviour
{
    private PlayerInput m_PlayerInput;
    private NavMeshAgent m_NavMeshAgent;
    private ARRaycastManager m_ARRaycastManager;

    [Tooltip("The radius of the destination bubble. The character will stop when it enters this distance from the target position.")]
    [SerializeField] private float destinationBubbleRadius = 0.5f;

    private Vector3 m_TargetPosition; // Track the current target position
    private bool m_IsMoving; // Track if the character is currently moving to a target

    private void Awake()
    {
        // Get components
        m_PlayerInput = GetComponent<PlayerInput>();
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_ARRaycastManager = FindFirstObjectByType<ARRaycastManager>();

        // Validate components
        if (m_PlayerInput == null)
        {
            Debug.LogError("PlayerInput component is missing. Ensure it's added to the player.", gameObject);
        }
        if (m_NavMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent component is missing. Ensure it's added to the player.", gameObject);
        }
        if (m_ARRaycastManager == null)
        {
            Debug.LogWarning("ARRaycastManager not found in the scene.", gameObject);
        }

        // Initialize state
        m_IsMoving = false;
        m_TargetPosition = transform.position;
    }

    private void OnEnable()
    {
        if (m_PlayerInput != null && m_PlayerInput.actions != null)
        {
            var moveAction = m_PlayerInput.actions.FindAction("Move");
            if (moveAction != null)
            {
                moveAction.performed += MoveToTarget;
                Debug.Log("Move action subscribed.", gameObject);
            }
            else
            {
                Debug.LogError("Move action not found in PlayerInput actions.", gameObject);
            }
        }
    }

    private void OnDisable()
    {
        if (m_PlayerInput != null && m_PlayerInput.actions != null)
        {
            var moveAction = m_PlayerInput.actions.FindAction("Move");
            if (moveAction != null)
            {
                moveAction.performed -= MoveToTarget;
                Debug.Log("Move action unsubscribed.", gameObject);
            }
        }

        // Stop the NavMeshAgent if it's active and on a NavMesh
        if (m_NavMeshAgent != null && m_NavMeshAgent.isOnNavMesh && m_NavMeshAgent.enabled)
        {
            m_NavMeshAgent.isStopped = true;
            m_NavMeshAgent.ResetPath();
        }

        // Reset movement state
        m_IsMoving = false;
    }

    private void MoveToTarget(InputAction.CallbackContext context)
    {
        if (m_NavMeshAgent == null || m_ARRaycastManager == null) return;

        // Get the current mouse or touch position
        Vector2 screenPosition;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.tapCount.ReadValue() > 0)
        {
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else
        {
            Debug.LogWarning("No valid input source detected for move action.", gameObject);
            return;
        }

        Debug.Log($"Move action triggered at screen position: {screenPosition}", gameObject);

        // Cast a ray using ARRaycastManager for AR planes
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (m_ARRaycastManager.Raycast(screenPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            Vector3 hitPosition = hits[0].pose.position;
            Debug.Log($"AR raycast hit at: {hitPosition}", gameObject);

            // Ensure the position is on the NavMesh
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(hitPosition, out navMeshHit, 1.0f, NavMesh.AllAreas))
            {
                hitPosition = navMeshHit.position;
                Debug.Log($"Adjusted hit position to NavMesh: {hitPosition}", gameObject);

                // Set the new target position and indicate the character is moving
                m_TargetPosition = hitPosition;
                m_IsMoving = true;

                // Move the character using NavMeshAgent
                if (m_NavMeshAgent.isOnNavMesh && m_NavMeshAgent.enabled)
                {
                    m_NavMeshAgent.isStopped = false;
                    m_NavMeshAgent.SetDestination(hitPosition);
                }
                else
                {
                    Debug.LogWarning("NavMeshAgent is not on a NavMesh or is disabled. Cannot move.", gameObject);
                    m_IsMoving = false;
                }
            }
            else
            {
                Debug.LogWarning("Clicked position is not on the NavMesh.", gameObject);
            }
        }
        else
        {
            Debug.Log("AR raycast did not hit any planes.", gameObject);
        }
    }

    private void Update()
    {
        // Check if the character is moving and has reached the destination bubble
        if (m_IsMoving && m_NavMeshAgent != null && m_NavMeshAgent.isOnNavMesh && m_NavMeshAgent.enabled)
        {
            // Calculate the distance to the target position based on the character's position
            float distanceToTarget = Vector3.Distance(transform.position, m_TargetPosition);

            // Check if the character has entered the destination bubble
            if (distanceToTarget <= destinationBubbleRadius)
            {
                Debug.Log("Character has entered the destination bubble. Stopping NavMeshAgent.", gameObject);
                m_NavMeshAgent.isStopped = true;
                m_NavMeshAgent.ResetPath();
                m_IsMoving = false; // Allow new move requests
            }
        }
    }
}
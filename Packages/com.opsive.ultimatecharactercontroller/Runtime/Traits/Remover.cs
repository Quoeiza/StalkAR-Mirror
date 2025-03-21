﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits
{
    using Opsive.Shared.Game;
    using UnityEngine;

    /// <summary>
    /// Places the object back in the ObjectPool after the specified number of seconds.
    /// </summary>
    public class Remover : MonoBehaviour
    {
        [Tooltip("The number of seconds until the object should be placed back in the pool.")]
        [SerializeField] protected float m_Lifetime = 5;

        [System.NonSerialized] private GameObject m_GameObject;
        private ScheduledEventBase m_RemoveEvent;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
        }

        /// <summary>
        /// Schedule the object for removal.
        /// </summary>
        private void OnEnable()
        {
            m_RemoveEvent = Scheduler.Schedule(m_Lifetime, Remove);
        }

        /// <summary>
        /// Cancels the remove event.
        /// </summary>
        public void CancelRemoveEvent()
        {
            if (m_RemoveEvent != null) {
                Scheduler.Cancel(m_RemoveEvent);
                m_RemoveEvent = null;
            }
        }

        /// <summary>
        /// The object has been destroyed - no need for removal if it hasn't already been removed.
        /// </summary>
        private void OnDisable()
        {
            CancelRemoveEvent();
        }

        /// <summary>
        /// Remove the object.
        /// </summary>
        private void Remove()
        {
            ObjectPoolBase.Destroy(m_GameObject);
            m_RemoveEvent = null;
        }
    }
}
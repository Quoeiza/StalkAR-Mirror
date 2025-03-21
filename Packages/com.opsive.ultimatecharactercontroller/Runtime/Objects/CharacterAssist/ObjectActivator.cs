﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects
{
    using Opsive.Shared.StateSystem;
    using UnityEngine;

    /// <summary>
    /// Activates or deactivates the GameObject based on the state.
    /// </summary>
    public class ObjectActivator : StateBehavior
    {
        [Tooltip("Should the GameObject be activated?")]
        [SerializeField] protected bool m_Active = true;

        public bool Active { get { return m_Active; } set { m_Active = value; } }

        [System.NonSerialized] private GameObject m_GameObject;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            m_GameObject = gameObject;

            base.Awake();
        }

        /// <summary>
        /// The StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            base.StateChange();

            m_GameObject.SetActive(m_Active);
        }
    }
}
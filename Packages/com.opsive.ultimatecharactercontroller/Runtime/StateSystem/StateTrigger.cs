﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.StateSystem
{
    using Opsive.Shared.Audio;
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Activates the specified state when the object enters the state, and deactivates the sate when the object leaves.
    /// </summary>
    public class StateTrigger : MonoBehaviour
    {
        [Tooltip("The name of the state to activate/deactivate.")]
        [SerializeField] [StateName] protected string m_StateName;
        [Tooltip("The delay before the state should be enabled.")]
        [SerializeField] protected float m_Delay;
        [Tooltip("The amount of time the state should be enabled for.")]
        [SerializeField] protected float m_Duration;
        [Tooltip("The LayerMask that the trigger can set the state of.")]
        [SerializeField] protected LayerMask m_LayerMask = 1 << LayerManager.Character;
        [Tooltip("Should the state change only be applied to the character?")]
        [SerializeField] protected bool m_RequireCharacter = true;
        [Tooltip("Does the state require the character changing transforms?")]
        [SerializeField] protected bool m_CharacterTransformChange;
        [Tooltip("A set of AudioClips that can be played when the state is activated.")]
        [SerializeField] protected AudioClipSet m_ActivateAudioClipSet = new AudioClipSet();

        public AudioClipSet ActivateAudioClipSet { get { return m_ActivateAudioClipSet; } set { m_ActivateAudioClipSet = value; } }

        private ScheduledEventBase m_ActivateStateEvent;
        private List<GameObject> m_DeathDeactivations;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            if (string.IsNullOrEmpty(m_StateName)) {
                enabled = false;
            }
        }

        /// <summary>
        /// The other collider has entered the trigger.
        /// </summary>
        /// <param name="other">The collider which entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask)) {
                return;
            }

            StateBehavior stateBehavior;
            if ((m_RequireCharacter && (stateBehavior = other.GetComponentInParent<Character.UltimateCharacterLocomotion>()) != null) ||
                (!m_RequireCharacter && (stateBehavior = other.GetComponentInParent<StateBehavior>()) != null)) {
                m_ActivateStateEvent = Scheduler.Schedule(m_Delay, ChangeState, stateBehavior.gameObject, true);

                m_ActivateAudioClipSet.PlayAudioClip(null);
            }
        }

        /// <summary>
        /// Activates or deactivates the state on the specified GameObject.
        /// </summary>
        /// <param name="stateGameObject">The GameObject to activate the state on.</param>
        /// <param name="activate">Should the state be activated?</param>
        private void ChangeState(GameObject stateGameObject, bool activate)
        {
            StateManager.SetState(stateGameObject, m_StateName, activate);
            if (m_CharacterTransformChange) {
                EventHandler.ExecuteEvent(stateGameObject, "OnCharacterImmediateTransformChange", true);
            }
            m_ActivateStateEvent = null;
            int index;
            if (m_DeathDeactivations != null && (index = m_DeathDeactivations.IndexOf(stateGameObject)) > 0) {
                m_DeathDeactivations.RemoveAt(index);
                EventHandler.UnregisterEvent(stateGameObject, "OnRespawn", OnRespawn);
            }

            // The state can be disabled automatically.
            if (activate && m_Duration > 0) {
                Scheduler.Schedule(m_Duration, ChangeState, stateGameObject, false);
            }
        }

        /// <summary>
        /// The other collider has exited the trigger.
        /// </summary>
        /// <param name="other">The collider which exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (m_Duration > 0 || !MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask)) {
                return;
            }

            TriggerExit(other.gameObject);
        }

        /// <summary>
        /// The other GameObject has exited the trigger.
        /// </summary>
        /// <param name="other">The GameObject which exited the trigger.</param>
        public void TriggerExit(GameObject other)
        {
            StateBehavior stateBehavior;
            if ((m_RequireCharacter && (stateBehavior = other.GetComponentInParent<Character.UltimateCharacterLocomotion>()) != null) ||
                (!m_RequireCharacter && (stateBehavior = other.GetComponentInParent<StateBehavior>()) != null)) {
                if (m_ActivateStateEvent != null && m_ActivateStateEvent.Active) {
                    Scheduler.Cancel(m_ActivateStateEvent);
                    m_ActivateStateEvent = null;
                } else {
                    // The state shouldn't change when the object dies. It can be changed when the character respawns.
                    var health = other.gameObject.GetCachedParentComponent<Traits.Health>();
                    if (health != null && !health.IsAlive()) {
                        // When the character respawns the trigger enter/exit event may not fire. Register that the state should be deactivated so when the 
                        // character respawns the state can then be disabled.
                        if (m_DeathDeactivations == null) {
                            m_DeathDeactivations = new List<GameObject>();
                        }
                        if (!m_DeathDeactivations.Contains(stateBehavior.gameObject)) {
                            m_DeathDeactivations.Add(stateBehavior.gameObject);
                            EventHandler.RegisterEvent(stateBehavior.gameObject, "OnRespawn", OnRespawn);
                        }
                        return;
                    }

                    StateManager.SetState(stateBehavior.gameObject, m_StateName, false);
                    if (m_CharacterTransformChange) {
                        EventHandler.ExecuteEvent(stateBehavior.gameObject, "OnCharacterImmediateTransformChange", true);
                    }
                }
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            for (int i = m_DeathDeactivations.Count - 1; i > -1; --i) {
                StateManager.SetState(m_DeathDeactivations[i], m_StateName, false);
                EventHandler.UnregisterEvent(m_DeathDeactivations[i], "OnRespawn", OnRespawn);
            }
            m_DeathDeactivations.Clear();
        }
    }
}
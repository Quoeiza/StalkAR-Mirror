﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using UnityEngine;

    /// <summary>
    /// The RestrictPosition ability restricts the character to the specified position.
    /// </summary>
    [DefaultStartType(AbilityStartType.Automatic)]
    [DefaultStopType(AbilityStopType.Manual)]
    public class RestrictPosition : Ability
    {
        /// <summary>
        /// Specifies how to restrict the character's position.
        /// </summary>
        public enum RestrictionType
        {
            RestrictX,  // Restricts the local X position.
            RestrictZ,  // Restricts the local Z position.
            RestrictXZ  // Restricts the local X and Z position.
        }

        [Tooltip("Specifies how to restrict the character's position.")]
        [HideInInspector] [SerializeField] protected RestrictionType m_Restriction = RestrictionType.RestrictXZ;
        [Tooltip("If restricting the X axis, specifies the minimum local X position the character can move to.")]
        [HideInInspector] [SerializeField] protected float m_MinXPosition;
        [Tooltip("If restricting the X axis, specifies the maximum local X position the character can move to.")]
        [HideInInspector] [SerializeField] protected float m_MaxXPosition;
        [Tooltip("If restricting the Z axis, specifies the minimum local Z position the character can move to.")]
        [HideInInspector] [SerializeField] protected float m_MinZPosition;
        [Tooltip("If restricting the Z axis, specifies the maximum local Z position the character can move to.")]
        [HideInInspector] [SerializeField] protected float m_MaxZPosition;
         
        public RestrictionType Restiction { get => m_Restriction; set => m_Restriction = value; }
        public float MinXPosition { get => m_MinXPosition; set => m_MinXPosition = value; }
        public float MaxXPosition { get => m_MaxXPosition; set => m_MaxXPosition = value; }
        public float MinZPosition { get => m_MinZPosition; set => m_MinZPosition = value; }
        public float MaxZPosition { get => m_MaxZPosition; set => m_MaxZPosition = value; }

        public override bool IsConcurrent { get => true; }

        /// <summary>
        /// Restrict the move direction if the character would be outside the valid position.
        /// </summary>
        public override void ApplyPosition()
        {
            var targetPosition = m_CharacterLocomotion.TargetPosition;
            if (RestrictedPosition(ref targetPosition)) {
                m_CharacterLocomotion.DesiredMovement = targetPosition - m_CharacterLocomotion.Position;
            }
        }

        /// <summary>
        /// Updates the target position to the restricted position. Will return true if the position is restricted.
        /// </summary>
        /// <param name="targetPosition">The target position that should be restricted.</param>
        /// <returns>True if the position is restricted.</returns>
        public bool RestrictedPosition(ref Vector3 targetPosition)
        {
            var restricted = false;
            // Restrict the x axis if the constraint is set to anything but RestrictZ.
            if (m_Restriction != RestrictionType.RestrictZ) {
                if (targetPosition.x < m_MinXPosition) {
                    targetPosition.x = m_MinXPosition;
                    restricted = true;
                } else if (targetPosition.x > m_MaxXPosition) {
                    targetPosition.x = m_MaxXPosition;
                    restricted = true;
                }
            }

            // Restrict the z axis if the constraint is set to anything but RestrictX.
            if (m_Restriction != RestrictionType.RestrictX) {
                if (targetPosition.z < m_MinZPosition) {
                    targetPosition.z = m_MinZPosition;
                    restricted = true;
                } else if (targetPosition.z > m_MaxZPosition) {
                    targetPosition.z = m_MaxZPosition;
                    restricted = true;
                }
            }
            return restricted;
        }
    }
}
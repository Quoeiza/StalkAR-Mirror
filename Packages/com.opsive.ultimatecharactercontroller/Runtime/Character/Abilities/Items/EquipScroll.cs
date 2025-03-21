﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The EquipScroll ability will scroll between the previous and next abilities with the scroll wheel.
    /// </summary>
    [DefaultStartType(AbilityStartType.Axis)]
    [DefaultInputName("Mouse ScrollWheel")]
    [AllowDuplicateTypes]
    public class EquipScroll : ItemSetAbilityBase
    {
        [Tooltip("The sensitivity for switching between items. The higher the value the faster the scroll wheel has to scroll in order to switch items.")]
        [SerializeField] protected float m_ScrollSensitivity = 0.1f;

        public float ScrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }

        private int m_ScrollItemSetIndex = -1;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // The EquipUnequip must exist in order for the item to be able to be equip toggled.
            if (m_EquipUnequipItemAbility == null) {
                Debug.LogError($"Error: The EquipUnequip ItemAbility with the category ID {m_ItemSetCategoryID} must be added to the character.");
                Enabled = false;
            }
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            // Don't scroll items if the scroll wheel value isn't large enough.
            if (Mathf.Abs(InputAxisValue) < m_ScrollSensitivity) {
                return false;
            }

            m_ScrollItemSetIndex = m_ItemSetManager.NextActiveItemSetIndex(m_ItemSetGroupIndex, m_EquipUnequipItemAbility.ActiveItemSetIndex, InputAxisValue > 0);

            return m_ScrollItemSetIndex != -1 && m_ScrollItemSetIndex != m_EquipUnequipItemAbility.ActiveItemSetIndex;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            // EquipUnequip should temporarily inherit the current prevention value.
            var preventStartUseReload = m_EquipUnequipItemAbility.PreventStartUseReloadActive;
            m_EquipUnequipItemAbility.PreventStartUseReloadActive = m_PreventStartUseReloadActive;
            m_EquipUnequipItemAbility.StartEquipUnequip(m_ScrollItemSetIndex);
            m_EquipUnequipItemAbility.PreventStartUseReloadActive = preventStartUseReload;

            // It is up to the EquipUnequip ability to do the actual equip - stop the current ability.
            StopAbility();
        }
    }
}
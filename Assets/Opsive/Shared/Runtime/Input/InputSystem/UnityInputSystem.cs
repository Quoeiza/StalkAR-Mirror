/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Integrations.InputSystem
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using System.Collections.Generic;

    /// <summary>
    /// Responds to input using the Unity Input System.
    /// </summary>
    public class UnityInputSystem : Opsive.Shared.Input.PlayerInput
    {
        [Tooltip("Should the cursor be disabled?")]
        [SerializeField] protected bool m_DisableCursor = true;
        [Tooltip("Should the cursor be enabled when the escape key is pressed?")]
        [SerializeField] protected bool m_EnableCursorWithEscape = true;
#if UNITY_EDITOR || !(UNITY_IPHONE || UNITY_ANDROID || UNITY_WP_8_1 || UNITY_BLACKBERRY)
        [Tooltip("If the cursor is enabled with escape should the look vector be prevented from updating?")]
        [SerializeField] protected bool m_PreventLookVectorChanges = true;
#endif
        private PlayerInput m_PlayerInput;
        private Dictionary<InputActionMap, Dictionary<string, InputAction>> m_InputActionByName = new Dictionary<InputActionMap, Dictionary<string, InputAction>>();

        protected override bool CanCheckForController => false;

        public bool DisableCursor {
            get { return m_DisableCursor; }
            set {

                m_DisableCursor = value;
                if (m_DisableCursor && Cursor.visible) {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                } else if (!m_DisableCursor && !Cursor.visible) {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
        public bool EnableCursorWithEscape { get { return m_EnableCursorWithEscape; } set { m_EnableCursorWithEscape = value; } }
#if UNITY_EDITOR || !(UNITY_IPHONE || UNITY_ANDROID || UNITY_WP_8_1 || UNITY_BLACKBERRY)
        public bool PreventLookMovementWithEscape { get { return m_PreventLookVectorChanges; } set { m_PreventLookVectorChanges = value; } }
#endif

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_PlayerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            m_PlayerInput.enabled = true;
        }

        /// <summary>
        /// The component has been enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (m_DisableCursor) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

#if UNITY_EDITOR || !(UNITY_IPHONE || UNITY_ANDROID || UNITY_WP_8_1 || UNITY_BLACKBERRY)
        /// <summary>
        /// Update the joystick and cursor state values.
        /// </summary>
        private void LateUpdate()
        {
            // Enable the cursor if the escape key is pressed. Disable the cursor if it is visbile but should be disabled upon press.
            if (m_EnableCursorWithEscape && Keyboard.current.escapeKey.wasPressedThisFrame) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (m_PreventLookVectorChanges) {
                    OnApplicationFocus(false);
                }
            } else if ( (Cursor.visible || m_Focus == false) && m_DisableCursor && !IsPointerOverUI() && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (m_PreventLookVectorChanges) {
                    OnApplicationFocus(true);
                }
            }
#if UNITY_EDITOR
            // The cursor should be visible when the game is paused.
            if (!Cursor.visible && Time.deltaTime == 0) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
#endif
        }
#endif

        /// <summary>
        /// Is a controller connected?
        /// </summary>
        /// <returns>True if a controller is connected.</returns>
        public override bool IsControllerConnected() { return true; }

        /// <summary>
        /// Internal method which returns true if the button is being pressed.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True of the button is being pressed.</returns>
        protected override bool GetButtonInternal(string name)
        {
            var action = GetActionByName(name);
            if (action != null) {
                if (action.activeControl is ButtonControl button && button.isPressed) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Internal method which returns true if the button was pressed this frame.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is pressed this frame.</returns>
        protected override bool GetButtonDownInternal(string name)
        {
            var action = GetActionByName(name);
            if (action != null) {
                if (action.activeControl is ButtonControl button && button.wasPressedThisFrame) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Internal method which returnstrue if the button is up.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is up.</returns>
        protected override bool GetButtonUpInternal(string name)
        {
            var action = GetActionByName(name);
            if (action != null) {
                for (int i = 0; i < action.controls.Count; i++) {
                    if (action.controls[i] is ButtonControl button && button.wasReleasedThisFrame) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Internal method which returns the value of the axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        protected override float GetAxisInternal(string name)
        {
            var action = GetActionByName(name);
            if (action != null) {
                return action.ReadValue<float>();
            }
            return 0.0f;
        }

        /// <summary>
        /// Internal method which returns the value of the raw axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the raw axis.</returns>
        protected override float GetAxisRawInternal(string name)
        {
            return GetAxisInternal(name);
        }

        /// <summary>
        /// Returns the position of the mouse.
        /// </summary>
        /// <returns>The mouse position.</returns>
        public override Vector2 GetMousePosition() { return Mouse.current.position.ReadValue(); }

        /// <summary>
        /// Enables or disables gameplay input. An example of when it will not be enabled is when there is a fullscreen UI over the main camera.
        /// </summary>
        /// <param name="enable">True if the input is enabled.</param>
        protected override void EnableGameplayInput(bool enable)
        {
            base.EnableGameplayInput(enable);

            if (enable && m_DisableCursor) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        /// <summary>
        /// Does the game have focus?
        /// </summary>
        /// <param name="hasFocus">True if the game has focus.</param>
        protected override void OnApplicationFocus(bool hasFocus)
        {
            base.OnApplicationFocus(hasFocus);

            if (enabled && hasFocus && m_DisableCursor) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        /// <summary>
        /// Returns the InputAction with the specified name.
        /// </summary>
        /// <param name="name">The name of the InputAction.</param>
        /// <returns>The InputAction with the specified name.</returns>
        private InputAction GetActionByName(string name)
        {
            if (m_PlayerInput.currentActionMap == null) {
                return null;
            }

            if (!m_InputActionByName.TryGetValue(m_PlayerInput.currentActionMap, out var inputActionByName)) {
                inputActionByName = new Dictionary<string, InputAction>();
                m_InputActionByName.Add(m_PlayerInput.currentActionMap, inputActionByName);
            }
            if (!inputActionByName.TryGetValue(name, out var inputAction)) {
                inputAction = m_PlayerInput.currentActionMap?.FindAction(name);
                inputActionByName.Add(name, inputAction);
            }
            return inputAction;
        }

        /// <summary>
        /// Resets the component.
        /// </summary>
        public void Reset()
        {
            var unityPlayerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (unityPlayerInput == null) {
                gameObject.AddComponent<UnityEngine.InputSystem.PlayerInput>();
            }
            var unityInput = GetComponent<Shared.Input.UnityInput>();
            if (unityInput != null) {
                DestroyImmediate(unityInput, true);
            }
        }
    }
}

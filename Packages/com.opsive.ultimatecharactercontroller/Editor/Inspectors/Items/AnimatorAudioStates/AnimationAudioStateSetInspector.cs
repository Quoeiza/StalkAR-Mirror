﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.AnimatorAudioState
{
    using Opsive.Shared.Editor.Inspectors.Utility;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
    using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
    using Opsive.UltimateCharacterController.StateSystem;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Draws a user friendly inspector for the AnimatorAudioStateSet class.
    /// </summary>
    public static class AnimatorAudioStateSetInspector
    {
        private static List<Type> s_SelectorTypeCache;
        private static List<string> s_SelectorTypeNameCache;

        /// <summary>
        /// Draws the AnimatorAudioStateSet.
        /// </summary>
        public static void DrawAnimatorAudioStateSet(UnityEngine.Object target, AnimatorAudioStateSet animatorAudioStateSet, string animatorAudioStateSetFieldName, bool randomDefaultSelector, 
                                                ref ReorderableList reorderableList, ReorderableList.ElementCallbackDelegate drawCallback, ReorderableList.SelectCallbackDelegate selectCallback,
                                                ReorderableList.AddCallbackDelegate addCallback, ReorderableList.RemoveCallbackDelegate removeCallback, string preferencesKey,
                                                ref ReorderableList reorderableAudioList, ReorderableList.ElementCallbackDelegate drawAudioElementCallback, 
                                                ReorderableList.AddCallbackDelegate addAudioCallback, ReorderableList.RemoveCallbackDelegate removeAudioCallback, 
                                                ref ReorderableList reorderableStateList, ReorderableList.ElementCallbackDelegate stateDrawElementCallback, 
                                                ReorderableList.AddCallbackDelegate stateAddCallback, ReorderableList.ReorderCallbackDelegate stateReorderCallback,
                                                ReorderableList.RemoveCallbackDelegate stateRemoveCallback, string statePreferencesKey)
        {
            PopulateAnimatorAudioStateSelectorTypes();
            if (animatorAudioStateSet.States == null || animatorAudioStateSet.States.Length == 0) {
                animatorAudioStateSet.States = new AnimatorAudioStateSet.AnimatorAudioState[] { new AnimatorAudioStateSet.AnimatorAudioState() };
            }

            var serializedObject = new SerializedObject(target);
            var serializedProperty = serializedObject.FindProperty(animatorAudioStateSetFieldName).FindPropertyRelative("m_States");
            if (reorderableList == null) {
                reorderableList = new ReorderableList(animatorAudioStateSet.States, typeof(AnimatorAudioStateSet.AnimatorAudioState), false, true, true, animatorAudioStateSet.States.Length > 1);
                reorderableList.drawHeaderCallback = OnAnimatorAudioStateListHeaderDraw;
                reorderableList.drawElementCallback = drawCallback;
                reorderableList.onSelectCallback = selectCallback;
                reorderableList.onAddCallback = addCallback;
                reorderableList.onRemoveCallback = removeCallback;
                reorderableList.serializedProperty = serializedProperty;
                if (EditorPrefs.GetInt(preferencesKey, -1) != -1) {
                    reorderableList.index = EditorPrefs.GetInt(preferencesKey, -1);
                }
            }

            // ReorderableLists do not like indentation.
            var indentLevel = EditorGUI.indentLevel;
            while (EditorGUI.indentLevel > 0) {
                EditorGUI.indentLevel--;
            }

            var listRect = GUILayoutUtility.GetRect(0, reorderableList.GetHeight());
            // Indent the list so it lines up with the rest of the content.
            listRect.x += Shared.Editor.Inspectors.Utility.InspectorUtility.IndentWidth * indentLevel;
            listRect.xMax -= Shared.Editor.Inspectors.Utility.InspectorUtility.IndentWidth * indentLevel;
            var prevPref = EditorPrefs.GetInt(preferencesKey, 0);
            reorderableList.DoList(listRect);
            while (EditorGUI.indentLevel < indentLevel) {
                EditorGUI.indentLevel++;
            }
            if (prevPref != EditorPrefs.GetInt(preferencesKey, 0)) {
                reorderableList = null;
                reorderableAudioList = null;
                reorderableStateList = null;
                return;
            }

            if (EditorPrefs.GetInt(preferencesKey, 0) >= animatorAudioStateSet.States.Length || EditorPrefs.GetInt(preferencesKey, 0) < 0) {
                EditorPrefs.SetInt(preferencesKey, 0);
            }

            serializedProperty = serializedProperty.GetArrayElementAtIndex(EditorPrefs.GetInt(preferencesKey, 0));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("m_AllowDuringMovement"));
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("m_RequireGrounded"));
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("m_StateName"));
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("m_ItemSubstateIndex"));
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }

            var animatorAudioState = animatorAudioStateSet.States[EditorPrefs.GetInt(preferencesKey, 0)];
            reorderableAudioList = AudioClipSetInspector.DrawAudioClipSet(animatorAudioState.AudioClipSet, reorderableAudioList, drawAudioElementCallback, addAudioCallback, removeAudioCallback);
            if (Shared.Editor.Inspectors.Utility.InspectorUtility.Foldout(animatorAudioState, new GUIContent("States"), false)) {
                EditorGUI.indentLevel--;
                // The MovementType class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the ability's state list. When the reorderable list is drawn
                // the ability object will be used so it's like the dummy object never existed.
                var gameObject = new GameObject();
                try {
                    var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                    stateIndexHelper.StateIndexData = new int[animatorAudioState.States.Length];
                    for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                        stateIndexHelper.StateIndexData[i] = i;
                    }
                    var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                    reorderableStateList = Shared.Editor.Inspectors.StateSystem.StateInspector.DrawStates(reorderableStateList, new SerializedObject(target), 
                                                                stateIndexSerializedObject.FindProperty("m_StateIndexData"),
                                                                statePreferencesKey, stateDrawElementCallback, stateAddCallback,
                                                                stateReorderCallback, stateRemoveCallback);
                } catch (Exception /*e*/) { }
                UnityEngine.Object.DestroyImmediate(gameObject);
                EditorGUI.indentLevel++;
            }
            GUILayout.Space(5);
        }

        /// <summary>
        /// Searches for an adds any AnimatorAudioStateSelectors available in the project.
        /// </summary>
        private static void PopulateAnimatorAudioStateSelectorTypes()
        {
            if (s_SelectorTypeCache != null) {
                return;
            }

            s_SelectorTypeCache = new List<Type>();
            s_SelectorTypeNameCache = new List<string>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must derive from AnimatorAudioStateSelector.
                    if (!typeof(AnimatorAudioStateSelector).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    s_SelectorTypeCache.Add(assemblyTypes[j]);
                    s_SelectorTypeNameCache.Add(Utility.InspectorUtility.DisplayTypeName(assemblyTypes[j], false));
                }
            }
        }

        /// <summary>
        /// Draws the header for the AnimatorAudioState list.
        /// </summary>
        private static void OnAnimatorAudioStateListHeaderDraw(Rect rect)
        {
            var activeRect = rect;
            activeRect.x += 13;
            activeRect.width -= 33;
            EditorGUI.LabelField(activeRect, "Animator Audio States");

            activeRect.x += activeRect.width - 32;
            activeRect.width = 50;
            EditorGUI.LabelField(activeRect, "Enabled");
        }

        /// <summary>
        /// Draws the AnimatorAudioState element.
        /// </summary>
        public static void OnAnimatorAudioStateElementDraw(ReorderableList list, AnimatorAudioStateSet animatorAudioStateSet, Rect rect, int index, bool isActive, bool isFocused, UnityEngine.Object target)
        {
            // Reduce the rect width so the enabled toggle can be added.
            var activeRect = rect;
            activeRect.width -= 20;
            GUI.Label(activeRect, animatorAudioStateSet.States[index].ItemSubstateIndex.ToString());

            // Draw the enabled toggle and serialize if there is a change.
            EditorGUI.BeginChangeCheck();
            activeRect = rect;
            activeRect.x += activeRect.width - 32;
            activeRect.width = 20;
            if (list.serializedProperty != null) {
                list.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("m_Enabled").boolValue = EditorGUI.Toggle(activeRect, animatorAudioStateSet.States[index].Enabled);
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
            } else {
                animatorAudioStateSet.States[index].Enabled = EditorGUI.Toggle(activeRect, animatorAudioStateSet.States[index].Enabled);
            }
            if (EditorGUI.EndChangeCheck()) {
                Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(target, "Change Value");
            }
        }

        /// <summary>
        /// A new element has been selected.
        /// </summary>
        public static void OnAnimatorAudioStateSelect(ReorderableList list, string preferencesKey)
        {
            EditorPrefs.SetInt(preferencesKey, list.index);
        }

        /// <summary>
        /// Adds a new AnimatorAudioState element.
        /// </summary>
        public static void OnAnimatorAudioStateListAdd(ReorderableList list, AnimatorAudioStateSet animatorAudioStateSet, string preferencesKey)
        {
            // Add a new state.
            EditorPrefs.SetInt(preferencesKey, list.count);
            if (list.serializedProperty != null) {
                list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
            } else {
                var states = animatorAudioStateSet.States;
                Array.Resize(ref states, states.Length + 1);
                var state = new AnimatorAudioStateSet.AnimatorAudioState();
                states[states.Length - 1] = state;
                animatorAudioStateSet.States = states;
            }
            list.displayRemove = true;
        }

        /// <summary>
        /// Removes the selected AnimatorAudioState element.
        /// </summary>
        public static void OnAnimatorAudioStateListRemove(ReorderableList list, AnimatorAudioStateSet animatorAudioStateSet, string preferencesKey)
        {
            // Removes the current state.
            if (list.serializedProperty != null) {
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
            } else {
                var stateList = new List<AnimatorAudioStateSet.AnimatorAudioState>(animatorAudioStateSet.States);
                stateList.RemoveAt(list.index);
                animatorAudioStateSet.States = stateList.ToArray();
            }
            list.displayRemove = list.count > 1;
            EditorPrefs.SetInt(preferencesKey, list.index - 1);
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        public static void OnAudioClipDraw(ReorderableList list, Rect rect, int index, AnimatorAudioStateSet.AnimatorAudioState[] animatorAudioStates, string preferencesKey, UnityEngine.Object target)
        {
            var state = animatorAudioStates[EditorPrefs.GetInt(preferencesKey, 0)];
            AudioClipSetInspector.OnAudioClipDraw(list, rect, index, state.AudioClipSet, target);
        }

        /// <summary>
        /// Adds a new AudioClip element.
        /// </summary>
        public static void OnAudioClipListAdd(ReorderableList list, AnimatorAudioStateSet.AnimatorAudioState[] animatorAudioStates, string preferencesKey, UnityEngine.Object target)
        {
            var state = animatorAudioStates[EditorPrefs.GetInt(preferencesKey, 0)];
            AudioClipSetInspector.OnAudioClipListAdd(list, state.AudioClipSet, target);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        public static void OnAudioClipListRemove(ReorderableList list, AnimatorAudioStateSet.AnimatorAudioState[] animatorAudioStates, string preferencesKey, UnityEngine.Object target)
        {
            var state = animatorAudioStates[EditorPrefs.GetInt(preferencesKey, 0)];
            AudioClipSetInspector.OnAudioClipListRemove(list, state.AudioClipSet, target);
        }
    }
}
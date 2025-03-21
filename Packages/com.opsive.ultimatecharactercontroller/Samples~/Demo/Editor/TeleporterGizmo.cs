﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Demo
{
    using Opsive.UltimateCharacterController.Demo.Objects;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Shows a custom gizmo for the Teleporter component.
    /// </summary>
    public class TeleporterGizmo
    {
        /// <summary>
        /// Draws the teleporter gizmo.
        /// </summary>
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawSpawnPointGizmo(Teleporter teleporter, GizmoType gizmoType)
        {
            var boxCollider = teleporter.GetComponent<BoxCollider>();
            if (boxCollider != null) {
                Gizmos.color = teleporter.GizmoColor;
                var teleporterTransform = teleporter.transform;
                Gizmos.matrix = Matrix4x4.TRS(teleporterTransform.position, teleporterTransform.rotation, teleporterTransform.lossyScale);
                var localScale = teleporterTransform.localScale;
                Gizmos.DrawCube(boxCollider.center, Vector3.Scale(boxCollider.size, localScale));

                Gizmos.color = InspectorUtility.GetContrastColor(teleporter.GizmoColor);
                Gizmos.DrawWireCube(boxCollider.center, Vector3.Scale(boxCollider.size, localScale));
            }

            if (teleporter.Destination != null) {
                Gizmos.color = teleporter.GizmoColor;
                Gizmos.matrix = Matrix4x4.TRS(teleporter.Destination.position, teleporter.Destination.rotation, teleporter.Destination.lossyScale);
                Gizmos.DrawSphere(Vector3.zero, 0.2f);

                Gizmos.color = InspectorUtility.GetContrastColor(teleporter.GizmoColor);
                Gizmos.DrawWireSphere(Vector3.zero, 0.2f);
            }
        }
    }
}
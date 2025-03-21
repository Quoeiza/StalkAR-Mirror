﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.CastEffects
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Spawns a particle when the cast is performed.
    /// </summary>
    [System.Serializable]
    public class SpawnParticle : MagicMultiTargetCastEffectModule, IMagicObjectAction
    {
        [Tooltip("The particle prefab that should be spawned.")]
        [SerializeField] protected GameObject m_ParticlePrefab;
        [Tooltip("The positional offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The rotational offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_RotationOffset;
        [Tooltip("Should the particle be parented to the origin?")]
        [SerializeField] protected bool m_ParentToOrigin;
        [Tooltip("Should the directional vector be projected onto the character's normal plane?")]
        [SerializeField] protected bool m_ProjectDirectionOnPlane;
        [Tooltip("Should the particle's parent be cleared when the cast stops?")]
        [SerializeField] protected bool m_ClearParentOnStop;
        [Tooltip("Should the particle's Length Scale be set?")]
        [SerializeField] protected bool m_SetRendererLengthScale;
        [Tooltip("Additional value to add to the particle's Length Scale.")]
        [SerializeField] protected float m_AdditionalLength = 0.1f;
        [Tooltip("The layer that the particle should occupy.")]
        [Shared.Utility.Layer] [SerializeField] protected int m_ParticleLayer = LayerManager.IgnoreRaycast;
        [Tooltip("The duration of the alpha fade in.")]
        [SerializeField] protected float m_FadeInDuration;
        [Tooltip("The duration of the alpha fade out.")]
        [SerializeField] protected float m_FadeOutDuration;
        [Tooltip("The name of the material's color property.")]
        [SerializeField] protected string m_MaterialColorName = "_TintColor";
        [Tooltip("The delta step of the alpha fade.")]
        [SerializeField] protected float m_FadeStep = 0.05f;

        public GameObject ParticlePrefab { get { return m_ParticlePrefab; } set { if (m_ParticlePrefab != value) { m_ParticlePrefab = value; m_Renderers = null; } } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public Vector3 RotationOffset { get { return m_RotationOffset; } set { m_RotationOffset = value; } }
        public bool ParentToOrigin { get { return m_ParentToOrigin; } set { m_ParentToOrigin = value; } }
        public bool ProjectDirectionOnPlane { get { return m_ProjectDirectionOnPlane; } set { m_ProjectDirectionOnPlane = value; } }
        public bool ClearParentOnStop { get { return m_ClearParentOnStop; } set { m_ClearParentOnStop = value; } }
        public bool SetRendererLengthScale { get { return m_SetRendererLengthScale; } set { m_SetRendererLengthScale = value; } }
        public float AdditionalLength { get { return m_AdditionalLength; } set { m_AdditionalLength = value; } }
        public int ParticleLayer { get { return m_ParticleLayer; }
            set {
                m_ParticleLayer = value;
                if (m_ParticleTransform == null) {
                    return;
                }
                m_ParticleTransform.SetLayerRecursively(m_ParticleLayer);
            }
        }
        public float FadeInDuration { get { return m_FadeInDuration; } set { m_FadeInDuration = value; } }
        public float FadeOutDuration { get { return m_FadeOutDuration; } set { m_FadeOutDuration = value; } }
        public float FadeStep { get { return m_FadeStep; } set { m_FadeStep = value; } }
        
        private Transform m_ParticleTransform;
        private ParticleSystem m_ParticleSystem;
        private ParticleSystemRenderer[] m_Renderers;
        private ScheduledEventBase m_FadeEvent;

        private int m_MaterialColorID;
        private bool m_Active;

        private MagicCastData m_CastData;

        public GameObject SpawnedGameObject { 
            set {
                if (m_FadeEvent != null) {
                    Scheduler.Cancel(m_FadeEvent);
                    m_FadeEvent = null;
                    SetRendererAlpha(0);
                }
                m_ParticleTransform = value.transform;
                m_ParticleSystem = value.GetCachedComponent<ParticleSystem>();
                m_Renderers = m_ParticleSystem.GetComponentsInChildren<ParticleSystemRenderer>();
                StartMaterialFade(value);
                m_Active = true;
            } 
        }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_MaterialColorID = Shader.PropertyToID(m_MaterialColorName);
        }

        /// <summary>
        /// Performs the cast.
        /// </summary>
        /// <param name="useDataStream">The use data stream, contains the cast data.</param>
        protected override void DoCastInternal(MagicUseDataStream useDataStream)
        {
            m_CastData = useDataStream.CastData;
            m_CastID = m_CastData.CastID;

            Vector3 position;
            Quaternion rotation;
            DetermineParticlePositionAndRotation(out position, out rotation);

            // If the cast is currently active then the particle should be reused.
            if (m_Active) {
                if (m_SetRendererLengthScale) {
                    SetRendererLength(m_CastData.CastOrigin.position, m_CastData.CastTargetPosition);
                }

                if (!m_ParentToOrigin) {
                    m_ParticleTransform.SetPositionAndRotation(position, rotation);
                } else {
                    m_ParticleTransform.rotation = rotation;
                }
                return;
            }

            if (m_ParticlePrefab == null) {
                Debug.LogError("Error: A Particle Prefab must be specified.", MagicAction);
                return;
            }

            if (m_FadeEvent != null) {
                Scheduler.Cancel(m_FadeEvent);
                m_FadeEvent = null;
                SetRendererAlpha(0);
            }
            var obj = ObjectPoolBase.Instantiate(m_ParticlePrefab, position, rotation, m_ParentToOrigin ? m_CastData.CastOrigin : null);
            m_ParticleTransform = obj.transform;
            m_ParticleTransform.SetLayerRecursively(m_ParticleLayer);
            m_ParticleSystem = obj.GetCachedComponent<ParticleSystem>();

            if (m_ParticleSystem == null) {
                Debug.LogError($"Error: A Particle System must be specified on the particle {m_ParticlePrefab}.", MagicAction);
                return;
            }

            m_ParticleSystem.Clear(true);
            m_Renderers = null;
            if (m_SetRendererLengthScale) {
                SetRendererLength(m_CastData.CastOrigin.position, m_CastData.CastTargetPosition);
            }
            StartMaterialFade(obj);

            // The MagicParticle can determine the impacts.
            var magicParticle = obj.GetComponent<MagicParticle>();
            if (magicParticle != null) {
                magicParticle.Initialize(MagicAction, m_CastID);
            }
            m_Active = true;
            CharacterItemAction.OnLateUpdateEvent += OnLateUpdate;
        }

        /// <summary>
        /// Determines the position and rotation of the particle.
        /// </summary>
        /// <param name="position">The returned position of the particle.</param>
        /// <param name="rotation">The returned rotation of the particle.</param>
        private void DetermineParticlePositionAndRotation(out Vector3 position, out Quaternion rotation)
        {
            var direction = m_CastData.Direction;
            position = MathUtility.TransformPoint(m_CastData.CastOrigin.position, CharacterTransform.rotation, m_PositionOffset);
            if (m_ProjectDirectionOnPlane) {
                direction = Vector3.ProjectOnPlane(direction, CharacterLocomotion.Up);
            }
            // The direction can't be 0.
            if (direction.sqrMagnitude == 0) {
                direction = CharacterLocomotion.transform.forward;
            }

            rotation = Quaternion.LookRotation(direction, CharacterLocomotion.Up) * Quaternion.Euler(m_RotationOffset);
        }

        /// <summary>
        /// Updates the particle's position and rotation during late update.
        /// </summary>
        private void OnLateUpdate()
        {
            if (!m_Active) {
                return;
            }

            Vector3 position;
            Quaternion rotation;
            DetermineParticlePositionAndRotation(out position, out rotation);

            if (!m_ParentToOrigin) {
                m_ParticleTransform.SetPositionAndRotation(position, rotation);
            } else {
                m_ParticleTransform.rotation = rotation;
            }
        }

        /// <summary>
        /// Sets the length of the renderer.
        /// </summary>
        /// <param name="position">The position that the particle is spawned from.</param>
        /// <param name="targetPosition">The target position of the cast.</param>
        private void SetRendererLength(Vector3 position, Vector3 targetPosition)
        {
            if (m_Renderers == null) {
                m_Renderers = m_ParticleSystem.GetComponentsInChildren<ParticleSystemRenderer>();
            }
            for (int i = 0; i < m_Renderers.Length; ++i) {
                m_Renderers[i].lengthScale = (position - targetPosition).magnitude + m_AdditionalLength;
            }
        }

        /// <summary>
        /// Starts to fade the particle materials.
        /// </summary>
        /// <param name="particle">The GameObject that the particle belongs to.</param>
        private void StartMaterialFade(GameObject particle)
        {
            // Optionally fade the particle into the world.
            if (m_FadeInDuration > 0) {
                if (m_Renderers == null) {
                    m_Renderers = particle.GetComponentsInChildren<ParticleSystemRenderer>();
                }
                SetRendererAlpha(0);
                var interval = m_FadeInDuration / (1 / m_FadeStep);
                m_FadeEvent = Scheduler.Schedule(interval, FadeMaterials, interval, 1f);
            }
        }

        /// <summary>
        /// Sets the alpha of the renderers.
        /// </summary>
        /// <param name="alpha">The alpha that should be set.</param>
        private void SetRendererAlpha(float alpha)
        {
            for (int i = 0; i < m_Renderers.Length; ++i) {
                if (!m_Renderers[i].material.HasProperty(m_MaterialColorID)) {
                    continue;
                }
                var color = m_Renderers[i].material.GetColor(m_MaterialColorID);
                color.a = alpha;
                m_Renderers[i].material.SetColor(m_MaterialColorID, color);
            }
        }

        /// <summary>
        /// Fades all of the materials which belong to the renderers.
        /// </summary>
        /// <param name="interval">The time interval which updates the fade.</param>
        /// <param name="targetAlpha">The target alpha value.</param>
        private void FadeMaterials(float interval, float targetAlpha)
        {
            var arrived = true;
            for (int i = 0; i < m_Renderers.Length; ++i) {
                if (!m_Renderers[i].material.HasProperty(m_MaterialColorID)) {
                    continue;
                }
                var color = m_Renderers[i].material.GetColor(m_MaterialColorID);
                color.a = Mathf.MoveTowards(color.a, targetAlpha, m_FadeStep);
                m_Renderers[i].material.SetColor(m_MaterialColorID, color);

                // Schedule the method again if the material isn't at the desired fade value.
                if (color.a != targetAlpha) {
                    arrived = false;
                }
            }
            if (arrived) {
                m_FadeEvent = null;
            } else {
                m_FadeEvent = Scheduler.Schedule(interval, FadeMaterials, interval, targetAlpha);
            }
        }

        /// <summary>
        /// The cast will be stopped. Start any cleanup.
        /// </summary>
        public override void CastWillStop()
        {
            base.CastWillStop();
            if (m_ClearParentOnStop) {
                m_ParticleSystem.transform.parent = null;
            }
        }

        /// <summary>
        /// Stops the cast.
        /// </summary>
        public override void StopCast()
        {
            if (!m_Active) {
                return;
            }

            m_ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            // Optionally fade the particle out of the world.
            if (m_FadeOutDuration > 0) {
                if (m_FadeEvent != null) {
                    Scheduler.Cancel(m_FadeEvent);
                    m_FadeEvent = null;
                }
                if (m_Renderers == null) {
                    m_Renderers = m_ParticleSystem.GetComponentsInChildren<ParticleSystemRenderer>();
                }
                var interval = m_FadeOutDuration / (1 / m_FadeStep);
                // Reset the alpha if the renderers have no fade in duration.
                if (m_FadeInDuration == 0) {
                    SetRendererAlpha(1);
                }
                m_FadeEvent = Scheduler.Schedule(interval, FadeMaterials, interval, 0f);
            }

            m_Active = false;
            CharacterItemAction.OnLateUpdateEvent -= OnLateUpdate;
            base.StopCast();
        }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="firstPerson">Changed to first person?.</param>
        public override void OnChangePerspectives(bool firstPerson)
        {
            if (!m_Active) {
                return;
            }
            
            var origin = MagicAction?.MagicUseDataStream?.CastData?.CastOrigin;
            var spawnedTransform = m_ParticleSystem.transform;
            if (spawnedTransform.parent == origin) {
                return;
            }

            spawnedTransform.parent = origin;
            spawnedTransform.position = MathUtility.TransformPoint(origin.position, CharacterTransform.rotation, m_PositionOffset);
        }
    }
}
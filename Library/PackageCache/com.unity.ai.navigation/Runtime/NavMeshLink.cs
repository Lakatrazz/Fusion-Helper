using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.AI;
#pragma warning disable IDE1006 // Unity-specific lower case public property names

namespace Unity.AI.Navigation
{
    /// <summary> Component used to create a navigable link between two NavMesh locations. </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-101)]
    [AddComponentMenu("Navigation/NavMesh Link", 33)]
    [HelpURL(HelpUrls.Manual + "NavMeshLink.html")]
    public partial class NavMeshLink : MonoBehaviour
    {
        // Serialized version is used to upgrade older serialized data to the current format.
        // Version 0: Initial version.
        // Version 1: Added m_IsOverridingCost field and made m_CostModifier always positive.
        [SerializeField, HideInInspector]
        byte m_SerializedVersion = 0;

        [SerializeField]
        int m_AgentTypeID;

        [SerializeField]
        Vector3 m_StartPoint = new(0.0f, 0.0f, -2.5f);

        [SerializeField]
        Vector3 m_EndPoint = new(0.0f, 0.0f, 2.5f);

        [SerializeField]
        Transform m_StartTransform;

        [SerializeField]
        Transform m_EndTransform;

        [SerializeField]
        bool m_Activated = true;

        [SerializeField]
        float m_Width;

        // This field's value in combination with m_IsOverridingCost determines the value of the costModifier property,
        // where m_IsOverridingCost determines the sign of the value. The costModifier property is positive or zero when
        // m_IsOverridingCost is true, and negative when m_IsOverridingCost is false.
        // Note that when m_SerializedVersion >= 1, m_CostModifier will always become positive or zero. Newly created
        // components are always upgraded to at least version 1 at initialization time.
        [SerializeField]
        [Min(0f)]
        float m_CostModifier = -1f;

        [SerializeField]
        bool m_IsOverridingCost = false;

        [SerializeField]
        bool m_Bidirectional = true;

        [SerializeField]
        bool m_AutoUpdatePosition;

        [SerializeField]
        int m_Area;

        /// <summary> Gets or sets the type of agent that can use the link. </summary>
        public int agentTypeID
        {
            get => m_AgentTypeID;
            set
            {
                if (value == m_AgentTypeID)
                    return;

                m_AgentTypeID = value;
                UpdateLink();
            }
        }

        /// <summary> Gets or sets the local position at the middle of the link's start edge, relative to the GameObject origin. </summary>
        /// <remarks> This property determines the position of the link's start edge only when <see cref="startTransform"/> is `null`. Otherwise, it is the `startTransform` that determines the edge's position.  <br/>
        /// The world scale of the GameObject is never used.</remarks>
        public Vector3 startPoint
        {
            get => m_StartPoint;
            set
            {
                if (value == m_StartPoint)
                    return;

                m_StartPoint = value;
                UpdateLink();
            }
        }

        /// <summary> Gets or sets the local position at the middle of the link's end edge, relative to the GameObject origin. </summary>
        /// <remarks> This property determines the position of the link's end edge only when <see cref="endTransform"/> is `null`. Otherwise, it is the `endTransform` that determines the edge's position. <br/>
        /// The world scale of the GameObject is never used.</remarks>
        public Vector3 endPoint
        {
            get => m_EndPoint;
            set
            {
                if (value == m_EndPoint)
                    return;

                m_EndPoint = value;
                UpdateLink();
            }
        }

        /// <summary> Gets or sets the <see cref="Transform"/> tracked by the middle of the link's start edge. </summary>
        /// <remarks> The link places the start edge at the world position of the object referenced by this property. In that case <see cref="startPoint"/> is not used. Otherwise, when this property is `null`, the component applies the GameObject's translation and rotation as a transform to <see cref="startPoint"/> in order to establish the world position of the link's start edge. </remarks>
        public Transform startTransform
        {
            get => m_StartTransform;
            set
            {
                if (value == m_StartTransform)
                    return;

                m_StartTransform = value;

                UpdateLink();
            }
        }

        /// <summary> Gets or sets the <see cref="Transform"/> tracked by the middle of the link's end edge. </summary>
        /// <remarks> The link places the end edge at the world position of the object referenced by this property. In that case <see cref="endPoint"/> is not used. Otherwise, when this property is `null`, the component applies the GameObject's translation and rotation as a transform to <see cref="endPoint"/> in order to establish the world position of the link's end edge. </remarks>
        public Transform endTransform
        {
            get => m_EndTransform;
            set
            {
                if (value == m_EndTransform)
                    return;

                m_EndTransform = value;

                UpdateLink();
            }
        }

        /// <summary> The width of the segments making up the ends of the link. </summary>
        /// <remarks> The segments are created perpendicular to the line from start to end, in the XZ plane of the GameObject. </remarks>
        public float width
        {
            get => m_Width;
            set
            {
                if (value.Equals(m_Width))
                    return;

                m_Width = value;
                UpdateLink();
            }
        }

        /// <summary> Gets or sets a value that determines the cost of traversing the link.</summary>
        /// <remarks> A negative value implies that the cost of traversing the link is obtained based on the area type.<br/>
        /// A positive or zero value overrides the cost associated with the area type.</remarks>
        public float costModifier
        {
            get => m_IsOverridingCost ? m_CostModifier : -m_CostModifier;
            set
            {
                var shouldOverride = value >= 0f;
                if (value.Equals(costModifier) && shouldOverride == m_IsOverridingCost)
                    return;

                m_IsOverridingCost = shouldOverride;
                m_CostModifier = Mathf.Abs(value);
                UpdateLink();
            }
        }

        /// <summary> Gets or sets whether agents can traverse the link in both directions. </summary>
        /// <remarks> When a link connects to NavMeshes at both ends, agents can always traverse that link from the start position to the end position. When this property is set to `true` it allows the agents to traverse the link from the end position to the start position as well. When the value is `false` the agents will not traverse the link from the end position to the start position. </remarks>
        public bool bidirectional
        {
            get => m_Bidirectional;
            set
            {
                if (value == m_Bidirectional)
                    return;

                m_Bidirectional = value;
                UpdateLink();
            }
        }

        /// <summary> Gets or sets whether the world positions of the link's edges update whenever
        /// the GameObject transform, the <see cref="startTransform"/> or the <see cref="endTransform"/> change at runtime. </summary>
        public bool autoUpdate
        {
            get => m_AutoUpdatePosition;
            set
            {
                if (value == m_AutoUpdatePosition)
                    return;

                m_AutoUpdatePosition = value;

                if (m_AutoUpdatePosition)
                    AddTracking(this);
                else
                    RemoveTracking(this);
            }
        }

        /// <summary> The area type of the link. </summary>
        public int area
        {
            get => m_Area;
            set
            {
                if (value == m_Area)
                    return;

                m_Area = value;
                UpdateLink();
            }
        }

        /// <summary> Gets or sets whether the link can be traversed by agents. </summary>
        /// <remarks> When this property is set to `true` it allows the agents to traverse the link. When the value is `false` no paths pass through this link and no agent can traverse it as part of their autonomous movement. </remarks>
        public bool activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                NavMesh.SetLinkActive(m_LinkInstance, m_Activated);
            }
        }

        /// <summary> Checks whether any agent occupies the link at this moment in time. </summary>
        /// <remarks> This property evaluates the internal state of the link every time it is used. </remarks>
        public bool occupied => NavMesh.IsLinkOccupied(m_LinkInstance);

        NavMeshLinkInstance m_LinkInstance;

        bool m_StartTransformWasEmpty = true;
        bool m_EndTransformWasEmpty = true;

        Vector3 m_LastStartWorldPosition = Vector3.positiveInfinity;
        Vector3 m_LastEndWorldPosition = Vector3.positiveInfinity;
        Vector3 m_LastPosition = Vector3.positiveInfinity;
        Quaternion m_LastRotation = Quaternion.identity;

        static readonly List<NavMeshLink> s_Tracked = new();

#if UNITY_EDITOR
        bool m_DelayEndpointUpgrade;
        static string s_LastWarnedPrefab;
        static double s_NextPrefabWarningTime;
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ClearTrackedList()
        {
            s_Tracked.Clear();
        }

        void UpgradeSerializedVersion()
        {
            if (m_SerializedVersion < 1)
            {
#if UNITY_EDITOR
                if (!StartEndpointUpgrade())
                    return;
#endif
                m_SerializedVersion = 1;
                m_IsOverridingCost = m_CostModifier >= 0f;
                m_CostModifier = Mathf.Abs(m_CostModifier);

                if (m_StartTransform == gameObject.transform)
                    m_StartTransform = null;

                if (m_EndTransform == gameObject.transform)
                    m_EndTransform = null;
            }
        }

        // ensures serialized version is up-to-date at run-time, in case it was not updated in the Editor
        void Awake() => UpgradeSerializedVersion();

        void OnEnable()
        {
            AddLink();
            if (m_AutoUpdatePosition && NavMesh.IsLinkValid(m_LinkInstance))
                AddTracking(this);
        }

        void OnDisable()
        {
            RemoveTracking(this);
            NavMesh.RemoveLink(m_LinkInstance);
        }

        /// <summary> Replaces the link with a new one using the current settings. </summary>
        public void UpdateLink()
        {
            if (!isActiveAndEnabled)
                return;

            NavMesh.RemoveLink(m_LinkInstance);
            AddLink();
        }

        static void AddTracking(NavMeshLink link)
        {
#if UNITY_EDITOR
            if (s_Tracked.Contains(link))
            {
                Debug.LogError("Link is already tracked: " + link);
                return;
            }
#endif
            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate += UpdateTrackedInstances;

            s_Tracked.Add(link);

            link.RecordEndpointTransforms();
        }

        static void RemoveTracking(NavMeshLink link)
        {
            s_Tracked.Remove(link);

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate -= UpdateTrackedInstances;
        }

        /// <summary>Gets the world positions of the start and end points for the link.</summary>
        /// <param name="worldStartPosition">Returns the world position of <see cref="startTransform"/> if it is not <c>null</c>; otherwise, <see cref="startPoint"/> transformed into world space.</param>
        /// <param name="worldEndPosition">Returns the world position of <see cref="endTransform"/> if it is not <c>null</c>; otherwise, <see cref="endPoint"/> transformed into world space.</param>
        internal void GetWorldPositions(
            out Vector3 worldStartPosition,
            out Vector3 worldEndPosition)
        {
            var startIsLocal = m_StartTransform == null;
            var endIsLocal = m_EndTransform == null;
            var toWorld = startIsLocal || endIsLocal ? LocalToWorldUnscaled() : Matrix4x4.identity;

            worldStartPosition = startIsLocal ? toWorld.MultiplyPoint3x4(m_StartPoint) : m_StartTransform.position;
            worldEndPosition = endIsLocal ? toWorld.MultiplyPoint3x4(m_EndPoint) : m_EndTransform.position;
        }

        /// <summary>Gets the positions of the start and end points in the local space of the link.</summary>
        /// <param name="localStartPosition">Returns the local position of <see cref="startTransform"/> if it is not <c>null</c>; otherwise, <see cref="startPoint"/>.</param>
        /// <param name="localEndPosition">Returns the local position of <see cref="endTransform"/> if it is not <c>null</c>; otherwise, <see cref="endPoint"/>.</param>
        internal void GetLocalPositions(
            out Vector3 localStartPosition,
            out Vector3 localEndPosition)
        {
            var startIsLocal = m_StartTransform == null;
            var endIsLocal = m_EndTransform == null;
            var toLocal = startIsLocal && endIsLocal ? Matrix4x4.identity : LocalToWorldUnscaled().inverse;

            localStartPosition = startIsLocal ? m_StartPoint : toLocal.MultiplyPoint3x4(m_StartTransform.position);
            localEndPosition = endIsLocal ? m_EndPoint : toLocal.MultiplyPoint3x4(m_EndTransform.position);
        }

        void AddLink()
        {
#if UNITY_EDITOR
            if (NavMesh.IsLinkValid(m_LinkInstance))
            {
                Debug.LogError("Link is already added: " + this);
                return;
            }
#endif
            GetLocalPositions(out var localStartPosition, out var localEndPosition);
            var link = new NavMeshLinkData
            {
                startPosition = localStartPosition,
                endPosition = localEndPosition,
                width = m_Width,
                costModifier = costModifier,
                bidirectional = m_Bidirectional,
                area = m_Area,
                agentTypeID = m_AgentTypeID,
            };
            m_LinkInstance = NavMesh.AddLink(link, transform.position, transform.rotation);
            if (NavMesh.IsLinkValid(m_LinkInstance))
            {
                NavMesh.SetLinkOwner(m_LinkInstance, this);
                NavMesh.SetLinkActive(m_LinkInstance, m_Activated);
            }

            m_LastPosition = transform.position;
            m_LastRotation = transform.rotation;

            RecordEndpointTransforms();

            GetWorldPositions(out m_LastStartWorldPosition, out m_LastEndWorldPosition);
        }

        internal void RecordEndpointTransforms()
        {
            m_StartTransformWasEmpty = m_StartTransform == null;
            m_EndTransformWasEmpty = m_EndTransform == null;
        }

        internal bool HaveTransformsChanged()
        {
            var startIsLocal = m_StartTransform == null;
            var endIsLocal = m_EndTransform == null;

            if (startIsLocal && endIsLocal &&
                m_StartTransformWasEmpty && m_EndTransformWasEmpty &&
                transform.position == m_LastPosition && transform.rotation == m_LastRotation)
                return false;

            var toWorld = startIsLocal || endIsLocal ? LocalToWorldUnscaled() : Matrix4x4.identity;

            var startWorldPos = startIsLocal ? toWorld.MultiplyPoint3x4(m_StartPoint) : m_StartTransform.position;
            if (startWorldPos != m_LastStartWorldPosition)
                return true;

            var endWorldPos = endIsLocal ? toWorld.MultiplyPoint3x4(m_EndPoint) : m_EndTransform.position;
            return endWorldPos != m_LastEndWorldPosition;
        }

        internal Matrix4x4 LocalToWorldUnscaled()
        {
            return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        }

        void OnDidApplyAnimationProperties()
        {
            UpdateLink();
        }

        static void UpdateTrackedInstances()
        {
            foreach (var instance in s_Tracked)
            {
                if (instance.HaveTransformsChanged())
                    instance.UpdateLink();

                instance.RecordEndpointTransforms();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // Ensures serialized version is up-to-date in the Editor irrespective of GameObject active state
            UpgradeSerializedVersion();

            m_Width = Mathf.Max(0.0f, m_Width);

            if (!NavMesh.IsLinkValid(m_LinkInstance))
                return;

            UpdateLink();

            if (!m_AutoUpdatePosition)
            {
                RemoveTracking(this);
            }
            else if (!s_Tracked.Contains(this))
            {
                AddTracking(this);
            }
        }

        void Reset()
        {
            UpgradeSerializedVersion();
        }

        bool StartEndpointUpgrade()
        {
            m_DelayEndpointUpgrade =
                (m_StartTransform != null &&
                    m_StartTransform != gameObject.transform &&
                    m_StartPoint.sqrMagnitude > 0.0001f)
                || (m_EndTransform != null &&
                    m_EndTransform != gameObject.transform &&
                    m_EndPoint.sqrMagnitude > 0.0001f);

            if (m_DelayEndpointUpgrade)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(this))
                {
                    var isInstance = PrefabUtility.IsPartOfPrefabInstance(this);
                    var prefabPath = isInstance
                        ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject)
                        : AssetDatabase.GetAssetPath(gameObject);

                    if ((prefabPath != s_LastWarnedPrefab
                            || EditorApplication.timeSinceStartup > s_NextPrefabWarningTime)
                        && prefabPath != "")
                    {
                        var prefabToPing = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);

                        Debug.LogWarning(L10n.Tr(
                                "A NavMesh Link component has an outdated format. "
                                + "To upgrade it, open and save the prefab at: ") + prefabPath
                            + (isInstance
                                ? L10n.Tr(" . The prefab instance is ") + PrefabUtility.GetNearestPrefabInstanceRoot(gameObject).name
                                : ""),
                            prefabToPing);

                        s_LastWarnedPrefab = prefabPath;
                        s_NextPrefabWarningTime = EditorApplication.timeSinceStartup + 5f;
                    }

                    m_DelayEndpointUpgrade = false;
                    return false;
                }

                if (IsInAuthoringScene())
                {
                    EditorApplication.delayCall += CompleteEndpointUpgrade;

                    EditorApplication.delayCall -= WarnAboutUnsavedUpgrade;
                    EditorApplication.delayCall += WarnAboutUnsavedUpgrade;

                    EditorSceneManager.MarkSceneDirty(gameObject.scene);

                    Debug.Log(L10n.Tr(
                            "A NavMesh Link component has auto-upgraded and it references a newly created object. "
                            + "Save your scene to keep the changes. "
                            + "GameObject: ") + gameObject.name,
                        gameObject);
                }
                else
                {
                    Debug.LogWarning(L10n.Tr(
                            "The NavMesh Link component does not reference the intended transforms. " +
                            "To correct it, save this NavMesh Link again at edit time. GameObject: ") + gameObject.name,
                        gameObject);
                }
            }

            return true;
        }

        static void WarnAboutUnsavedUpgrade()
        {
            Debug.LogWarning(L10n.Tr(
                "At least one NavMesh Link component has auto-upgraded to a new format. "
                + "Save your scene to keep the changes. "));
        }

        void CompleteEndpointUpgrade()
        {
            var discardedByPrefabStageOnHiddenReload = this == null;
            if (discardedByPrefabStageOnHiddenReload ||
                gameObject == null || !m_DelayEndpointUpgrade)
                return;

            var linkIndexString = "";
            var allMyLinks = gameObject.GetComponents<NavMeshLink>();
            if (allMyLinks.Length > 1)
            {
                for (var i = 0; i < allMyLinks.Length; i++)
                {
                    if (allMyLinks[i] == this)
                    {
                        linkIndexString = " " + i;
                        break;
                    }
                }
            }

            var localToWorldUnscaled = LocalToWorldUnscaled();

            if (m_StartTransform != null &&
                m_StartTransform != gameObject.transform &&
                m_StartPoint.sqrMagnitude > 0.0001f)
            {
                var startGO = new GameObject($"Link Start {gameObject.name}{linkIndexString}");
                startGO.transform.SetParent(m_StartTransform);
                startGO.transform.position = localToWorldUnscaled.MultiplyPoint3x4(transform.InverseTransformPoint(m_StartTransform.position + m_StartPoint));
                m_StartTransform = startGO.transform;
            }

            if (m_EndTransform != null &&
                m_EndTransform != gameObject.transform &&
                m_EndPoint.sqrMagnitude > 0.0001f)
            {
                var endGO = new GameObject($"Link End {gameObject.name}{linkIndexString}");
                endGO.transform.SetParent(m_EndTransform);
                endGO.transform.position = localToWorldUnscaled.MultiplyPoint3x4(transform.InverseTransformPoint(m_EndTransform.position + m_EndPoint));
                m_EndTransform = endGO.transform;
            }

            if (IsInAuthoringScene())
                EditorSceneManager.MarkSceneDirty(gameObject.scene);

            m_DelayEndpointUpgrade = false;
        }

        bool IsInAuthoringScene()
        {
            return !EditorApplication.isPlaying || PrefabStageUtility.GetPrefabStage(gameObject) != null;
        }
#endif
    }
}

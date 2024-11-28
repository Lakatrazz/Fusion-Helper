using System;
using UnityEditor;
using UnityEngine;

namespace Unity.AI.Navigation.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshLink))]
    class NavMeshLinkEditor : UnityEditor.Editor
    {
        SerializedProperty m_AgentTypeID;
        SerializedProperty m_Area;
        SerializedProperty m_IsOverridingCost;
        SerializedProperty m_CostModifier;
        SerializedProperty m_AutoUpdatePosition;
        SerializedProperty m_Bidirectional;
        SerializedProperty m_EndPoint;
        SerializedProperty m_StartPoint;
        SerializedProperty m_EndTransform;
        SerializedProperty m_StartTransform;
        SerializedProperty m_Activated;
        SerializedProperty m_Width;

        static int s_SelectedID;
        static int s_SelectedPoint = -1;

        static Color s_HandleColor = new Color(255f, 167f, 39f, 210f) / 255;
        static Color s_HandleColorDisabled = new Color(255f * 0.75f, 167f * 0.75f, 39f * 0.75f, 100f) / 255;

        void OnEnable()
        {
            m_AgentTypeID = serializedObject.FindProperty("m_AgentTypeID");
            m_Area = serializedObject.FindProperty("m_Area");
            m_IsOverridingCost = serializedObject.FindProperty("m_IsOverridingCost");
            m_CostModifier = serializedObject.FindProperty("m_CostModifier");
            m_AutoUpdatePosition = serializedObject.FindProperty("m_AutoUpdatePosition");
            m_Bidirectional = serializedObject.FindProperty("m_Bidirectional");
            m_EndPoint = serializedObject.FindProperty("m_EndPoint");
            m_StartPoint = serializedObject.FindProperty("m_StartPoint");
            m_EndTransform = serializedObject.FindProperty("m_EndTransform");
            m_StartTransform = serializedObject.FindProperty("m_StartTransform");
            m_Activated = serializedObject.FindProperty("m_Activated");
            m_Width = serializedObject.FindProperty("m_Width");

            s_SelectedID = 0;
            s_SelectedPoint = -1;
        }

        internal static void AlignTransformToEndPoints(NavMeshLink navLink)
        {
            var toWorld = navLink.LocalToWorldUnscaled();

            var allLinksOnGameObject = navLink.gameObject.GetComponents<NavMeshLink>();
            Span<(Vector3 start, Vector3 end)> serializedEndpointsWorld = stackalloc (Vector3, Vector3)[allLinksOnGameObject.Length];
            Span<(Vector3 start, Vector3 end)> worldEndpointsBeforeAlign = stackalloc (Vector3, Vector3)[allLinksOnGameObject.Length];
            var thisLink = -1;
            for (var i = 0; i < allLinksOnGameObject.Length; i++)
            {
                var link = allLinksOnGameObject[i];
                if (link == navLink)
                    thisLink = i;

                // Store the world positions of the serialized point values
                serializedEndpointsWorld[i].start = toWorld.MultiplyPoint3x4(link.startPoint);
                serializedEndpointsWorld[i].end = toWorld.MultiplyPoint3x4(link.endPoint);

                // Store the world positions of the used endpoints
                link.GetWorldPositions(out var initialStartPos, out var initialEndPos);
                worldEndpointsBeforeAlign[i].start = initialStartPos;
                worldEndpointsBeforeAlign[i].end = initialEndPos;
            }

            // Use overall world position of target points or transforms to determine midpoint
            var worldStartPt = worldEndpointsBeforeAlign[thisLink].start;
            var worldEndPt = worldEndpointsBeforeAlign[thisLink].end;
            var startToEnd = worldEndPt - worldStartPt;
            var up = navLink.transform.up;

            // Flatten
            var forward = startToEnd - Vector3.Dot(up, startToEnd) * up;

            Undo.RecordObject(navLink.transform, Content.UndoReCenterOrigin);
            var middlePos = (worldEndPt + worldStartPt) * 0.5f;
            var lookTowardsEndAndKeepUp = Quaternion.LookRotation(forward, up);
            navLink.transform.SetPositionAndRotation(middlePos, lookTowardsEndAndKeepUp);

            // Transform points back to local space
            var toNewLocal = navLink.LocalToWorldUnscaled().inverse;
            for (var i = 0; i < allLinksOnGameObject.Length; i++)
            {
                var link = allLinksOnGameObject[i];
                Undo.RecordObject(link, Content.UndoReCenterOrigin);
                var startAtOwnChild = link.startTransform != null && link.startTransform.IsChildOf(link.transform);
                var endAtOwnChild = link.endTransform != null && link.endTransform.IsChildOf(link.transform);
                if (startAtOwnChild)
                    Undo.RecordObject(link.startTransform, Content.UndoReCenterOrigin);
                if (endAtOwnChild)
                    Undo.RecordObject(link.endTransform, Content.UndoReCenterOrigin);
                using (new DeferredLinkUpdateScope(link))
                {
                    link.startPoint = toNewLocal.MultiplyPoint3x4(serializedEndpointsWorld[i].start);
                    link.endPoint = toNewLocal.MultiplyPoint3x4(serializedEndpointsWorld[i].end);

                    // Ensure transform targets return to world positions, in case they are children of the NavMeshLink object
                    if (startAtOwnChild)
                        link.startTransform.position = worldEndpointsBeforeAlign[i].start;

                    if (endAtOwnChild)
                        link.endTransform.position = worldEndpointsBeforeAlign[i].end;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            NavMeshComponentsGUIUtility.AgentTypePopup(Content.AgentType, m_AgentTypeID);
            NavMeshComponentsGUIUtility.AreaPopup(Content.AreaType, m_Area);

            EditorGUILayout.PropertyField(m_IsOverridingCost, Content.CostOverrideToggle);
            EditorGUI.BeginDisabled(!m_IsOverridingCost.boolValue);
            EditorGUILayout.PropertyField(m_CostModifier, Content.CostModifier);
            EditorGUI.EndDisabled();

            EditorGUILayout.Space();

            m_StartPoint.isExpanded = EditorGUILayout.Foldout(m_StartPoint.isExpanded, Content.Positions, true);
            if (m_StartPoint.isExpanded)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(m_StartTransform, Content.StartTransform);
                using (new EditorGUI.DisabledScope(m_StartTransform.objectReferenceValue != null))
                    EditorGUILayout.PropertyField(m_StartPoint, Content.StartPoint);

                EditorGUILayout.PropertyField(m_EndTransform, Content.EndTransform);
                using (new EditorGUI.DisabledScope(m_EndTransform.objectReferenceValue != null))
                    EditorGUILayout.PropertyField(m_EndPoint, Content.EndPoint);

                var buttonRect = EditorGUILayout.GetControlRect();
                buttonRect = EditorGUI.IndentedRect(buttonRect);
                buttonRect.width -= EditorGUIUtility.standardVerticalSpacing;
                buttonRect.width *= 0.5f;
                if (GUI.Button(buttonRect, Content.ReverseDirectionButton))
                {
                    Undo.RecordObjects(targets, Content.UndoReverseDirection);
                    foreach (var nml in targets)
                        ReverseDirection((NavMeshLink)nml);
                    SceneView.RepaintAll();
                }

                buttonRect.x += buttonRect.width + EditorGUIUtility.standardVerticalSpacing;
                if (GUI.Button(buttonRect, Content.ReCenterButton))
                {
                    Undo.RecordObjects(targets, Content.UndoReCenterOrigin);
                    foreach (var nml in targets)
                        AlignTransformToEndPoints((NavMeshLink)nml);

                    SceneView.RepaintAll();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_AutoUpdatePosition, Content.AutoUpdatePositions);

            EditorGUILayout.PropertyField(m_Bidirectional, Content.Bidirectional);
            EditorGUILayout.PropertyField(m_Width, Content.Width);

            EditorGUILayout.PropertyField(m_Activated, Content.Activated);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
        }

        internal static void ReverseDirection(NavMeshLink navLink)
        {
            using (new DeferredLinkUpdateScope(navLink))
            {
                (navLink.startPoint, navLink.endPoint) = (navLink.endPoint, navLink.startPoint);
                (navLink.startTransform, navLink.endTransform) = (navLink.endTransform, navLink.startTransform);
            }
        }

        internal static Vector3 GetLocalDirectionRight(NavMeshLink navLink, out Vector3 localStartPosition, out Vector3 localEndPosition)
        {
            navLink.GetLocalPositions(out localStartPosition, out localEndPosition);
            var dir = localEndPosition - localStartPosition;
            return new Vector3(dir.z, 0.0f, -dir.x).normalized;
        }

        static void DrawLink(NavMeshLink navLink)
        {
            var right = GetLocalDirectionRight(navLink, out var startPos, out var endPos);
            var rad = navLink.width * 0.5f;
            var edgeRadius = right * rad;

            ReadOnlySpan<Vector3> corners = stackalloc[]
            {
                startPos - edgeRadius,
                startPos + edgeRadius,
                endPos + edgeRadius,
                endPos - edgeRadius
            };
            Gizmos.DrawLineStrip(corners, true);
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Active | GizmoType.Pickable)]
        static void RenderBoxGizmo(NavMeshLink navLink, GizmoType gizmoType)
        {
            if (!EditorApplication.isPlaying && navLink.isActiveAndEnabled && navLink.HaveTransformsChanged())
                navLink.UpdateLink();

            var color = s_HandleColor;
            if (!navLink.enabled)
                color = s_HandleColorDisabled;

            var oldColor = Gizmos.color;
            var oldMatrix = Gizmos.matrix;

            Gizmos.matrix = navLink.LocalToWorldUnscaled();
            Gizmos.color = color;
            DrawLink(navLink);

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navLink.transform.position, "NavMeshLink Icon", true);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        static void RenderBoxGizmoNotSelected(NavMeshLink navLink, GizmoType gizmoType)
        {
            if (!EditorApplication.isPlaying && navLink.isActiveAndEnabled && navLink.HaveTransformsChanged())
                navLink.UpdateLink();

            var color = s_HandleColor;
            if (!navLink.enabled)
                color = s_HandleColorDisabled;

            var oldColor = Gizmos.color;
            var oldMatrix = Gizmos.matrix;

            Gizmos.matrix = navLink.LocalToWorldUnscaled();
            Gizmos.color = color;
            DrawLink(navLink);

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navLink.transform.position, "NavMeshLink Icon", true);
        }

        public void OnSceneGUI()
        {
            var navLink = (NavMeshLink)target;
            if (!navLink.enabled)
                return;

            var toWorld = navLink.LocalToWorldUnscaled();

            navLink.GetWorldPositions(out var worldStartPt, out var worldEndPt);
            var worldMidPt = Vector3.Lerp(worldStartPt, worldEndPt, 0.35f);
            var startSize = HandleUtility.GetHandleSize(worldStartPt);
            var endSize = HandleUtility.GetHandleSize(worldEndPt);
            var midSize = HandleUtility.GetHandleSize(worldMidPt);

            var zup = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
            var right = toWorld.MultiplyVector(GetLocalDirectionRight(navLink, out _, out _));

            var oldColor = Handles.color;
            Handles.color = s_HandleColor;

            Vector3 newWorldPos;

            var startIsLocal = navLink.startTransform == null;
            if (s_SelectedPoint == 0 && navLink.GetInstanceID() == s_SelectedID)
            {
                EditorGUI.BeginChangeCheck();
                if (startIsLocal)
                    Handles.CubeHandleCap(0, worldStartPt, zup, 0.1f * startSize, Event.current.type);
                else
                    Handles.SphereHandleCap(0, worldStartPt, zup, 0.1f * startSize, Event.current.type);

                newWorldPos = Handles.PositionHandle(worldStartPt, navLink.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    if (startIsLocal)
                    {
                        Undo.RecordObject(navLink, Content.UndoMoveLinkStartPoint);
                        navLink.startPoint = toWorld.inverse.MultiplyPoint3x4(newWorldPos);
                    }
                    else
                    {
                        Undo.RecordObject(navLink.startTransform, Content.UndoMoveLinkStartObject);
                        navLink.startTransform.position = newWorldPos;
                    }
                }
            }
            else
            {
                if (Handles.Button(worldStartPt, zup, 0.1f * startSize, 0.1f * startSize,
                        startIsLocal ? Handles.CubeHandleCap : Handles.SphereHandleCap))
                {
                    s_SelectedPoint = 0;
                    s_SelectedID = navLink.GetInstanceID();
                }
            }

            var endIsLocal = navLink.endTransform == null;
            if (s_SelectedPoint == 1 && navLink.GetInstanceID() == s_SelectedID)
            {
                EditorGUI.BeginChangeCheck();
                if (endIsLocal)
                    Handles.CubeHandleCap(0, worldEndPt, zup, 0.1f * endSize, Event.current.type);
                else
                    Handles.SphereHandleCap(0, worldEndPt, zup, 0.1f * endSize, Event.current.type);

                newWorldPos = Handles.PositionHandle(worldEndPt, navLink.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    if (endIsLocal)
                    {
                        Undo.RecordObject(navLink, Content.UndoMoveLinkEndPoint);
                        navLink.endPoint = toWorld.inverse.MultiplyPoint3x4(newWorldPos);
                    }
                    else
                    {
                        Undo.RecordObject(navLink.endTransform, Content.UndoMoveLinkEndObject);
                        navLink.endTransform.position = newWorldPos;
                    }
                }
            }
            else
            {
                if (Handles.Button(worldEndPt, zup, 0.1f * endSize, 0.1f * endSize,
                        endIsLocal ? Handles.CubeHandleCap : Handles.SphereHandleCap))
                {
                    s_SelectedPoint = 1;
                    s_SelectedID = navLink.GetInstanceID();
                }
            }

            EditorGUI.BeginChangeCheck();
            newWorldPos = Handles.Slider(worldMidPt + 0.5f * navLink.width * right, right, midSize * 0.03f, Handles.DotHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(navLink, Content.UndoModifyLinkWidth);
                navLink.width = Mathf.Max(0.0f, 2.0f * Vector3.Dot(right, (newWorldPos - worldMidPt)));
            }

            EditorGUI.BeginChangeCheck();
            newWorldPos = Handles.Slider(worldMidPt - 0.5f * navLink.width * right, -right, midSize * 0.03f, Handles.DotHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(navLink, Content.UndoModifyLinkWidth);
                navLink.width = Mathf.Max(0.0f, 2.0f * Vector3.Dot(-right, (newWorldPos - worldMidPt)));
            }

            Handles.color = oldColor;
        }

        [MenuItem("GameObject/AI/NavMesh Link", false, 2002)]
        public static void CreateNavMeshLink(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            NavMeshComponentsGUIUtility.CreateAndSelectGameObject("NavMesh Link", parent, typeof(NavMeshLink));
        }

        internal class DeferredLinkUpdateScope : IDisposable
        {
            readonly NavMeshLink m_NavLink;
            readonly bool m_WasEnabled;

            public DeferredLinkUpdateScope(NavMeshLink link)
            {
                m_NavLink = link;
                m_WasEnabled = link.enabled;
                link.enabled = false;
            }

            public void Dispose()
            {
                m_NavLink.enabled = m_WasEnabled;
            }
        }

        static class Content
        {
            public static readonly GUIContent AgentType = EditorGUIUtility.TrTextContent("Agent Type", "Specifies the agent type that can use the link.");
            public static readonly GUIContent AreaType = EditorGUIUtility.TrTextContent("Area Type", "The area type of the NavMesh Link, which affects pathfinding costs.");
            public static readonly GUIContent CostOverrideToggle = EditorGUIUtility.TrTextContent("Cost Override", "If enabled, the value below will be used instead of the area cost defined in the Navigation window.");
            public static readonly GUIContent CostModifier = EditorGUIUtility.TrTextContent(" ", "If Cost Override is enabled, the link uses this cost instead of the area cost defined in the Navigation window.");
            public static readonly GUIContent Positions = EditorGUIUtility.TrTextContent("Positions", "Configure the ends of the link, each specified either as a Transform's position or as an unscaled offset in the space of this GameObject.");
            public static readonly GUIContent StartTransform = EditorGUIUtility.TrTextContent("Start Transform", "Transform whose world position specifies the link start point.");
            public static readonly GUIContent StartPoint = EditorGUIUtility.TrTextContent("Start Point", "A local position where the link starts. Used only if Start Transform does not reference any object.");
            public static readonly GUIContent EndTransform = EditorGUIUtility.TrTextContent("End Transform", "Transform whose world position specifies the link end point.");
            public static readonly GUIContent EndPoint = EditorGUIUtility.TrTextContent("End Point", "A local position where the link ends. Used only if End Transform does not reference any object.");
            public static readonly GUIContent ReverseDirectionButton = EditorGUIUtility.TrTextContent("Swap", "Reverse the direction of the link by swapping the start and end.");
            public static readonly GUIContent ReCenterButton = EditorGUIUtility.TrTextContent("Re-Center Origin", "Place this GameObject at the middle point between the start and end of this link, and rotate it to point forward from the link start towards the end.");
            public static readonly GUIContent Width = EditorGUIUtility.TrTextContent("Width", "World-space width of the segments making up the ends of the link.");
            public static readonly GUIContent AutoUpdatePositions = EditorGUIUtility.TrTextContent("Auto Update Positions", "If enabled, the link will automatically update when the Start Transform, End Transform, or this GameObject's Transform changes.");
            public static readonly GUIContent Bidirectional = EditorGUIUtility.TrTextContent("Bidirectional", "If enabled, agents can traverse the link in both directions.");
            public static readonly GUIContent Activated = EditorGUIUtility.TrTextContent("Activated", "If enabled, allows the agents to traverse the link.");
            public static readonly string UndoReCenterOrigin = L10n.Tr("Re-Center NavMesh Link origin");
            public static readonly string UndoReverseDirection = L10n.Tr("Swap NavMesh Link start and end");
            public static readonly string UndoMoveLinkStartPoint = L10n.Tr("Move NavMesh Link start point");
            public static readonly string UndoMoveLinkStartObject = L10n.Tr("Move NavMesh Link start object");
            public static readonly string UndoMoveLinkEndPoint = L10n.Tr("Move NavMesh Link end point");
            public static readonly string UndoMoveLinkEndObject = L10n.Tr("Move NavMesh Link end object");
            public static readonly string UndoModifyLinkWidth = L10n.Tr("Modify NavMesh Link width");
        }
    }
}

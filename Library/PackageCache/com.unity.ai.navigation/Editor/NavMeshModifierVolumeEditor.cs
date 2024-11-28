using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.AI.Navigation.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshModifierVolume))]
    class NavMeshModifierVolumeEditor : UnityEditor.Editor
    {
        SerializedProperty m_AffectedAgents;
        SerializedProperty m_Area;
        SerializedProperty m_Center;
        SerializedProperty m_Size;

        static Color s_HandleColor = new Color(187f, 138f, 240f, 210f) / 255;
        static Color s_HandleColorDisabled = new Color(187f * 0.75f, 138f * 0.75f, 240f * 0.75f, 100f) / 255;

        BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        bool editingCollider
        {
            get { return EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner(this); }
        }

        void OnEnable()
        {
            m_AffectedAgents = serializedObject.FindProperty("m_AffectedAgents");
            m_Area = serializedObject.FindProperty("m_Area");
            m_Center = serializedObject.FindProperty("m_Center");
            m_Size = serializedObject.FindProperty("m_Size");
        }

        Bounds GetBounds()
        {
            var navModifier = (NavMeshModifierVolume)target;
            return new Bounds(navModifier.transform.position, navModifier.size);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditMode.DoEditModeInspectorModeButton(EditMode.SceneViewEditMode.Collider, "Edit Volume",
                EditorGUIUtility.IconContent("EditCollider"), GetBounds, this);

            EditorGUILayout.PropertyField(m_Size, Content.Size);
            EditorGUILayout.PropertyField(m_Center, Content.Center);

            NavMeshComponentsGUIUtility.AreaPopup(Content.Area, m_Area);
            NavMeshComponentsGUIUtility.AgentMaskPopup(Content.AffectedAgents, m_AffectedAgents);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Active)]
        static void RenderBoxGizmo(NavMeshModifierVolume navModifier, GizmoType gizmoType)
        {
            var color = navModifier.enabled ? s_HandleColor : s_HandleColorDisabled;
            var colorTrans = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, color.a * 0.15f);

            var oldColor = Gizmos.color;
            var oldMatrix = Gizmos.matrix;

            Gizmos.matrix = navModifier.transform.localToWorldMatrix;

            Gizmos.color = colorTrans;
            Gizmos.DrawCube(navModifier.center, navModifier.size);

            Gizmos.color = color;
            Gizmos.DrawWireCube(navModifier.center, navModifier.size);

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navModifier.transform.position, "NavMeshModifierVolume Icon", true);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        static void RenderBoxGizmoNotSelected(NavMeshModifierVolume navModifier, GizmoType gizmoType)
        {
            var color = navModifier.enabled ? s_HandleColor : s_HandleColorDisabled;
            var oldColor = Gizmos.color;
            var oldMatrix = Gizmos.matrix;

            Gizmos.matrix = navModifier.transform.localToWorldMatrix;

            Gizmos.color = color;
            Gizmos.DrawWireCube(navModifier.center, navModifier.size);

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navModifier.transform.position, "NavMeshModifierVolume Icon", true);
        }

        void OnSceneGUI()
        {
            if (!editingCollider)
                return;

            var vol = (NavMeshModifierVolume)target;
            var color = vol.enabled ? s_HandleColor : s_HandleColorDisabled;
            using (new Handles.DrawingScope(color, vol.transform.localToWorldMatrix))
            {
                m_BoundsHandle.center = vol.center;
                m_BoundsHandle.size = vol.size;

                EditorGUI.BeginChangeCheck();
                m_BoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(vol, Content.UndoModifyVolume);
                    Vector3 center = m_BoundsHandle.center;
                    Vector3 size = m_BoundsHandle.size;
                    vol.center = center;
                    vol.size = size;
                    EditorUtility.SetDirty(target);
                }
            }
        }

        [MenuItem("GameObject/AI/NavMesh Modifier Volume", false, 2001)]
        public static void CreateNavMeshModifierVolume(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            NavMeshComponentsGUIUtility.CreateAndSelectGameObject("NavMesh Modifier Volume", parent, typeof(NavMeshModifierVolume));
        }

        static class Content
        {
            public static readonly GUIContent Size = EditorGUIUtility.TrTextContent("Size", "Dimensions of the NavMesh Modifier Volume.");
            public static readonly GUIContent Center = EditorGUIUtility.TrTextContent("Center", "The center of the NavMesh Modifier Volume relative to the GameObject center.");
            public static readonly GUIContent Area = EditorGUIUtility.TrTextContent("Area Type", "Describes the area type to which the NavMesh Modifier Volume applies.");
            public static readonly GUIContent AffectedAgents = EditorGUIUtility.TrTextContent("Affected Agents", "A selection of agent types that the NavMesh Modifier Volume affects.");
            public static readonly string UndoModifyVolume = L10n.Tr("Modify NavMesh Modifier Volume");
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace Unity.AI.Navigation.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshModifier))]
    class NavMeshModifierEditor : UnityEditor.Editor
    {
        SerializedProperty m_AffectedAgents;
        SerializedProperty m_IgnoreFromBuild;
        SerializedProperty m_OverrideArea;
        SerializedProperty m_Area;
        SerializedProperty m_ApplyToChildren;
        SerializedProperty m_OverrideGenerateLinks;
        SerializedProperty m_GenerateLinks;

        void OnEnable()
        {
            m_AffectedAgents = serializedObject.FindProperty("m_AffectedAgents");
            m_IgnoreFromBuild = serializedObject.FindProperty("m_IgnoreFromBuild");
            m_OverrideArea = serializedObject.FindProperty("m_OverrideArea");
            m_Area = serializedObject.FindProperty("m_Area");
            m_ApplyToChildren = serializedObject.FindProperty("m_ApplyToChildren");
            m_OverrideGenerateLinks = serializedObject.FindProperty("m_OverrideGenerateLinks");
            m_GenerateLinks = serializedObject.FindProperty("m_GenerateLinks");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int mode = m_IgnoreFromBuild.boolValue ? 1 : 0;

            mode = EditorGUILayout.Popup(Content.Mode, mode, Content.ModeChoices);

            m_IgnoreFromBuild.boolValue = mode == 1;

            NavMeshComponentsGUIUtility.AgentMaskPopup(Content.AffectedAgents, m_AffectedAgents);

            EditorGUILayout.PropertyField(m_ApplyToChildren, Content.ApplyToChildren);

            if (!m_IgnoreFromBuild.boolValue)
            {
                EditorGUILayout.PropertyField(m_OverrideArea, Content.OverrideArea);
                if (m_OverrideArea.boolValue)
                {
                    EditorGUI.indentLevel++;
                    NavMeshComponentsGUIUtility.AreaPopup(Content.AreaType, m_Area);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(m_OverrideGenerateLinks, Content.OverrideGenerateLinks);
                if (m_OverrideGenerateLinks.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_GenerateLinks, Content.GenerateLinks);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        static class Content
        {
            public static readonly GUIContent Mode = EditorGUIUtility.TrTextContent("Mode", "Specifies whether to consider or ignore the affected GameObject(s).");
            public static readonly string[] ModeChoices = { L10n.Tr("Add or Modify Object"), L10n.Tr("Remove Object") };
            public static readonly GUIContent AffectedAgents = EditorGUIUtility.TrTextContent("Affected Agents", "Specifies which agents the NavMesh Modifier affects.");
            public static readonly GUIContent ApplyToChildren = EditorGUIUtility.TrTextContent("Apply To Children", "If enabled, applies the configuration to the child hierarchy of the GameObject until another NavMesh Modifier component is encountered.");
            public static readonly GUIContent OverrideArea = EditorGUIUtility.TrTextContent("Override Area", "If enabled, the area type of the NavMeshModifier will be overridden by the area type selected below.");
            public static readonly GUIContent AreaType = EditorGUIUtility.TrTextContent("Area Type", "The area type of the NavMeshModifier.");
            public static readonly GUIContent OverrideGenerateLinks = EditorGUIUtility.TrTextContent("Override Generate Links", "If enabled, forces the NavMesh bake process to either include or ignore the affected GameObject(s) when you generate links.");
            public static readonly GUIContent GenerateLinks = EditorGUIUtility.TrTextContent("Generate Links", "If enabled, specifies whether or not to include the affected GameObject(s) when you generate links.");
        }
    }
}

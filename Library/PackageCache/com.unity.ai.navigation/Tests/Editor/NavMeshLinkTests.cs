using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.TestTools;

namespace Unity.AI.Navigation.Editor.Tests
{
    [TestFixture]
    internal class NavMeshLinkTests : DomainReloadTestBase
    {
        const string k_TrackedListFieldName = "s_Tracked";

        [SerializeField] NavMeshLink m_Link;

        [UnityTest]
        public IEnumerator TrackedList_AddOneItemToListInEditMode_TrackedListSetToZeroInPlayMode([Values(EnterPlayModeOptions.DisableDomainReload, EnterPlayModeOptions.None)] EnterPlayModeOptions option)
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = option;

            var listVar = GetTrackedList();
            Assume.That(listVar, Is.Not.Null);
            Assume.That(listVar.Count, Is.Zero);

            listVar.Add(null);
            Assume.That(listVar.Count, Is.Not.Zero);

            yield return new EnterPlayMode();
            listVar = GetTrackedList();
            Assert.That(listVar.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator TrackedList_CreateAutoUpdatedNavMeshLinkInEditMode_NavMeshLinkRemainsInTrackedListInPlayMode([Values(EnterPlayModeOptions.DisableDomainReload, EnterPlayModeOptions.None)] EnterPlayModeOptions option)
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = option;

            var trackedList = GetTrackedList();
            Assume.That(trackedList, Is.Not.Null);
            Assume.That(trackedList.Count, Is.Zero);

            // Create a link during edit mode.
            // Setting it up so that it gets added to the tracked list.
            m_TestGo = new GameObject("TestObj", typeof(NavMeshLink));
            m_Link = m_TestGo.GetComponent<NavMeshLink>();
            m_Link.autoUpdate = true;

            trackedList = GetTrackedList();
            Assume.That(trackedList, Is.Not.Null);
            Assume.That(trackedList.Count, Is.EqualTo(1));
            Assume.That(trackedList[0], Is.EqualTo(m_Link));

            yield return new EnterPlayMode();

            trackedList = GetTrackedList();
            Assert.That(trackedList.Count, Is.EqualTo(1));
            Assert.That(trackedList[0], Is.EqualTo(m_Link));
        }

        static List<NavMeshLink> GetTrackedList()
        {
            var field = typeof(NavMeshLink).GetField(k_TrackedListFieldName, BindingFlags.Static | BindingFlags.NonPublic);
            Assume.That(field, Is.Not.Null, $"Cannot find field '{k_TrackedListFieldName}' in class {nameof(NavMeshLink)}.");
            return field?.GetValue(null) as List<NavMeshLink>;
        }

        [TestCase(0, 0, true, TestName = "CostModifier at 0 should override cost and return 0.")]
        [TestCase(1, 1, true, TestName = "CostModifier at 1 should override cost and return 1.")]
        [TestCase(-1, 1, false, TestName = "CostModifier at -1 should not override cost and return 1.")]
        [TestCase(42, 42, true, TestName = "CostModifier at 42 should override cost and return 42.")]
        [TestCase(-42, 42, false, TestName = "CostModifier at -42 should not override cost and return 42.")]
        public void CostModifier_SetValue_SerializedPropertyIsTheSame(int costModifier, float expectedCostValue, bool expectedOverrideValue)
        {
            m_TestGo = new GameObject("TestObj", typeof(NavMeshLink));
            m_Link = m_TestGo.GetComponent<NavMeshLink>();
            m_Link.costModifier = costModifier;

            var serializedObject = new SerializedObject(m_Link);
            var costModifierProperty = serializedObject.FindProperty("m_CostModifier");
            var isOverridingCostProperty = serializedObject.FindProperty("m_IsOverridingCost");

            Assume.That(costModifierProperty, Is.Not.Null, "Cannot find property 'm_CostModifier' in NavMeshLink.");
            Assume.That(isOverridingCostProperty, Is.Not.Null, "Cannot find property 'm_IsOverridingCost' in NavMeshLink.");

            Assert.That(costModifierProperty.floatValue, Is.EqualTo(expectedCostValue).Using(FloatComparer.s_ComparerWithDefaultTolerance));
            Assert.That(isOverridingCostProperty.boolValue, Is.EqualTo(expectedOverrideValue));
        }

        [Test]
        public void CostModifier_Reset_CostModifierRunsThroughUpgrader()
        {
            m_TestGo = new GameObject("TestObj", typeof(NavMeshLink));
            m_Link = m_TestGo.GetComponent<NavMeshLink>();
            m_Link.costModifier = 42;

            var serializedObject = new SerializedObject(m_Link);
            var costModifierProperty = serializedObject.FindProperty("m_CostModifier");
            var isOverridingCostProperty = serializedObject.FindProperty("m_IsOverridingCost");

            Assume.That(costModifierProperty.floatValue, Is.EqualTo(42f), "Cost modifier is not 42.");
            Assume.That(isOverridingCostProperty.boolValue, Is.True, "Is overriding cost is false.");

            Unsupported.SmartReset(m_Link);
            serializedObject.Update();

            Assume.That(costModifierProperty, Is.Not.Null, "Cannot find property 'm_CostModifier' in NavMeshLink.");
            Assume.That(isOverridingCostProperty, Is.Not.Null, "Cannot find property 'm_IsOverridingCost' in NavMeshLink.");

            Assert.That(costModifierProperty.floatValue, Is.EqualTo(1f), "Cost modifier is not 1.");
            Assert.That(isOverridingCostProperty.boolValue, Is.False, "Is overriding cost is true.");
        }

        [Test]
        public void NavMeshLink_Reset_NavMeshLinkHasDefaultValues()
        {
            m_TestGo = new GameObject("TestObj", typeof(NavMeshLink));
            m_Link = m_TestGo.GetComponent<NavMeshLink>();
            m_Link.costModifier = 42;

            Unsupported.SmartReset(m_Link);

            AssertNavMeshLinkHasDefaultValues(m_Link);
        }

        static void AssertNavMeshLinkHasDefaultValues(NavMeshLink link)
        {
            var serializedObject = new SerializedObject(link);
            serializedObject.Update();

            var serializedVersionProperty = serializedObject.FindProperty("m_SerializedVersion");
            var agentTypeIdProperty = serializedObject.FindProperty("m_AgentTypeID");
            var startPointProperty = serializedObject.FindProperty("m_StartPoint");
            var endPointProperty = serializedObject.FindProperty("m_EndPoint");
            var startTransformProperty = serializedObject.FindProperty("m_StartTransform");
            var endTransformProperty = serializedObject.FindProperty("m_EndTransform");
            var activatedProperty = serializedObject.FindProperty("m_Activated");
            var widthProperty = serializedObject.FindProperty("m_Width");
            var costModifierProperty = serializedObject.FindProperty("m_CostModifier");
            var isOverridingCostProperty = serializedObject.FindProperty("m_IsOverridingCost");
            var bidirectionalProperty = serializedObject.FindProperty("m_Bidirectional");
            var autoUpdatePositionProperty = serializedObject.FindProperty("m_AutoUpdatePosition");
            var areaProperty = serializedObject.FindProperty("m_Area");

            Assume.That(serializedVersionProperty, Is.Not.Null, "Cannot find property 'm_SerializedVersion' in NavMeshLink.");
            Assume.That(agentTypeIdProperty, Is.Not.Null, "Cannot find property 'm_AgentTypeID' in NavMeshLink.");
            Assume.That(startPointProperty, Is.Not.Null, "Cannot find property 'm_StartPoint' in NavMeshLink.");
            Assume.That(endPointProperty, Is.Not.Null, "Cannot find property 'm_EndPoint' in NavMeshLink.");
            Assume.That(startTransformProperty, Is.Not.Null, "Cannot find property 'm_StartTransform' in NavMeshLink.");
            Assume.That(endTransformProperty, Is.Not.Null, "Cannot find property 'm_EndTransform' in NavMeshLink.");
            Assume.That(activatedProperty, Is.Not.Null, "Cannot find property 'm_Activated' in NavMeshLink.");
            Assume.That(widthProperty, Is.Not.Null, "Cannot find property 'm_Width' in NavMeshLink.");
            Assume.That(costModifierProperty, Is.Not.Null, "Cannot find property 'm_CostModifier' in NavMeshLink.");
            Assume.That(isOverridingCostProperty, Is.Not.Null, "Cannot find property 'm_IsOverridingCost' in NavMeshLink.");
            Assume.That(bidirectionalProperty, Is.Not.Null, "Cannot find property 'm_Bidirectional' in NavMeshLink.");
            Assume.That(autoUpdatePositionProperty, Is.Not.Null, "Cannot find property 'm_AutoUpdatePosition' in NavMeshLink.");
            Assume.That(areaProperty, Is.Not.Null, "Cannot find property 'm_Area' in NavMeshLink.");

            Assert.That(serializedVersionProperty.intValue, Is.EqualTo(1), "Serialized version is not 1.");
            Assert.That(agentTypeIdProperty.intValue, Is.EqualTo(0), "Agent type ID is not 0.");
            Assert.That(startPointProperty.vector3Value, Is.EqualTo(new Vector3(0f, 0f, -2.50f)), "Start point is not at (0, 0, -2.50).");
            Assert.That(endPointProperty.vector3Value, Is.EqualTo(new Vector3(0f, 0f, 2.50f)), "End point is not at (0, 0, 2.50).");
            Assert.That(startTransformProperty.objectReferenceValue, Is.Null, "Start transform is not null.");
            Assert.That(endTransformProperty.objectReferenceValue, Is.Null, "End transform is not null.");
            Assert.That(activatedProperty.boolValue, Is.True, "Link is not activated.");
            Assert.That(widthProperty.floatValue, Is.Zero, "Width is not 0.");
            Assert.That(bidirectionalProperty.boolValue, Is.True, "Link is not bidirectional.");
            Assert.That(autoUpdatePositionProperty.boolValue, Is.False, "Auto update position is true.");
            Assert.That(areaProperty.intValue, Is.EqualTo(0), "Area is not 0.");

            Assert.That(costModifierProperty.floatValue, Is.EqualTo(1f), "Cost modifier is not 1.");
            Assert.That(isOverridingCostProperty.boolValue, Is.False, "Is overriding cost is true.");
        }
    }
}

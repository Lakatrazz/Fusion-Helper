using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AI.Navigation.Editor.Tests
{
    [TestFixture]
    public class NavMeshLinkInspectorTests
    {
        GameObject m_TestGameObject;
        InspectorWindowWrapper m_InspectorWindowWrapper;

        [SetUp]
        public void SetUp()
        {
            m_TestGameObject = new GameObject("Test GameObject", typeof(NavMeshLink));
            m_InspectorWindowWrapper = new InspectorWindowWrapper();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_TestGameObject);
            m_InspectorWindowWrapper?.Close();
        }

        [UnityTest]
        public IEnumerator ShowNavMeshLinkInInspector_NoErrorsOrWarnings()
        {
            Selection.objects = new Object[] { m_TestGameObject };
            yield return WaitAmountOfMs(100);

            m_InspectorWindowWrapper.Focus();
            m_InspectorWindowWrapper.RepaintImmediately();
            yield return WaitAmountOfMs(100);

            // Validate that the inspector is showing the NavMeshLink component
            var rootVisualElement = m_InspectorWindowWrapper.GetRootVisualElement();
            Assume.That(rootVisualElement, Is.Not.Null, "Root visual element not found");
            var headerElement = rootVisualElement.Q<IMGUIContainer>("NavMesh LinkHeader");
            Assume.That(headerElement, Is.Not.Null, "NavMesh Link header element not found");

            LogAssert.NoUnexpectedReceived();
        }

        static IEnumerator WaitAmountOfMs(int ms)
        {
            var startTime = GetTimeSinceStartupMs();
            while (GetTimeSinceStartupMs() < startTime + ms)
                yield return null;
        }

        static long GetTimeSinceStartupMs() => (long)(Time.realtimeSinceStartup * 1000f);
    }
}

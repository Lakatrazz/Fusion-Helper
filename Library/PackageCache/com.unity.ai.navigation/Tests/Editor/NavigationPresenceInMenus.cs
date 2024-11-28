#if UNITY_EDITOR || UNITY_STANDALONE

using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Unity.AI.Navigation.Editor.Tests
{
    [TestFixture]
    [Description("Verifies that the desired Navigation editor menus are accessible with the package.")]
    class NavigationPresenceInMenus
    {
        GameObject m_ComponentsReceiver;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Create an empty game object and select it in order for components menus to be available
            m_ComponentsReceiver = new GameObject("ComponentsReceiver");
            Selection.activeObject = m_ComponentsReceiver;
        }

        static IEnumerable<string> NavigationMenuItemProvider()
        {
            yield return "Component/Navigation/Nav Mesh Agent";
            yield return "Component/Navigation/Nav Mesh Obstacle";
            yield return "Component/Navigation/NavMesh Surface";
            yield return "Component/Navigation/NavMesh Modifier Volume";
            yield return "Component/Navigation/NavMesh Modifier";
            yield return "Component/Navigation/NavMesh Link";
            yield return "Window/AI/Navigation";
        }

        [Test]
        [TestCaseSource(nameof(NavigationMenuItemProvider))]
        public void MenuIsEnabled(string menuPath)
        {
            var menuEnabled = Menu.GetEnabled(menuPath);
            Assert.That(menuEnabled, Is.True, $"Navigation component menu '{menuPath}' should be available");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Object.DestroyImmediate(m_ComponentsReceiver);
        }
    }
}
#endif

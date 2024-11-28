#if UNITY_EDITOR || UNITY_STANDALONE
//#define KEEP_ARTIFACTS_FOR_INSPECTION

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Unity.AI.Navigation.Editor.Tests
{
    class NavMeshLinkUpgradeInPrefabTests
    {
        // Adjust these tests whenever you change the content of the prefab
        const string k_PrefabName = "TestBundleWithAllLinks.prefab";
        const string k_ParentFolder = "Assets";
        const string k_TempFolder = "TempLinkUpgrade";
        static readonly string k_TempFolderPath = Path.Combine(k_ParentFolder, k_TempFolder);
        static readonly string k_TestPrefabPath = Path.Combine(k_TempFolderPath, k_PrefabName);
        static readonly string k_PrebuiltPrefabPath = Path.Combine("Packages", "com.unity.ai.navigation", "Tests", "PrebuiltAssets", k_PrefabName + "_");

        [SerializeField]
        string m_PreviousScenePath;

        [SerializeField]
        GameObject m_LinkPrefab;
        GameObject m_PrefabInstance;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            AssetDatabase.DeleteAsset(k_TempFolderPath);
            var folderGUID = AssetDatabase.CreateFolder(k_ParentFolder, k_TempFolder);
            Assume.That(folderGUID, Is.Not.Empty);

            AssetDatabase.CopyAsset(k_PrebuiltPrefabPath, k_TestPrefabPath);
            AssetDatabase.Refresh();

            m_LinkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_TestPrefabPath);

            m_PreviousScenePath = SceneManager.GetActiveScene().path;
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.DestroyImmediate(m_PrefabInstance);

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (string.IsNullOrEmpty(m_PreviousScenePath))
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            else
                EditorSceneManager.OpenScene(m_PreviousScenePath);

#if !KEEP_ARTIFACTS_FOR_INSPECTION
            AssetDatabase.DeleteAsset(k_TempFolderPath);
#endif
        }

        [Test]
        public void PrefabLinkVersion2_0_0_Instantiated_WarnsAboutOutdatedFormat()
        {
            ForgetPastWarnings();

            LogAssert.Expect(LogType.Warning, new Regex("A NavMesh Link component has an outdated format..*"));

            m_PrefabInstance = TestUtility.InstantiatePrefab(m_LinkPrefab, "Links Prefab Instance");
            Assume.That(m_PrefabInstance, Is.Not.Null);

#if KEEP_ARTIFACTS_FOR_INSPECTION
            var sceneWithPrefab = Path.Combine(k_TempFolderPath, "SceneWithPrefab1.unity");
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), sceneWithPrefab);
#endif
        }

        [Test]
        public void PrefabLinkVersion2_0_0_WhenInstantiated_KeepsReferencesToOtherObjects()
        {
            m_PrefabInstance = TestUtility.InstantiatePrefab(m_LinkPrefab, "Links Prefab Instance");

#if KEEP_ARTIFACTS_FOR_INSPECTION
            var sceneWithPrefab = Path.Combine(k_TempFolderPath, "SceneWithPrefab2.unity");
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), sceneWithPrefab);
#endif
            var scaledParent = m_PrefabInstance.transform.Find("Scaled_Links_Root");
            Assume.That(scaledParent, Is.Not.Null, "There should be a Scaled_Links_Root object.");
            var objectWithLinks = scaledParent.transform.Find("Multiple NavMesh Links");
            Assume.That(objectWithLinks, Is.Not.Null, "There should be a Multiple NavMesh Links object.");
            var links = objectWithLinks.GetComponents<NavMeshLink>();
            Assume.That(links.Length, Is.EqualTo(4));

            foreach (var navMeshLink in links)
            {
                // Differentiate links by width
                if (Math.Abs(navMeshLink.width - 1f) < 0.1f)
                {
                    Assert.That(navMeshLink.startTransform, Is.Null,
                        "Start Transform self-reference should have been removed in first link.");
                    Assert.That(navMeshLink.endTransform, Is.Null,
                        "End Transform self-reference should have been removed in first link.");
                }
                else if (Math.Abs(navMeshLink.width - 2f) < 0.1f)
                {
                    Assert.That(navMeshLink.startTransform.name, Is.EqualTo("Bundle Plane 2"),
                        "Start Transform should reference Bundle Plane 2 in second link.");
                    Assert.That(navMeshLink.endTransform, Is.SameAs(navMeshLink.startTransform),
                        "Start Transform and End Transform should reference the same object in second link.");
                }
            }
        }

        [UnityTest]
        [Explicit("Entering playmode is rather slow and the situation being tested happens rarely")]
        public IEnumerator PrefabLinkVersion2_0_0_WhenInstantiatedInPlaymode_WarnsAboutOutdatedReferences()
        {
            yield return new EnterPlayMode();

            LogAssert.Expect(LogType.Warning, new Regex(
                "The NavMesh Link component does not reference the intended transforms.*"));

            m_PrefabInstance = TestUtility.InstantiatePrefab(m_LinkPrefab, "Links Prefab Instance In Playmode");

            var scaledObjectWithLinks = GameObject.Find("Multiple NavMesh Links");
            Assume.That(scaledObjectWithLinks, Is.Not.Null);
            var linksFromScaledObject = scaledObjectWithLinks.GetComponents<NavMeshLink>();
            Assume.That(linksFromScaledObject.Length, Is.EqualTo(4));

            var unscaledObjectWithLinks = GameObject.Find("Unscaled Links");
            Assume.That(unscaledObjectWithLinks, Is.Not.Null);
            var linksFromUnscaledObject = unscaledObjectWithLinks.GetComponents<NavMeshLink>();
            Assume.That(linksFromUnscaledObject.Length, Is.EqualTo(3));

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();
        }

        static void ForgetPastWarnings()
        {
            var lastWarnedPrefab = typeof(NavMeshLink).GetField("s_LastWarnedPrefab",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assume.That(lastWarnedPrefab, Is.Not.Null,
                "Correct the test script if NavMeshLink.s_LastWarnedPrefab has been renamed or removed.");
            lastWarnedPrefab?.SetValue(null, null);
        }
    }
}
#endif

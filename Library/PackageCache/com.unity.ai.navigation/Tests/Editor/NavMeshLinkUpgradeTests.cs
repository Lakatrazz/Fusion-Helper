#if UNITY_EDITOR || UNITY_STANDALONE
//#define KEEP_ARTIFACTS_FOR_INSPECTION

using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Unity.AI.Navigation.Editor.Tests
{
    class NavMeshLinkUpgradeTests
    {
        // Adjust these tests whenever you change the content of the scene
        const string k_SceneName = "Test_Links_Created_With_2_0_0.unity";
        const string k_ParentFolder = "Assets";
        const string k_TempFolder = "TempLinkUpgrade";
        static readonly string k_TempFolderPath = Path.Combine(k_ParentFolder, k_TempFolder);
        static readonly string k_TestScenePath = Path.Combine(k_TempFolderPath, k_SceneName);
        static readonly string k_PrebuiltScenePath = Path.Combine("Packages", "com.unity.ai.navigation", "Tests", "PrebuiltAssets", k_SceneName);

        string m_PreviousScenePath;
        bool m_DelayCallHappened;

        GameObject m_ScaledObjectWithLinks;
        GameObject m_UnscaledObjectWithLinks;
        NavMeshLink[] m_LinksFromScaledObject;
        NavMeshLink[] m_LinksFromUnscaledObject;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            AssetDatabase.DeleteAsset(k_TempFolderPath);
            var folderGUID = AssetDatabase.CreateFolder(k_ParentFolder, k_TempFolder);
            Assume.That(folderGUID, Is.Not.Empty);

            AssetDatabase.CopyAsset(k_PrebuiltScenePath, k_TestScenePath);
            AssetDatabase.Refresh();

            m_PreviousScenePath = SceneManager.GetActiveScene().path;
            var testScene = EditorSceneManager.OpenScene(k_TestScenePath);
            Assume.That(testScene, Is.Not.Null, "The test scene should exist under the Assets folder.");

            m_ScaledObjectWithLinks = GameObject.Find("Multiple NavMesh Links");
            Assume.That(m_ScaledObjectWithLinks, Is.Not.Null);
            m_LinksFromScaledObject = m_ScaledObjectWithLinks.GetComponents<NavMeshLink>();
            Assume.That(m_LinksFromScaledObject.Length, Is.EqualTo(4));

            m_UnscaledObjectWithLinks = GameObject.Find("Unscaled Links");
            Assume.That(m_UnscaledObjectWithLinks, Is.Not.Null);
            m_LinksFromUnscaledObject = m_UnscaledObjectWithLinks.GetComponents<NavMeshLink>();
            Assume.That(m_LinksFromUnscaledObject.Length, Is.EqualTo(3));
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (!m_DelayCallHappened)
            {
                // Allow for the Link upgrade to be completed by the delayCall
                EditorApplication.delayCall += () => m_DelayCallHappened = true;

                yield return new WaitUntil(() => m_DelayCallHappened);
                yield return null;

                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
        }

        [Test]
        public void ScaledLinkVersion2_0_0_WhenLoaded_HasCorrectStart([NUnit.Framework.Range(0, 3)] int i)
        {
            var link = m_LinksFromScaledObject[i];
            var desc = $"(scaled[{i}]_{link.width}_{link.costModifier})";

            Assert.That(link.startTransform, Is.Null.Or.Not.SameAs(link.gameObject.transform),
                "The start transform should not point to the link GameObject itself. {0}", desc);

            if (link.startTransform != null)
            {
                var startParent = link.startTransform.parent;
                if (startParent != null)
                {
                    Assert.That(startParent.name, Is.EqualTo("Plane 2"),
                        "The start transform should be a child of Plane 2. {0}", desc);
                    Assert.That(link.startTransform.name, Contains.Substring($"Link Start {link.name}"),
                        "The new start transform should be named after the link that uses it. {0}", desc);

                    Assert.That(link.startTransform.position, Is.Not.EqualTo(Vector3.zero),
                        "The new start transform should be placed away from origin. {0}", desc);
                    Assert.That(link.startTransform.position, Is.Not.EqualTo(startParent.position),
                        "The new start transform should be placed away from the parent. {0}", desc);
                }
                else
                {
                    Assert.That(link.startPoint, Is.EqualTo(Vector3.zero).Using(new Vector3EqualityComparer(0.001f)),
                        "Start Point should be around zero when start transform keeps the original reference. {0}", desc);
                    Assert.That(link.startTransform.name, Is.EqualTo("NavMesh Surface"),
                        "The upgraded link should keep referencing the GameObject NavMesh Surface. {0}", desc);
                }
            }
        }

        [Test]
        public void ScaledLinkVersion2_0_0_WhenLoaded_HasCorrectEnd([NUnit.Framework.Range(0, 3)] int i)
        {
            var link = m_LinksFromScaledObject[i];
            var desc = $"(scaled[{i}]_{link.width}_{link.costModifier})";

            Assert.That(link.endTransform, Is.Null.Or.Not.SameAs(link.gameObject.transform),
                "The end transform should not point to the link GameObject itself. {0}", desc);

            if (link.endTransform != null)
            {
                var endParent = link.endTransform.parent;
                if (endParent != null)
                {
                    Assert.That(link.endTransform.parent.name, Is.EqualTo("Plane 2"),
                        "The end transform should be a child of Plane 2. {0}", desc);
                    Assert.That(link.endTransform.name, Contains.Substring($"Link End {link.name}"),
                        "The new end transform should be named after the link that uses it. {0}", desc);

                    Assert.That(link.endTransform.position, Is.Not.EqualTo(Vector3.zero),
                        "The new end transform should be placed away from origin. {0}", desc);
                    Assert.That(link.endTransform.position, Is.Not.EqualTo(endParent.position),
                        "The new end transform should be placed away from the parent. {0}", desc);
                }
                else
                {
                    Assert.That(link.endPoint, Is.EqualTo(Vector3.zero).Using(new Vector3EqualityComparer(0.001f)),
                        "End Point should be around zero when end transform keeps the original reference. {0}", desc);
                    Assert.That(link.endTransform.name, Is.EqualTo("Plane 2"),
                        "The upgraded link should keep referencing the GameObject Plane 2. {0}", desc);
                }
            }
        }

        [Test]
        public void UnscaledLinkVersion2_0_0_WhenLoaded_HasCorrectStart([NUnit.Framework.Range(0, 2)] int i)
        {
            var link = m_LinksFromUnscaledObject[i];
            var desc = $"(unscaled[{i}]_{link.width}_{link.costModifier})";

            Assert.That(link.startTransform, Is.Null.Or.Not.SameAs(link.gameObject.transform),
                "The start transform should not point to the link GameObject itself. {0}", desc);

            if (link.startTransform != null)
            {
                var startParent = link.startTransform.parent;
                if (startParent != null)
                {
                    Assert.That(startParent.name, Is.EqualTo("Plane 2"),
                        "The start transform should be a child of Plane 2. {0}", desc);
                    Assert.That(link.startTransform.name, Contains.Substring($"Link Start {link.name}"),
                        "The new start transform should be named after the link that uses it. {0}", desc);

                    Assert.That(link.startTransform.position, Is.Not.EqualTo(Vector3.zero),
                        "The new start transform should be placed away from origin. {0}", desc);
                    Assert.That(link.startTransform.position, Is.Not.EqualTo(startParent.position),
                        "The new start transform should be placed away from the parent. {0}", desc);
                }
                else
                {
                    Assert.That(link.startPoint, Is.EqualTo(Vector3.zero).Using(new Vector3EqualityComparer(0.001f)),
                        "Start Point should be around zero when start transform keeps the original reference. {0}", desc);
                    Assert.That(link.startTransform.name, Is.EqualTo("NavMesh Surface"),
                        "The upgraded link should keep referencing the GameObject NavMesh Surface. {0}", desc);
                }
            }
        }

        [Test]
        public void UnscaledLinkVersion2_0_0_WhenLoaded_HasCorrectEnd([NUnit.Framework.Range(0, 2)] int i)
        {
            var link = m_LinksFromUnscaledObject[i];
            var desc = $"(unscaled[{i}]_{link.width}_{link.costModifier})";

            Assert.That(link.endTransform, Is.Null.Or.Not.SameAs(link.gameObject.transform),
                "The end transform should not point to the link GameObject itself. {0}", desc);

            if (link.endTransform != null)
            {
                var endParent = link.endTransform.parent;
                if (endParent != null)
                {
                    Assert.That(link.endTransform.parent.name, Is.EqualTo("Plane 2"),
                        "The end transform should be a child of Plane 2. {0}", desc);
                    Assert.That(link.endTransform.name, Contains.Substring($"Link End {link.name}"),
                        "The new end transform should be named after the link that uses it. {0}", desc);

                    Assert.That(link.endTransform.position, Is.Not.EqualTo(Vector3.zero),
                        "The new end transform should be placed away from origin. {0}", desc);
                    Assert.That(link.endTransform.position, Is.Not.EqualTo(endParent.position),
                        "The new end transform should be placed away from the parent. {0}", desc);
                }
                else
                {
                    Assert.That(link.endPoint, Is.EqualTo(Vector3.zero).Using(new Vector3EqualityComparer(0.001f)),
                        "End Point should be around zero when end transform keeps the original reference. {0}", desc);
                    Assert.That(link.endTransform.name, Is.EqualTo("Plane 2"),
                        "The upgraded link should keep referencing the GameObject Plane 2. {0}", desc);
                }
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Assume.That(SceneManager.GetActiveScene().isDirty, Is.False,
                "Scene should not have changed after it was saved in SetUp.");

            if (string.IsNullOrEmpty(m_PreviousScenePath))
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            else
                EditorSceneManager.OpenScene(m_PreviousScenePath);

#if !KEEP_ARTIFACTS_FOR_INSPECTION
            AssetDatabase.DeleteAsset(k_TempFolderPath);
#endif
        }
    }
}
#endif

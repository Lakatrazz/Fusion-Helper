#if UNITY_EDITOR || UNITY_STANDALONE

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Unity.AI.Navigation.Tests
{
    [TestFixture]
    [PrebuildSetup("Unity.AI.Navigation.Tests." + nameof(SimpleScene2PlanesNavigationSetup))]
    [PostBuildCleanup("Unity.AI.Navigation.Tests." + nameof(SimpleScene2PlanesNavigationSetup))]
    class OffMeshLinkGetSetNavMeshArea : OffMeshLinkTestBase
    {
        int m_AreaMask;
        NavMeshLink m_Link;
        readonly string k_SceneName = "OffMeshLinkTwoPlanesScene";

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            yield return SceneManager.LoadSceneAsync(k_SceneName, LoadSceneMode.Additive);
            yield return null;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(k_SceneName));
        }

        [UnityTest]
        [UnityPlatform(exclude = new[] { RuntimePlatform.OSXServer, RuntimePlatform.WindowsServer, RuntimePlatform.LinuxServer })]
        public IEnumerator OffMeshLink_WithCustomArea_AllowsThroughOnlyPathsWithMatchingMasks()
        {
            m_Link = CreateBiDirectionalLink(true);
            yield return null;

            var defaultArea = NavMesh.GetAreaFromName("Walkable");
            var jumpArea = NavMesh.GetAreaFromName("Jump");

            Assume.That(m_Link.area, Is.EqualTo(defaultArea), "Unexpected NavMesh area for NavMeshLink");

            // Check we can pass 'default' with 'default' mask
            m_AreaMask = 1 << defaultArea;
            VerifyAreaPassing(true);

            // Change oml area to 'jump'
            m_Link.area = jumpArea;
            Assume.That(m_Link.area, Is.EqualTo(jumpArea), "Unexpected NavMesh area for NavMeshLink");

            // Check we cannot pass 'jump' with 'default' mask
            VerifyAreaPassing(false);

            // Check we can pass 'jump' with 'default' + 'jump' mask
            m_AreaMask |= 1 << jumpArea;
            VerifyAreaPassing(true);
        }

        void VerifyAreaPassing(bool expectToPass)
        {
            var path = new NavMeshPath();
            NavMesh.CalculatePath(m_PlaneStart.position, m_PlaneEnd.position, m_AreaMask, path);
            if (expectToPass)
                Assert.That(path.status, Is.EqualTo(NavMeshPathStatus.PathComplete),
                    "Expected complete path; with NavMesh area mask " + m_AreaMask + " when NavMeshLink area is " + m_Link.area);
            else
                Assert.That(path.status, Is.EqualTo(NavMeshPathStatus.PathPartial),
                    "Expected partial path; with NavMesh area mask " + m_AreaMask + " when NavMeshLink area is " + m_Link.area);
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            yield return SceneManager.UnloadSceneAsync(k_SceneName);
        }
    }
}
#endif

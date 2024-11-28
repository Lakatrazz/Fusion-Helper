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
    class AddDynamicOffMeshLinkWorks : OffMeshLinkTestBase
    {
        const string k_SceneName = "OffMeshLinkTwoPlanesScene";

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            yield return SceneManager.LoadSceneAsync(k_SceneName, LoadSceneMode.Additive);
            yield return null;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(k_SceneName));
        }

        [UnityTest]
        [UnityPlatform(exclude = new[] { RuntimePlatform.OSXServer, RuntimePlatform.WindowsServer, RuntimePlatform.LinuxServer })] //MTT-4133 Fails on Dedicated Server
        public IEnumerator OffMeshLink_WhenAddedToGameObject_BecomesUsableImmediately()
        {
            CreateBiDirectionalLink(true);
            m_Agent.SetDestination(m_PlaneEnd.position);
            yield return null;

            Assert.That(m_Agent.pathStatus, Is.EqualTo(NavMeshPathStatus.PathComplete), "DynamicOffMeshLink has not been created.");
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            yield return SceneManager.UnloadSceneAsync(k_SceneName);
        }
    }
}
#endif

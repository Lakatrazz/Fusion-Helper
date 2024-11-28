#if UNITY_EDITOR || UNITY_STANDALONE

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Unity.AI.Navigation.Tests
{
    [TestFixture]
    [Category("RegressionTest")]
    [PrebuildSetup("Unity.AI.Navigation.Tests." + nameof(SimpleScene2PlanesNavigationSetup))]
    [PostBuildCleanup("Unity.AI.Navigation.Tests." + nameof(SimpleScene2PlanesNavigationSetup))]
    class AgentCustomLinkMovement : OffMeshLinkTestBase
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
        public IEnumerator Agent_WithoutAutoTraverseOnOffMeshLink_DoesNotMoveByItself()
        {
            var link = CreateBiDirectionalLink(true);

            m_Agent.autoTraverseOffMeshLink = false;
            m_Agent.baseOffset = 1.0f;
            m_Agent.transform.position = link.startTransform.position;
            var hasDestination = m_Agent.SetDestination(link.endTransform.position);

            Assert.That(hasDestination, Is.True, "NavMeshAgent destination has not been set.");
            yield return null;

            Assert.That(m_Agent.isOnOffMeshLink, Is.True, "NavMeshAgent is currently not positioned on NavMeshLink.");

            // Move to gap between the NavMeshes connected by the NavMeshLink
            var midAirPosition = new Vector3(17.71f, 3.92f, -6.66f);
            m_Agent.transform.position = midAirPosition;
            yield return null;

            // Ensure the agent stays at this position - as 'autoTraverseOffMeshLink' is false
            Assert.That(m_Agent.transform.position, Is.EqualTo(midAirPosition).Using(new Vector3EqualityComparer(0.01f)), "NavMeshAgent should be at midAirPosition.");
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            yield return SceneManager.UnloadSceneAsync(k_SceneName);
        }
    }
}
#endif

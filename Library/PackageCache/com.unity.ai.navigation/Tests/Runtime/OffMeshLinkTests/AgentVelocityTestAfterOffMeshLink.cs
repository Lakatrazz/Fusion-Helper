#if UNITY_EDITOR || UNITY_STANDALONE

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Unity.AI.Navigation.Tests
{
    [TestFixture]
    [Category("RegressionTest")]
    [PrebuildSetup("Unity.AI.Navigation.Tests." + nameof(SimpleScene2PlanesNavigationSetup))]
    [PostBuildCleanup("Unity.AI.Navigation.Tests." + nameof(SimpleScene2PlanesNavigationSetup))]
    class AgentVelocityTestAfterOffMeshLink : OffMeshLinkTestBase
    {
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
        public IEnumerator Agent_AfterTraversingOffMeshLink_HasVelocityAlignedWithTheLink()
        {
            var link = CreateBiDirectionalLink(true);
            m_Agent.transform.position = link.startTransform.position + new Vector3(3, 0, 3);
            m_Agent.SetDestination(link.endTransform.position + new Vector3(-3, 0, 3));
            yield return null;

            while (!m_Agent.isOnOffMeshLink)
                yield return null;

            while (m_Agent.isOnOffMeshLink)
                yield return null;

            yield return 0;

            var agentMoveDir = m_Agent.velocity;
            agentMoveDir.y = 0;
            agentMoveDir = agentMoveDir.normalized;

            var linkDir = link.endTransform.position - link.startTransform.position;
            linkDir.y = 0;
            linkDir = linkDir.normalized;

            // Get the angle in degrees between the direction vectors.
            var angle = Vector3.Angle(linkDir, agentMoveDir);

            Assert.That(angle, Is.LessThan(5.0f), "Agent Velocity is not aligned with the off-mesh link.");
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            yield return SceneManager.UnloadSceneAsync(k_SceneName);
        }
    }
}
#endif

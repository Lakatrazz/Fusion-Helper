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
    [PrebuildSetup("Unity.AI.Navigation.Tests." + nameof(CurrentNextOffMeshLinkDataSetUp))]
    [PostBuildCleanup("Unity.AI.Navigation.Tests." + nameof(CurrentNextOffMeshLinkDataSetUp))]
    class CurrentNextOffMeshLinkData
    {
        const string k_SceneName = "OffMeshLinkTest";

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            yield return SceneManager.LoadSceneAsync(k_SceneName, LoadSceneMode.Additive);
            yield return null;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(k_SceneName));
        }

        [UnityTest]
        [Explicit("Unstable test")]
        public IEnumerator Agent_TraversingOffMeshLink_ReportsCorrectCurrentAndNextLink()
        {
            var agent = GameObject.Find("Agent").GetComponent<NavMeshAgent>();
            var offMeshLink = GameObject.Find("Plane1").GetComponent<NavMeshLink>();
            var target = GameObject.Find("Plane2").transform;

            Assert.That(offMeshLink, Is.Not.Null, "Didn't find Off-mesh link");
            Assert.That(agent, Is.Not.Null, "Didn't find NavMeshAgent");

            var destinationSet = agent.SetDestination(target.position);
            agent.speed *= 10;
            Assert.That(destinationSet, Is.True, "NavMeshAgent's destination position is not set");

            // Wait for path calculation
            yield return null;

            // Before link
            while (!agent.isOnOffMeshLink)
            {
                Assert.That(agent.currentOffMeshLinkData.valid, Is.False, "Before link : agent.currentOffMeshLinkData is valid");

                AssertValidOffMeshLinkData(agent.nextOffMeshLinkData, offMeshLink);
                yield return null;
            }

            // On link
            while (agent.isOnOffMeshLink)
            {
                Assert.That(agent.nextOffMeshLinkData.valid, Is.False, "On link : agent.nextOffMeshLinkData is valid");

                AssertValidOffMeshLinkData(agent.currentOffMeshLinkData, offMeshLink);
                yield return null;
            }

            // After link
            Assert.That(agent.currentOffMeshLinkData.valid, Is.False, "After link : agent.currentOffMeshLinkData is valid");
            Assert.That(agent.nextOffMeshLinkData.valid, Is.False, "After link : agent.nextOffMeshLinkData is valid");
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            yield return SceneManager.UnloadSceneAsync(k_SceneName);
        }

        static void AssertValidOffMeshLinkData(OffMeshLinkData offMeshLinkData, NavMeshLink offMeshLink)
        {
            // Double check to avoid spamming success in log-file (decreasing tests performance)
            Assert.That(offMeshLinkData.valid, Is.True, "OffMeshLinkData should be valid.");
            Assert.That(offMeshLinkData.activated, Is.True, "OffMeshLinkData should be activated.");
            Assert.That(offMeshLinkData.linkType, Is.EqualTo(OffMeshLinkType.LinkTypeManual), "OffMeshLinkData's linkType should be Manual.");
            Assert.That(offMeshLinkData.owner, Is.EqualTo(offMeshLink), "OffMeshLinkData should reference the NavMeshLink in the scene as the owner object.");
#pragma warning disable CS0618
            Assert.That(offMeshLinkData.offMeshLink, Is.Null, "OffMeshLinkData should not reference any legacy OffMeshLink in the scene.");
#pragma warning restore CS0618
        }
    }
}
#endif

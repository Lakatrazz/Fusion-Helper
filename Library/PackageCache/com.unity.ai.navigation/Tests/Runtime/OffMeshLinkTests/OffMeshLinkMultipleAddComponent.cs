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
    class OffMeshLinkMultipleAddComponent
    {
        const string k_SceneName = "OffMeshLinkTwoPlanesScene";
        GameObject m_LinkGO;

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            yield return SceneManager.LoadSceneAsync(k_SceneName, LoadSceneMode.Additive);
            yield return null;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(k_SceneName));
            m_LinkGO = new GameObject("OffMeshLinkMultipleAddComponent");
        }

        [Test]
        [UnityPlatform(exclude = new[] { RuntimePlatform.OSXServer, RuntimePlatform.WindowsServer, RuntimePlatform.LinuxServer })]
        public void OffMeshLink_WhenMultipleAddedToGameObject_AreAllUsable()
        {
            var a = GameObject.Find("plane1").GetComponent<Transform>();
            var b = GameObject.Find("plane2").GetComponent<Transform>();

            Assert.That(a, Is.Not.Null, "Plane1 is missing.");
            Assert.That(b, Is.Not.Null, "Plane2 is missing.");

            var pathAB = new NavMeshPath();
            var pathBA = new NavMeshPath();

            var foundAB = NavMesh.CalculatePath(a.position, b.position, -1, pathAB);
            var foundBA = NavMesh.CalculatePath(b.position, a.position, -1, pathBA);
            Assert.That(foundAB, Is.True, "Found unexpected path A->B.");
            Assert.That(foundBA, Is.True, "Found unexpected path B->A.");

            // Create setup where one GO has two OffMeshLinks with 'Bi Directional' set to false
            AddOneWayLink(a, b);
            AddOneWayLink(b, a);

            // Tests that path a->b and b->a are valid and have same end-points (mirrored).
            foundAB = NavMesh.CalculatePath(a.position, b.position, -1, pathAB);
            foundBA = NavMesh.CalculatePath(b.position, a.position, -1, pathBA);
            Assert.That(foundAB, Is.True, "No path from A->B");
            Assert.That(foundBA, Is.True, "No path from B->A");

            var d1 = Vector3.Distance(pathAB.corners[0], pathBA.corners[pathBA.corners.Length - 1]);
            var d2 = Vector3.Distance(pathAB.corners[pathAB.corners.Length - 1], pathBA.corners[0]);

            Assert.That(d1, Is.EqualTo(0.0f).Within(1e-5f), "Endpoint mismatch: A start -> B end.");
            Assert.That(d2, Is.EqualTo(0.0f).Within(1e-5f), "Endpoint mismatch: B start -> A end.");
        }

        void AddOneWayLink(Transform start, Transform end)
        {
            var offMeshLink = m_LinkGO.AddComponent<NavMeshLink>();
            Assert.That(offMeshLink, Is.Not.Null, "Failed to create NavMeshLink.");
            offMeshLink.bidirectional = false;
            offMeshLink.startTransform = start;
            offMeshLink.endTransform = end;

            // we modified the endpoint references above - now explicitly update positions.
            offMeshLink.UpdateLink();
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            Object.DestroyImmediate(m_LinkGO);
            yield return SceneManager.UnloadSceneAsync(k_SceneName);
        }
    }
}
#endif

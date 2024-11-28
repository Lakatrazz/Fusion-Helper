#if UNITY_EDITOR || UNITY_STANDALONE

using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace Unity.AI.Navigation.Tests
{
    class OffMeshLinkTestBase
    {
        protected Transform m_PlaneStart;
        protected Transform m_PlaneEnd;
        protected NavMeshAgent m_Agent;

        public NavMeshLink CreateBiDirectionalLink(bool autoUpdatePositions)
        {
            var planeStartGO = GameObject.Find("plane1");
            Assert.That(planeStartGO, Is.Not.Null, "Didn't find gameobject plane1");
            m_PlaneStart = planeStartGO.transform;
            var planeEndGO = GameObject.Find("plane2");
            Assert.That(planeEndGO, Is.Not.Null, "Didn't find gameobject plane2");
            m_PlaneEnd = planeEndGO.transform;
            var agentGo = GameObject.Find("Agent");
            Assert.That(agentGo, Is.Not.Null, "Didn't find gameobject Agent");
            m_Agent = agentGo.GetComponent<NavMeshAgent>();
            Assert.That(m_Agent, Is.Not.Null, "Didn't find component NavMeshAgent in gameobject Agent");

            m_Agent.speed *= 10.0f;
            m_Agent.acceleration *= 10.0f;

            var linkGO = new GameObject("link");
            var link = linkGO.AddComponent<NavMeshLink>();
            Assert.That(link, Is.Not.Null, "Unable to add NavMeshLink component.");
            link.startTransform = m_PlaneStart;
            link.endTransform = m_PlaneEnd;
            link.autoUpdate = autoUpdatePositions;
            return link;
        }
    }
}
#endif

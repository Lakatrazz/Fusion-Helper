#if UNITY_EDITOR || UNITY_STANDALONE

using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace Unity.AI.Navigation.Tests
{
    [TestFixture]
    class NavMeshSurfaceModifierTests
    {
        NavMeshSurface m_Surface;
        NavMeshModifier m_Modifier;
        NavMeshSurfaceBuildFromAwake m_BuildOnAwake;

        [SetUp]
        public void CreatePlaneWithModifier()
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            m_Surface = plane.AddComponent<NavMeshSurface>();
            m_Modifier = plane.AddComponent<NavMeshModifier>();
        }

        [TearDown]
        public void DestroyPlaneWithModifier()
        {
            Object.DestroyImmediate(m_Modifier.gameObject);
        }

        [Test]
        public void ModifierIgnoreAffectsSelf()
        {
            m_Modifier.ignoreFromBuild = true;

            m_Surface.BuildNavMesh();

            Assert.IsFalse(NavMeshSurfaceTests.HasNavMeshAtOrigin());
        }

        [Test]
        public void ModifierIgnoreAffectsChild()
        {
            m_Modifier.ignoreFromBuild = true;
            m_Modifier.GetComponent<MeshRenderer>().enabled = false;

            var childPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            childPlane.transform.SetParent(m_Modifier.transform);

            m_Surface.BuildNavMesh();

            Assert.IsFalse(NavMeshSurfaceTests.HasNavMeshAtOrigin());
            Object.DestroyImmediate(childPlane);
        }

        [Test]
        public void ModifierIgnoreDoesNotAffectSibling()
        {
            m_Modifier.ignoreFromBuild = true;
            m_Modifier.GetComponent<MeshRenderer>().enabled = false;

            var siblingPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);

            m_Surface.BuildNavMesh();

            Assert.IsTrue(NavMeshSurfaceTests.HasNavMeshAtOrigin());
            Object.DestroyImmediate(siblingPlane);
        }

        [Test]
        public void ModifierOverrideAreaAffectsSelf()
        {
            m_Modifier.area = 4;
            m_Modifier.overrideArea = true;

            m_Surface.BuildNavMesh();

            var expectedAreaMask = 1 << 4;
            Assert.IsTrue(NavMeshSurfaceTests.HasNavMeshAtOrigin(expectedAreaMask));
        }

        [Test]
        public void Modifier_WhenSurfaceBuiltOnAwake_OverridesArea()
        {
            m_Modifier.gameObject.SetActive(false);
            m_BuildOnAwake = m_Modifier.gameObject.AddComponent<NavMeshSurfaceBuildFromAwake>();
            m_BuildOnAwake.surface = m_Surface;
            m_Modifier.area = 4;
            m_Modifier.overrideArea = true;

            // Enable the components to build the NavMesh on Awake
            m_Modifier.gameObject.SetActive(true);

            NavMesh.SamplePosition(Vector3.zero, out var hit, 0.1f, NavMesh.AllAreas);
            Assume.That(hit.hit, Is.True, "There should be a NavMesh at position (0,0,0).");
            Assert.That(hit.mask, Is.EqualTo(1 << m_Modifier.area),
                "The NavMesh should have the Modifier area mask at position (0,0,0).");
        }

        [Test]
        public void ModifierOverrideAreaAffectsChild()
        {
            m_Modifier.area = 4;
            m_Modifier.overrideArea = true;
            m_Modifier.GetComponent<MeshRenderer>().enabled = false;

            var childPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            childPlane.transform.SetParent(m_Modifier.transform);

            m_Surface.BuildNavMesh();

            var expectedAreaMask = 1 << 4;
            Assert.IsTrue(NavMeshSurfaceTests.HasNavMeshAtOrigin(expectedAreaMask));
            Object.DestroyImmediate(childPlane);
        }

        [Test]
        public void ModifierOverrideAreaDoesNotAffectSibling()
        {
            m_Modifier.area = 4;
            m_Modifier.overrideArea = true;
            m_Modifier.GetComponent<MeshRenderer>().enabled = false;

            var siblingPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);

            m_Surface.BuildNavMesh();

            var expectedAreaMask = 1;
            Assert.IsTrue(NavMeshSurfaceTests.HasNavMeshAtOrigin(expectedAreaMask));
            Object.DestroyImmediate(siblingPlane);
        }
    }
}
#endif

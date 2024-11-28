using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.AI.Navigation.Editor.Tests
{
    [TestFixture]
    internal class NavMeshSurfacesTests : DomainReloadTestBase
    {
        [SerializeField] NavMeshSurface m_Surface;

        [UnityTest]
        public IEnumerator ActiveSurfaces_AddOneItemToListInEditMode_SurfaceListSetToZeroInPlayMode([Values(EnterPlayModeOptions.DisableDomainReload, EnterPlayModeOptions.None)] EnterPlayModeOptions option)
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = option;

            var activeSurfaces = NavMeshSurface.activeSurfaces;
            Assume.That(activeSurfaces, Is.Not.Null);
            Assume.That(activeSurfaces.Count, Is.Zero);

            activeSurfaces.Add(null);
            Assume.That(activeSurfaces.Count, Is.Not.Zero);

            yield return new EnterPlayMode();
            activeSurfaces = NavMeshSurface.activeSurfaces;
            Assert.That(activeSurfaces.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ActiveSurfaces_CreateSurfaceInEditMode_SurfaceRemainsInActiveSurfacesInPlayMode([Values(EnterPlayModeOptions.DisableDomainReload, EnterPlayModeOptions.None)] EnterPlayModeOptions option)
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = option;

            var activeSurfaces = NavMeshSurface.activeSurfaces;
            Assume.That(activeSurfaces, Is.Not.Null);
            Assume.That(activeSurfaces.Count, Is.Zero);

            m_TestGo = new GameObject("TestObj", typeof(NavMeshSurface));
            m_Surface = m_TestGo.GetComponent<NavMeshSurface>();

            activeSurfaces = NavMeshSurface.activeSurfaces;
            Assume.That(activeSurfaces, Is.Not.Null);
            Assume.That(activeSurfaces.Count, Is.EqualTo(1));
            Assume.That(activeSurfaces[0], Is.EqualTo(m_Surface));

            yield return new EnterPlayMode();

            activeSurfaces = NavMeshSurface.activeSurfaces;
            Assert.That(activeSurfaces.Count, Is.EqualTo(1));
            Assert.That(activeSurfaces[0], Is.EqualTo(m_Surface));
        }
    }
}

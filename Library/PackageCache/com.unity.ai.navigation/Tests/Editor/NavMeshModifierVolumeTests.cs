using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.AI.Navigation.Editor.Tests
{
    [TestFixture]
    internal class NavMeshModifierVolumeTests : DomainReloadTestBase
    {
        [SerializeField] NavMeshModifierVolume m_Modifier;

        [UnityTest]
        public IEnumerator ActiveModifiers_AddOneItemToListInEditMode_ModifierListSetToZeroInPlayMode([Values(EnterPlayModeOptions.DisableDomainReload, EnterPlayModeOptions.None)] EnterPlayModeOptions option)
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = option;

            var activeModifiers = NavMeshModifierVolume.activeModifiers;
            Assume.That(activeModifiers, Is.Not.Null);
            Assume.That(activeModifiers.Count, Is.Zero);

            activeModifiers.Add(null);
            Assume.That(activeModifiers.Count, Is.Not.Zero);

            yield return new EnterPlayMode();
            activeModifiers = NavMeshModifierVolume.activeModifiers;
            Assert.That(activeModifiers.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ActiveModifiers_CreateModifierInEditMode_ModifierRemainsInActiveModifiersInPlayMode([Values(EnterPlayModeOptions.DisableDomainReload, EnterPlayModeOptions.None)] EnterPlayModeOptions option)
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = option;

            var activeModifiers = NavMeshModifierVolume.activeModifiers;
            Assume.That(activeModifiers, Is.Not.Null);
            Assume.That(activeModifiers.Count, Is.Zero);

            m_TestGo = new GameObject("TestObj", typeof(NavMeshModifierVolume));
            m_Modifier = m_TestGo.GetComponent<NavMeshModifierVolume>();

            Assume.That(activeModifiers.Count, Is.EqualTo(1));
            Assume.That(activeModifiers[0], Is.EqualTo(m_Modifier));

            yield return new EnterPlayMode();

            activeModifiers = NavMeshModifierVolume.activeModifiers;
            Assert.That(activeModifiers.Count, Is.EqualTo(1));
            Assert.That(activeModifiers[0], Is.EqualTo(m_Modifier));
        }
    }
}

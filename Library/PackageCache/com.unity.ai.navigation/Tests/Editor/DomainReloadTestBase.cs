using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.AI.Navigation.Editor.Tests
{
    internal abstract class DomainReloadTestBase
    {
        struct OptionSet
        {
            public bool enterPlayModeOptionsEnabled;
            public EnterPlayModeOptions enterPlayModeOptions;
        }
        OptionSet m_OriginalOptions;

        [SerializeField] protected GameObject m_TestGo;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();

            if (m_TestGo != null)
            {
                Object.DestroyImmediate(m_TestGo);
                m_TestGo = null;
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (EditorApplication.isPlaying)
                return;

            m_OriginalOptions = new OptionSet()
            {
                enterPlayModeOptions = EditorSettings.enterPlayModeOptions,
                enterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled
            };
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (EditorApplication.isPlaying)
                return;

            EditorSettings.enterPlayModeOptions = m_OriginalOptions.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = m_OriginalOptions.enterPlayModeOptionsEnabled;
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

#if UNITY_EDITOR
namespace Unity.AI.Navigation.Editor.Tests.InPlaymode
{
    [TestFixture]
    [Explicit]
    [UnityPlatform(include = new[] { RuntimePlatform.LinuxEditor, RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor })]
    [Description("These tests run on demand and only in the editor playmode")]
    public class NavMeshLinkEditorTestsInPlaymode : NavMeshLinkEditorTests { }
}
#endif

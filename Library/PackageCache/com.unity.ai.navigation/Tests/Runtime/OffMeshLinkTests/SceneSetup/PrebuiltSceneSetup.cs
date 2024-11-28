using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.TestTools;

namespace Unity.AI.Navigation.Tests
{
    abstract class PrebuiltSceneSetup : IPrebuildSetup, IPostBuildCleanup
    {
        const string k_RootDir = "Assets";
        const string k_TestDir = "TmpScenes";
        string testDirectory { get; set; } = "";
        protected string pathToTestScene { get; private set; } = "";

        protected abstract string GetSceneFile();
        protected abstract void SceneSetup();

        public void Setup()
        {
#if UNITY_EDITOR
            testDirectory = Path.Combine(k_RootDir, k_TestDir);
            pathToTestScene = Path.Combine(testDirectory, GetSceneFile());

            AssetDatabase.Refresh();
            if (!AssetDatabase.IsValidFolder(testDirectory))
                testDirectory = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(k_RootDir, k_TestDir));
            AssetDatabase.Refresh();

            SceneSetup();

            var editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes)
            {
                new EditorBuildSettingsScene(pathToTestScene, true)
            };
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
#endif
        }

        public void Cleanup()
        {
#if UNITY_EDITOR
            AssetDatabase.Refresh();
            testDirectory = Path.Combine(k_RootDir, k_TestDir);
            pathToTestScene = Path.Combine(testDirectory, GetSceneFile());
            var baseSceneGuidTxt = AssetDatabase.AssetPathToGUID(pathToTestScene);

            if (AssetDatabase.IsValidFolder(testDirectory))
                AssetDatabase.DeleteAsset(testDirectory);

            if (GUID.TryParse(baseSceneGuidTxt, out var sceneGuid))
                EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(scene => scene.guid != sceneGuid).ToArray();
#endif
        }
    }
}
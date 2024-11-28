#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools.Utils;
#pragma warning disable CS0618 // UnityEditor.AI.NavMeshBuilder is necessary in this implementation

namespace Unity.AI.Navigation.Tests
{
    class SimpleScene2PlanesNavigationSetup : PrebuiltSceneSetup
    {
        protected override string GetSceneFile()
        {
            return "OffMeshLinkTwoPlanesScene.unity";
        }

        protected override void SceneSetup()
        {
#if UNITY_EDITOR
            var myScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(myScene);

            var plane1 = Utils.CreatePrimitive(PrimitiveType.Plane);
            plane1.transform.position = new Vector3(10f, 0f, 0f);
            plane1.name = "plane1";
            GameObjectUtility.SetStaticEditorFlags(plane1, StaticEditorFlags.NavigationStatic);

            var plane2 = Utils.CreatePrimitive(PrimitiveType.Plane);
            plane2.transform.position = new Vector3(25f, 0f, 0f);
            plane2.name = "plane2";
            GameObjectUtility.SetStaticEditorFlags(plane2, StaticEditorFlags.NavigationStatic);

            var capsule = Utils.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Agent";
            capsule.transform.position = new Vector3(6, 0, 0);
            capsule.AddComponent<NavMeshAgent>();

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), pathToTestScene);
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), pathToTestScene);

            EditorSceneManager.CloseScene(myScene, true);
            UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
#endif
        }
    }
}

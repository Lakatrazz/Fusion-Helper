#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Utils = UnityEngine.TestTools.Utils.Utils;
#pragma warning disable CS0618 // UnityEditor.AI.NavMeshBuilder is necessary in this implementation

namespace Unity.AI.Navigation.Tests
{
    class CurrentNextOffMeshLinkDataSetUp : PrebuiltSceneSetup
    {
        protected override string GetSceneFile()
        {
            return "OffMeshLinkTest.unity";
        }

        protected override void SceneSetup()
        {
#if UNITY_EDITOR
            var myScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(myScene);

            var plane1 = Utils.CreatePrimitive(PrimitiveType.Plane);
            GameObjectUtility.SetStaticEditorFlags(plane1, StaticEditorFlags.NavigationStatic);
            plane1.name = "Plane1";
            plane1.transform.position = Vector3.zero;

            var plane2 = Utils.CreatePrimitive(PrimitiveType.Plane);
            GameObjectUtility.SetStaticEditorFlags(plane2, StaticEditorFlags.NavigationStatic);
            plane2.name = "Plane2";
            plane2.transform.position = new Vector3(0, 0, 15);

            var offMeshLink = plane1.AddComponent<NavMeshLink>();
            offMeshLink.startTransform = plane1.transform;
            offMeshLink.endTransform = plane2.transform;

            var cube = Utils.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(0, 0, -4);
            cube.name = "Agent";
            cube.AddComponent<NavMeshAgent>();

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), pathToTestScene);
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), pathToTestScene);

            EditorSceneManager.CloseScene(myScene, true);
            UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
#endif
        }
    }
}

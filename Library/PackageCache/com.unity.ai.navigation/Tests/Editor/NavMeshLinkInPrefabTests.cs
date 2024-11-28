using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Unity.AI.Navigation.Editor.Tests
{
    class NavMeshLinkInPrefabTests
    {
        [Test]
        public void NavMeshLink_DifferentGOActiveState_PromotedToPrefab_InstanceHasNoOverrides([Values]bool isActive)
        {
            var assetPath = $"Assets/{GUID.Generate()}.prefab";
            NavMeshLink nml = null;
            try
            {
                var go = new GameObject("NavMesh Link");
                go.SetActive(isActive);
                nml = go.AddComponent<NavMeshLink>();

                PrefabUtility.SaveAsPrefabAssetAndConnect(nml.gameObject, assetPath, InteractionMode.AutomatedAction);

                var sp = new SerializedObject(nml).GetIterator();
                var overrides = new List<string>();
                while (sp.NextVisible(true))
                {
                    if (!sp.isDefaultOverride && sp.prefabOverride)
                        overrides.Add(sp.propertyPath);
                }
                Assert.That(overrides, Is.Empty, "Newly promoted prefab instance overrides one or more properties.");
            }
            finally
            {
                if (nml != null)
                    Object.DestroyImmediate(nml.gameObject);
                AssetDatabase.DeleteAsset(assetPath);
            }
        }
    }
}

using UnityEngine;

namespace Unity.AI.Navigation.Tests
{
    [DefaultExecutionOrder(-1)]
    class NavMeshSurfaceBuildFromAwake : MonoBehaviour
    {
        public NavMeshSurface surface;

        void Awake()
        {
            surface.BuildNavMesh();
        }
    }
}

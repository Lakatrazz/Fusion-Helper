using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace Unity.AI.Navigation.Tests
{
    public class NavMeshLinkTests
    {
        NavMeshLink m_NavMeshLink;
        Transform m_StartTransform;
        Transform m_EndTransform;
        GameObject m_ScaledSkewer;
        GameObject m_LinkImitator;
        GameObject m_LinkStartImitator;
        GameObject m_LinkEndImitator;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_NavMeshLink = new GameObject("NavMeshLink").AddComponent<NavMeshLink>();
            m_StartTransform = new GameObject("Start Transform").transform;
            m_EndTransform = new GameObject("End Transform").transform;
            m_ScaledSkewer = new GameObject("Unevenly Scaled Skewer");
            m_LinkImitator = new GameObject("Link Imitator");
            m_LinkStartImitator = new GameObject("Link Start Imitator");
            m_LinkEndImitator = new GameObject("Link End Imitator");

            Assume.That(k_UnevenScale / k_UnevenScale.x, Is.Not.EqualTo(Vector3.one).Using(Vector3EqualityComparer.Instance));

            m_ScaledSkewer.transform.localScale = k_UnevenScale;
            m_ScaledSkewer.transform.SetPositionAndRotation(new Vector3(2f, 1f, 1f), Quaternion.identity);
            m_LinkImitator.transform.parent = m_ScaledSkewer.transform;
            m_LinkStartImitator.transform.parent = m_LinkImitator.transform;
            m_LinkEndImitator.transform.parent = m_LinkImitator.transform;

            // To debug, add these components to imitators, only to show icons for them in the scene
            //m_LinkStartImitator.AddComponent<NavMeshModifier>().enabled = false;
            //m_LinkEndImitator.AddComponent<NavMeshModifier>().enabled = false;
            //m_ScaledSkewer.AddComponent<NavMeshModifierVolume>().enabled = false;
        }

        [SetUp]
        public void SetUp()
        {
            m_NavMeshLink.transform.parent = null;

            // Note: Adjust the expected test return values if you change the setup
            m_NavMeshLink.startPoint = Vector3.back;
            m_NavMeshLink.endPoint = Vector3.forward;
            m_StartTransform.position = Vector3.left;
            m_EndTransform.position = Vector3.right;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (m_NavMeshLink != null)
                Object.DestroyImmediate(m_NavMeshLink.gameObject);
            if (m_StartTransform != null)
                Object.DestroyImmediate(m_StartTransform.gameObject);
            if (m_EndTransform != null)
                Object.DestroyImmediate(m_EndTransform.gameObject);
            if (m_LinkStartImitator != null)
                Object.DestroyImmediate(m_LinkStartImitator);
            if (m_LinkEndImitator != null)
                Object.DestroyImmediate(m_LinkEndImitator);
            if (m_LinkImitator != null)
                Object.DestroyImmediate(m_LinkImitator);
            if (m_ScaledSkewer != null)
                Object.DestroyImmediate(m_ScaledSkewer);
        }

        static readonly Quaternion k_RotatedAroundYAxis = new(0f, 1f, 0f, 0f);
        static readonly Quaternion k_ArbitraryRotationAroundYAxis = Quaternion.Euler(0f, 25f, 0f);
        static readonly Vector3 k_UniformScale = new(2f, 2f, 2f);
        static readonly Vector3 k_UnevenScale = new(2f, 1f, 0.7f);

        static readonly TestCaseData[] k_TestCases =
        {
            new TestCaseData(Vector3.zero, Quaternion.identity, Vector3.one, false, false)
                .SetName("At origin, use start and end points")
                .Returns((Vector3.back, Vector3.forward)),
            new TestCaseData(Vector3.zero, Quaternion.identity, Vector3.one, false, true)
                .SetName("At origin, use start point and end transform")
                .Returns((Vector3.back, Vector3.right)),
            new TestCaseData(Vector3.zero, Quaternion.identity, Vector3.one, true, false)
                .SetName("At origin, use start transform and end point")
                .Returns((Vector3.left, Vector3.forward)),
            new TestCaseData(Vector3.zero, Quaternion.identity, Vector3.one, true, true)
                .SetName("At origin, use start and end transforms")
                .Returns((Vector3.left, Vector3.right)),
            new TestCaseData(Vector3.one, Quaternion.identity, Vector3.one, false, false)
                .SetName("Offset from origin, use start and end points")
                .Returns((Vector3.one + Vector3.back, Vector3.one + Vector3.forward)),
            new TestCaseData(Vector3.one, Quaternion.identity, Vector3.one, false, true)
                .SetName("Offset from origin, use start point and end transform")
                .Returns((Vector3.one + Vector3.back, Vector3.right)),
            new TestCaseData(Vector3.one, Quaternion.identity, Vector3.one, true, false)
                .SetName("Offset from origin, use start transform and end point")
                .Returns((Vector3.left, Vector3.one + Vector3.forward)),
            new TestCaseData(Vector3.one, Quaternion.identity, Vector3.one, true, true)
                .SetName("Offset from origin, use start and end transforms")
                .Returns((Vector3.left, Vector3.right)),
            new TestCaseData(Vector3.zero, k_RotatedAroundYAxis, Vector3.one, false, false)
                .SetName("Rotated at origin, use start and end points")
                .Returns((Vector3.forward, Vector3.back)),
            new TestCaseData(Vector3.zero, k_RotatedAroundYAxis, Vector3.one, false, true)
                .SetName("Rotated at origin, use start point and end transform")
                .Returns((Vector3.forward, Vector3.right)),
            new TestCaseData(Vector3.zero, k_RotatedAroundYAxis, Vector3.one, true, false)
                .SetName("Rotated at origin, use start transform and end point")
                .Returns((Vector3.left, Vector3.back)),
            new TestCaseData(Vector3.zero, k_RotatedAroundYAxis, Vector3.one, true, true)
                .SetName("Rotated at origin, use start and end transforms")
                .Returns((Vector3.left, Vector3.right)),
            new TestCaseData(Vector3.zero, Quaternion.identity, k_UniformScale, false, false)
                .SetName("Scaled at origin, use start and end points")
                .Returns((Vector3.back, Vector3.forward)),
            new TestCaseData(Vector3.zero, Quaternion.identity, k_UniformScale, false, true)
                .SetName("Scaled at origin, use start point and end transform")
                .Returns((Vector3.back, Vector3.right)),
            new TestCaseData(Vector3.zero, Quaternion.identity, k_UniformScale, true, false)
                .SetName("Scaled at origin, use start transform and end point")
                .Returns((Vector3.left, Vector3.forward)),
            new TestCaseData(Vector3.zero, Quaternion.identity, k_UniformScale, true, true)
                .SetName("Scaled at origin, use start and end transforms")
                .Returns((Vector3.left, Vector3.right)),
        };

        [TestCaseSource(nameof(k_TestCases))]
        public (Vector3 start, Vector3 end) GetWorldPositions_ReturnsExpectedResults(
            Vector3 transformPosition, Quaternion transformRotation, Vector3 transformScale,
            bool useStartTransform, bool useEndTransform
        )
        {
            m_NavMeshLink.transform.position = transformPosition;
            m_NavMeshLink.transform.rotation = transformRotation;
            m_NavMeshLink.transform.localScale = transformScale;
            m_NavMeshLink.startTransform = useStartTransform ? m_StartTransform : null;
            m_NavMeshLink.endTransform = useEndTransform ? m_EndTransform : null;

            m_NavMeshLink.GetWorldPositions(out var worldStart, out var worldEnd);
            return (start: worldStart, end: worldEnd);
        }

        // The expected values have been obtained by observing a correct result in the Editor
        static readonly TestCaseData[] k_SkewedTestCases =
        {
            new TestCaseData(Vector3.zero, Quaternion.identity, Vector3.one,
                    new Vector3(2f, 1f, 0f), new Vector3(2f, 1f, 2f))
                .SetName("At parent origin"),

            new TestCaseData(Vector3.zero, k_ArbitraryRotationAroundYAxis, Vector3.one,
                    new Vector3(1.577382f, 1f, 0.09369224f), new Vector3(2.422618f, 1f, 1.906308f))
                .SetName("Rotated"),

            new TestCaseData(Vector3.one, k_ArbitraryRotationAroundYAxis, Vector3.one,
                    new Vector3(3.577382f, 2f, 0.7936923f), new Vector3(4.422618f, 2f, 2.606308f))
                .SetName("Offset from parent"),

            new TestCaseData(Vector3.zero, Quaternion.identity, k_UniformScale,
                    new Vector3(2f, 1f, 0f), new Vector3(2f, 1f, 2f))
                .SetName("Scaled"),

            new TestCaseData(Vector3.one, k_ArbitraryRotationAroundYAxis, k_UniformScale,
                    new Vector3(3.577382f, 2f, 0.7936923f), new Vector3(4.422618f, 2f, 2.606308f))
                .SetName("Rotated, scaled and with offset")
        };

        [TestCaseSource(nameof(k_SkewedTestCases))]
        public void GetWorldPositionsForPoints_WhenLinkParentHasUnevenScale_ReturnsEndpointsNonSkewed(
            Vector3 transformPosition, Quaternion transformRotation, Vector3 transformScale,
            Vector3 expectedStart, Vector3 expectedEnd)
        {
            m_NavMeshLink.transform.parent = m_ScaledSkewer.transform;
            m_NavMeshLink.transform.localPosition = transformPosition;
            m_NavMeshLink.transform.localRotation = transformRotation;
            m_NavMeshLink.transform.localScale = transformScale;
            m_NavMeshLink.startTransform = null;
            m_NavMeshLink.endTransform = null;
            Assume.That(Vector3.zero, Is.Not.EqualTo(m_NavMeshLink.startPoint).Or.Not.EqualTo(m_NavMeshLink.endPoint),
                "At least one endpoint should be skewed away from the local origin.");

            m_LinkImitator.transform.parent = m_ScaledSkewer.transform;
            m_LinkImitator.transform.localPosition = transformPosition;
            m_LinkImitator.transform.localRotation = transformRotation;
            m_LinkImitator.transform.localScale = transformScale;
            m_LinkStartImitator.transform.localPosition = m_NavMeshLink.startPoint;
            m_LinkEndImitator.transform.localPosition = m_NavMeshLink.endPoint;

            Assume.That(m_LinkStartImitator.transform.position, Is.Not.EqualTo(expectedStart)
                    .Using(Vector3EqualityComparer.Instance),
                "The wanted link start position should not be skewed along with the transform hierarchy.");

            Assume.That(m_LinkEndImitator.transform.position, Is.Not.EqualTo(expectedEnd)
                    .Using(Vector3EqualityComparer.Instance),
                "The wanted link end position should not be skewed along with the transform hierarchy.");

            m_NavMeshLink.GetWorldPositions(out var worldStart, out var worldEnd);

            // Uncomment to get the new expected values if you change the setup
            //Debug.Log($"(new Vector3({worldStart.x}f, {worldStart.y}f, {worldStart.z}f), new Vector3({worldEnd.x}f, {worldEnd.y}f, {worldEnd.z}f))");

            Assert.That(worldStart, Is.EqualTo(expectedStart)
                    .Using(Vector3EqualityComparer.Instance),
                "Start position should be at an unscaled offset from the Link.");

            Assert.That(worldEnd, Is.EqualTo(expectedEnd)
                    .Using(Vector3EqualityComparer.Instance),
                "End position should be at an unscaled offset from the Link.");
        }

        static readonly Vector3 k_DoubleOne = 2f * Vector3.one;
        static readonly TestCaseData[] k_TestCasesForLocal =
        {
            new TestCaseData(false, false, Vector3.one, -Vector3.one)
                .SetName("From start and end points"),
            new TestCaseData(false, true, Vector3.one, -k_DoubleOne)
                .SetName("From start point and end transform"),
            new TestCaseData(true, false, k_DoubleOne, -Vector3.one)
                .SetName("From start transform and end point"),
            new TestCaseData(true, true, k_DoubleOne, -k_DoubleOne)
                .SetName("From start and end transforms")
        };

        [TestCaseSource(nameof(k_TestCasesForLocal))]
        public void GetLocalPositions_ReturnsExpectedResults(
            bool useStartTransform, bool useEndTransform,
            Vector3 expectedStart, Vector3 expectedEnd)
        {
            var origin = Vector3.one;
            m_NavMeshLink.transform.SetPositionAndRotation(origin, Quaternion.Euler(90f, -90f, 90f));
            m_NavMeshLink.transform.localScale = k_DoubleOne;
            m_StartTransform.position = origin - k_DoubleOne;
            m_EndTransform.position = origin + k_DoubleOne;

            m_NavMeshLink.enabled = false;
            m_NavMeshLink.startTransform = useStartTransform ? m_StartTransform : null;
            m_NavMeshLink.endTransform = useEndTransform ? m_EndTransform : null;
            m_NavMeshLink.startPoint = !useStartTransform ? Vector3.one : 100f * Vector3.one;
            m_NavMeshLink.endPoint = !useEndTransform ? -Vector3.one : -100f * Vector3.one;
            m_NavMeshLink.enabled = true;

            m_NavMeshLink.GetLocalPositions(out var localStart, out var localEnd);

            Assert.That(localStart, Is.EqualTo(expectedStart).Using(Vector3EqualityComparer.Instance),
                "Start should be reported at a different position.");
            Assert.That(localEnd, Is.EqualTo(expectedEnd).Using(Vector3EqualityComparer.Instance),
                "End should be reported at a different position.");
        }
    }
}

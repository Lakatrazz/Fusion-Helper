using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using From = Unity.AI.Navigation.Editor.Tests.NavMeshLinkEditorTests.LinkEndType;
using To = Unity.AI.Navigation.Editor.Tests.NavMeshLinkEditorTests.LinkEndType;

// Note: To pause and inspect the state during these editor tests,
// run them in playmode from the NavMeshLinkEditorTestsInPlaymode class
// in the Unity.AI.Navigation.Editor.Tests.InPlaymode namespace.
namespace Unity.AI.Navigation.Editor.Tests
{
    public class NavMeshLinkEditorTests
    {
        List<UnityEngine.Object> m_TestObjects = new();
        GameObject m_LinkGameObject;
        GameObject m_Start;
        GameObject m_End;
        NavMeshLink m_Link;
        NavMeshLink m_LinkSibling1;
        NavMeshLink m_LinkSibling2;

        static readonly Vector3EqualityComparer k_DefaultThreshold = Vector3EqualityComparer.Instance;

        GameObject CreateTestObject(string name, params Type[] components)
        {
            var go = new GameObject(name, components);
            m_TestObjects.Add(go);
            return go;
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            m_LinkGameObject = new GameObject("Link");
            m_Link = m_LinkGameObject.AddComponent<NavMeshLink>();
            m_Start = new GameObject("Start");
            m_End = new GameObject("End");

            // To debug, add these components, only to show icons for them in the scene
            //m_Start.AddComponent<NavMeshSurface>().enabled = false;
            //m_End.AddComponent<NavMeshModifier>().enabled = false;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (m_LinkGameObject != null)
                UnityEngine.Object.DestroyImmediate(m_LinkGameObject);
            if (m_Start != null)
                UnityEngine.Object.DestroyImmediate(m_Start);
            if (m_End != null)
                UnityEngine.Object.DestroyImmediate(m_End);
        }

        [SetUp]
        public void Setup()
        {
            using (new NavMeshLinkEditor.DeferredLinkUpdateScope(m_Link))
            {
                m_Link.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                m_Link.transform.localScale = Vector3.one;
                m_Link.startPoint = Vector3.left;
                m_Link.endPoint = Vector3.right;
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (m_LinkSibling1 != null)
                UnityEngine.Object.DestroyImmediate(m_LinkSibling1);

            if (m_LinkSibling2 != null)
                UnityEngine.Object.DestroyImmediate(m_LinkSibling2);

            foreach (var obj in m_TestObjects)
            {
                if (obj != null)
                    UnityEngine.Object.DestroyImmediate(obj);
            }

            m_TestObjects.Clear();
        }

        protected static readonly Vector3[] k_ReverseDirectionPositions =
            { Vector3.zero, new(1f, 2f, 3f), new(1f, -2f, 3f) };
        protected static readonly Quaternion[] k_ReverseDirectionOrientations =
            { Quaternion.identity, new(0f, 0.7071067812f, 0f, 0.7071067812f) };
        protected static readonly Vector3[] k_ReverseDirectionScales =
            { Vector3.one, new(0.5f, 1f, 2f), new(0.5f, -1f, 2f) };

        [Test]
        public void ReverseDirection_SwapsStartAndEndPoints(
            [ValueSource(nameof(k_ReverseDirectionPositions))]
            Vector3 position,
            [ValueSource(nameof(k_ReverseDirectionOrientations))]
            Quaternion orientation,
            [ValueSource(nameof(k_ReverseDirectionScales))]
            Vector3 scale
        )
        {
            using (new NavMeshLinkEditor.DeferredLinkUpdateScope(m_Link))
            {
                m_Link.transform.SetPositionAndRotation(position, orientation);
                m_Link.transform.localScale = scale;
                m_Link.startPoint = new Vector3(2f, 0f, 0f);
                m_Link.endPoint = new Vector3(0f, 0f, 2f);
            }

            NavMeshLinkEditor.ReverseDirection(m_Link);

            Assert.That(
                (m_Link.startPoint, m_Link.endPoint),
                Is.EqualTo((new Vector3(0f, 0f, 2f), new Vector3(2f, 0f, 0f))),
                "Start and end points did not swap."
            );
        }

        [Test]
        public void ReverseDirection_SwapsStartAndEndPoints_TargetTransformsDoNotAffect(
            [ValueSource(nameof(k_ReverseDirectionPositions))]
            Vector3 position,
            [ValueSource(nameof(k_ReverseDirectionOrientations))]
            Quaternion orientation,
            [ValueSource(nameof(k_ReverseDirectionScales))]
            Vector3 scale
        )
        {
            using (new NavMeshLinkEditor.DeferredLinkUpdateScope(m_Link))
            {
                m_Link.startTransform = CreateTestObject("Start").transform;
                m_Link.endTransform = CreateTestObject("End").transform;
                m_Link.transform.SetPositionAndRotation(position, orientation);
                m_Link.transform.localScale = scale;
                m_Link.startPoint = new Vector3(2f, 0f, 0f);
                m_Link.endPoint = new Vector3(0f, 0f, 2f);
            }

            NavMeshLinkEditor.ReverseDirection(m_Link);

            Assert.That(
                (m_Link.startPoint, m_Link.endPoint),
                Is.EqualTo((new Vector3(0f, 0f, 2f), new Vector3(2f, 0f, 0f))),
                "Start and end points did not swap."
            );
        }

        [Test]
        public void ReverseDirection_SwapsStartAndEndTransforms()
        {
            var start = m_Link.startTransform = CreateTestObject("Start").transform;
            var end = m_Link.endTransform = CreateTestObject("End").transform;

            NavMeshLinkEditor.ReverseDirection(m_Link);

            Assert.That(
                (m_Link.startTransform, m_Link.endTransform),
                Is.EqualTo((end, start)),
                "Start and end transform did not swap."
            );
        }

        [Test]
        public void ReverseDirection_OneTransformIsNotSet_SwapsStartAndEndTransforms()
        {
            var start = m_Link.startTransform = CreateTestObject("Start").transform;
            var end = m_Link.endTransform;

            NavMeshLinkEditor.ReverseDirection(m_Link);

            Assert.That(
                (m_Link.startTransform, m_Link.endTransform),
                Is.EqualTo((end, start)),
                "Start and end transform did not swap."
            );
        }

        static readonly Vector3 k_Offset101 = new(1, 1, 1);
        static readonly Vector3 k_Offset103 = new(1, 1, 3);
        static readonly Quaternion k_DoNotRotate = Quaternion.identity;
        static readonly Quaternion k_FlipToRight = Quaternion.Euler(0, 0, -90);
        static readonly Quaternion k_UpSideDown = Quaternion.Euler(0, 0, 180);

        //TestCaseData( startType, endType, transformRotation,
        //      expectedPosition, expectedForward)
        protected static readonly TestCaseData[] k_PointsOnly =
        {
            new(From.Point, To.Point, k_DoNotRotate,
                new Vector3(1, 1, 2), Vector3.back),

            new(From.Point, To.Point, k_UpSideDown,
                new Vector3(-1, -1, 2), Vector3.back),
        };
        protected static readonly TestCaseData[] k_PointAndTransforms =
        {
            new(From.Point, To.Transform, k_DoNotRotate,
                new Vector3(1, 2, 2), Vector3.back),

            new(From.Point, To.Transform, k_FlipToRight,
                new Vector3(1, 1, 2), Quaternion.Euler(-116.565f, 0, -90) * Vector3.forward),

            new(From.Transform, To.Point, k_DoNotRotate,
                new Vector3(1, 2, 2), Vector3.back),

            new(From.Transform, To.Point, k_FlipToRight,
                new Vector3(1, 1, 2), Quaternion.Euler(116.565f, 0, -90) * Vector3.forward),

            new(From.Transform, To.Transform, k_DoNotRotate,
                new Vector3(1, 3, 2), Vector3.back),

            new(From.Transform, To.Transform, k_UpSideDown,
                new Vector3(1, 3, 2), Vector3.back),
        };
        protected static readonly TestCaseData[] k_ChildTransforms =
        {
            new(From.TransformChild, To.TransformChild, k_FlipToRight,
                new Vector3(3, -1, 2), Vector3.back),
        };

        void ConfigureLinkForTest(From startType, To endType, Quaternion transformRotation)
        {
            m_Start.transform.position = k_Offset103 + 2f * Vector3.up;
            m_End.transform.position = k_Offset101 + 2f * Vector3.up;
            m_Start.transform.parent = startType == From.TransformChild ? m_Link.transform : null;
            m_End.transform.parent = endType == To.TransformChild ? m_Link.transform : null;

            using (new NavMeshLinkEditor.DeferredLinkUpdateScope(m_Link))
            {
                m_Link.startPoint = k_Offset103;
                m_Link.endPoint = k_Offset101;
                m_Link.startTransform = startType != From.Point ? m_Start.transform : null;
                m_Link.endTransform = endType != To.Point ? m_End.transform : null;

                m_Link.transform.rotation = transformRotation;
            }
        }

        [TestCaseSource(nameof(k_PointsOnly))]
        [TestCaseSource(nameof(k_PointAndTransforms))]
        [TestCaseSource(nameof(k_ChildTransforms))]
        public void AlignTransformToEndPoints_MovesTransformInTheMiddle(
            From startType, To endType, Quaternion transformRotation,
            Vector3 expectedPosition, Vector3 _)
        {
            ConfigureLinkForTest(startType, endType, transformRotation);

            NavMeshLinkEditor.AlignTransformToEndPoints(m_Link);

            Assert.That(m_Link.transform.position, Is.EqualTo(expectedPosition).Using(k_DefaultThreshold),
                "The Link object should be in the middle between the endpoints.");
        }

        [TestCaseSource(nameof(k_PointsOnly))]
        [TestCaseSource(nameof(k_PointAndTransforms))]
        [Description("When the endpoints remain at the same world position it means that the local points must have been adjusted correctly.")]
        public void AlignTransformToEndPoints_EndpointsWorldPositionsRemainUnchanged(
            From startType, To endType, Quaternion transformRotation,
            Vector3 _, Vector3 __)
        {
            ConfigureLinkForTest(startType, endType, transformRotation);

            var initialEndpointsMatrix = LocalToWorldUnscaled(m_Link);
            var initialStartWorld = initialEndpointsMatrix.MultiplyPoint3x4(m_Link.startPoint);
            var initialEndWorld = initialEndpointsMatrix.MultiplyPoint3x4(m_Link.endPoint);
            var initialStartLocal = m_Link.startPoint;
            var initialEndLocal = m_Link.endPoint;

            NavMeshLinkEditor.AlignTransformToEndPoints(m_Link);

            Assume.That(m_Link.startPoint, Is.Not.EqualTo(initialStartLocal).Using(k_DefaultThreshold),
                "The local position of Start point should have been adjusted.");

            Assert.That(m_Link.endPoint, Is.Not.EqualTo(initialEndLocal).Using(k_DefaultThreshold),
                "The local position of End point should have been adjusted.");

            var endpointsMatrix = LocalToWorldUnscaled(m_Link);
            var startWorld = endpointsMatrix.MultiplyPoint3x4(m_Link.startPoint);
            var endWorld = endpointsMatrix.MultiplyPoint3x4(m_Link.endPoint);

            Assert.That(startWorld, Is.EqualTo(initialStartWorld).Using(k_DefaultThreshold),
                "The world position of Start should remain unchanged.");

            Assert.That(endWorld, Is.EqualTo(initialEndWorld).Using(k_DefaultThreshold),
                "The world position of End should remain unchanged.");
        }

        static Matrix4x4 LocalToWorldUnscaled(NavMeshLink link)
        {
            return Matrix4x4.TRS(link.transform.position, link.transform.rotation, Vector3.one);
        }

        [TestCaseSource(nameof(k_PointAndTransforms))]
        [TestCaseSource(nameof(k_ChildTransforms))]
        public void AlignTransformToEndPoints_EndsTransformsRemainUnchanged(
            From startType, To endType, Quaternion transformRotation,
            Vector3 _, Vector3 __)
        {
            ConfigureLinkForTest(startType, endType, transformRotation);

            var initialStartPosition = m_Link.startTransform != null ? m_Link.startTransform.position : Vector3.negativeInfinity;
            var initialEndPosition = m_Link.endTransform != null ? m_Link.endTransform.position : Vector3.negativeInfinity;

            NavMeshLinkEditor.AlignTransformToEndPoints(m_Link);

            if (m_Link.startTransform != null)
                Assert.That(m_Link.startTransform.position, Is.EqualTo(initialStartPosition).Using(k_DefaultThreshold),
                    "The Link start transform should not have moved.");

            if (m_Link.endTransform != null)
                Assert.That(m_Link.endTransform.position, Is.EqualTo(initialEndPosition).Using(k_DefaultThreshold),
                    "The Link end transform should not have moved.");
        }

        [Test]
        [Explicit("Functionality not implemented yet for child game objects")]
        public void AlignTransformToEndPoints_ChildGameObjectsRetainWorldPositions()
        {
            using (new NavMeshLinkEditor.DeferredLinkUpdateScope(m_Link))
            {
                m_Link.startPoint = k_Offset103;
                m_Link.endPoint = k_Offset101;
                m_Link.startTransform = null;
                m_Link.endTransform = null;

                m_Link.transform.rotation = k_FlipToRight;
            }

            var child1 = CreateTestObject("Child 1").GetComponent<Transform>();
            var child2 = CreateTestObject("Child 2").GetComponent<Transform>();
            var grandchild = CreateTestObject("Grandchild").GetComponent<Transform>();
            child2.rotation = Quaternion.Euler(0, 90, 0);
            child1.parent = m_Link.transform;
            child2.parent = m_Link.transform;
            grandchild.parent = child2.transform;

            child1.position = new Vector3(2, 1, 3);
            child2.position = new Vector3(-0.5f, 0.6f, 0.7f);
            grandchild.position = new Vector3(-3, 2, 1);
            var child1Earlier = child1.position;
            var child2Earlier = child2.position;
            var grandchildEarlier = grandchild.position;

            NavMeshLinkEditor.AlignTransformToEndPoints(m_Link);

            Assert.That(child1.position, Is.EqualTo(child1Earlier).Using(k_DefaultThreshold),
                "Child object 1 should not have moved.");
            Assert.That(child2.position, Is.EqualTo(child2Earlier).Using(k_DefaultThreshold),
                "Child object 2 should not have moved.");
            Assert.That(grandchild.position, Is.EqualTo(grandchildEarlier).Using(k_DefaultThreshold),
                "Grandchild object should not have moved.");
        }

        [Test]
        public void AlignTransformToEndPoints_EndpointsWorldPositionsRemainUnchanged_InSiblingLinks()
        {
            using (new NavMeshLinkEditor.DeferredLinkUpdateScope(m_Link))
            {
                m_Link.startPoint = k_Offset103;
                m_Link.endPoint = k_Offset101;
                m_Link.startTransform = null;
                m_Link.endTransform = null;

                m_Link.transform.rotation = k_FlipToRight;
            }

            m_LinkSibling1 = m_Link.gameObject.AddComponent<NavMeshLink>();
            m_LinkSibling2 = m_Link.gameObject.AddComponent<NavMeshLink>();
            m_LinkSibling1.startPoint = m_Link.endPoint;
            m_LinkSibling1.endPoint = m_Link.startPoint;
            m_LinkSibling2.startPoint = m_Link.endPoint;
            m_LinkSibling2.endPoint = m_Link.startPoint;

            var initialStartLocal = m_Link.startPoint;

            NavMeshLinkEditor.AlignTransformToEndPoints(m_Link);

            Assume.That(m_Link.startPoint, Is.Not.EqualTo(initialStartLocal).Using(k_DefaultThreshold));

            Assert.That(m_LinkSibling1.startPoint, Is.EqualTo(m_Link.endPoint).Using(k_DefaultThreshold),
                "The sibling 1 Start point should have been adjusted.");
            Assert.That(m_LinkSibling1.endPoint, Is.EqualTo(m_Link.startPoint).Using(k_DefaultThreshold),
                "The sibling 1 End point should have been adjusted.");

            Assert.That(m_LinkSibling2.startPoint, Is.EqualTo(m_Link.endPoint).Using(k_DefaultThreshold),
                "The sibling 2 Start point should have been adjusted.");
            Assert.That(m_LinkSibling2.endPoint, Is.EqualTo(m_Link.startPoint).Using(k_DefaultThreshold),
                "The sibling 2 End point should have been adjusted.");
        }

        [TestCaseSource(nameof(k_PointsOnly))]
        [TestCaseSource(nameof(k_PointAndTransforms))]
        [TestCaseSource(nameof(k_ChildTransforms))]
        public void AlignTransformToEndPoints_UpVectorRemainsUnchanged(
            From startType, To endType, Quaternion transformRotation,
            Vector3 _, Vector3 __)
        {
            ConfigureLinkForTest(startType, endType, transformRotation);

            var initialUp = m_Link.transform.up;

            NavMeshLinkEditor.AlignTransformToEndPoints(m_Link);

            Assert.That(m_Link.transform.up, Is.EqualTo(initialUp).Using(k_DefaultThreshold),
                "The Link's up vector should remain unchanged.");
        }

        [TestCaseSource(nameof(k_PointsOnly))]
        [TestCaseSource(nameof(k_PointAndTransforms))]
        [TestCaseSource(nameof(k_ChildTransforms))]
        public void AlignTransformToEndPoints_OrientsForwardVectorFromStartToEndInXZPlane(
            From startType, To endType, Quaternion transformRotation,
            Vector3 _, Vector3 expectedForward)
        {
            ConfigureLinkForTest(startType, endType, transformRotation);

            NavMeshLinkEditor.AlignTransformToEndPoints(m_Link);

            Assert.That(m_Link.transform.forward, Is.EqualTo(expectedForward).Using(k_DefaultThreshold),
                "The Link's forward vector should point from the start towards the end of the link.");
        }

        static readonly Vector3 k_ForwardRightDiagonal = Quaternion.Euler(0, 45, 0) * Vector3.forward;
        static readonly Quaternion k_RotatedX90 = Quaternion.Euler(90, 0, 0);
        static readonly Quaternion k_RotatedY180 = Quaternion.Euler(0, 180, 0);
        static readonly Quaternion k_RotatedZ90 = Quaternion.Euler(0, 0, 90);
        static readonly Quaternion k_RotatedZ180 = Quaternion.Euler(0, 0, 180);
        protected static readonly TestCaseData[] k_RotateToChangeLinkDirectionLocally =
        {
            new TestCaseData(Quaternion.identity, Vector3.right, Vector3.one)
                .SetName("Link aligned to World axes"),

            new TestCaseData(k_RotatedX90, Vector3.zero, new Vector3(1, 1, -1))
                .SetName("Endpoints following the Up direction")
                .SetDescription("The right vector cannot be properly defined in this case"),

            new TestCaseData(k_RotatedY180, Vector3.forward, new Vector3(-1, 1, -1))
                .SetName("Endpoints following Right direction"),

            new TestCaseData(k_RotatedZ90, Vector3.right, new Vector3(1, -1, 1))
                .SetName("Link tipped to the left"),

            new TestCaseData(k_RotatedZ180, k_ForwardRightDiagonal, new Vector3(-1, -1, 1))
                .SetName("Link rotated freely")
                .SetDescription("The rotation has been chosen to produce a Right vector easy to identify and verify."),
        };

        [TestCaseSource(nameof(k_RotateToChangeLinkDirectionLocally))]
        [Description("The link's direction changes because the end transform moves in local space when the game object rotates.")]
        public void CalcLinkRight_ReturnsLocalRightIn2D(
            Quaternion transformRotation,
            Vector3 expectedLocalRight, Vector3 expectedEnd)
        {
            m_Link.transform.SetPositionAndRotation(Vector3.one, transformRotation);
            m_Link.transform.localScale = 2f * Vector3.one;
            m_End.transform.position = 2f * Vector3.one;

            using (new NavMeshLinkEditor.DeferredLinkUpdateScope(m_Link))
            {
                m_Link.startPoint = new Vector3(1, -1, -1);
                m_Link.endTransform = m_End.transform;

                m_Link.startTransform = null;
                m_Link.endPoint = Vector3.negativeInfinity;
            }

            var linkRight = NavMeshLinkEditor.GetLocalDirectionRight(m_Link, out var localStart, out var localEnd);

            Assume.That(localStart, Is.EqualTo(m_Link.startPoint).Using(k_DefaultThreshold), "Wrong local Start reported.");
            Assume.That(localEnd, Is.EqualTo(expectedEnd).Using(k_DefaultThreshold), "Wrong local End reported.");

            Assert.That(linkRight, Is.EqualTo(expectedLocalRight).Using(k_DefaultThreshold),
                "Wrong Right vector relative to the direction from start to end.");
        }

        public enum LinkEndType
        {
            Point,
            Transform,
            TransformChild
        }
    }
}

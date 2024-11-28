using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AI.Navigation.Editor.Tests
{
    internal class InspectorWindowWrapper
    {
        readonly EditorWindow m_InspectorWindow;
        readonly bool m_OriginalInspectorThrottling;
        readonly bool m_OriginalPanelThrottling;

        public InspectorWindowWrapper()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in windows)
            {
                if (window.GetType().Name != "InspectorWindow")
                    continue;

                m_InspectorWindow = window;
                break;
            }

            Assume.That(m_InspectorWindow, Is.Not.Null, "Inspector window not found");
            m_OriginalInspectorThrottling = GetDisableInspectorElementThrottling();
            m_OriginalPanelThrottling = GetDisablePanelThrottling();

            SetDisableInspectorElementThrottling(true);
            SetDisablePanelThrottling(true);
        }

        /// <summary>
        /// Set the inspector's element throttling, by accessing and setting its internal disabledThrottling property.
        /// The throttling is there to collect calls to UI Toolkit and execute them in batch.
        /// Disabling the throttling means that calls will be executed at the next available time.
        /// </summary>
        /// <param name="enabled">True means that throttling is disabled.</param>
        static void SetDisableInspectorElementThrottling(bool enabled)
        {
            var throttlingPropertyInfo = GetDisableInspectorElementThrottlingPropertyInfo();
            throttlingPropertyInfo?.SetValue(null, enabled);
        }

        static bool GetDisableInspectorElementThrottling()
        {
            var throttlingPropertyInfo = GetDisableInspectorElementThrottlingPropertyInfo();
            return (bool)throttlingPropertyInfo?.GetValue(null);
        }

        static PropertyInfo GetDisableInspectorElementThrottlingPropertyInfo()
        {
            return typeof(InspectorElement).GetProperty("disabledThrottling", BindingFlags.Static | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Set the inspector's panel throttling, by accessing and setting its internal TimerEventScheduler's disableThrottling property.
        /// The throttling is there to reduce the number of times a UI panel is refreshed in a given interval.
        /// Disabling the throttling means that the refresh will happen as frequently as the system allows it to refresh.
        /// </summary>
        /// <param name="enabled">True means that throttling is disabled.</param>
        void SetDisablePanelThrottling(bool enabled)
        {
            var disableThrottlingPropInfo = GetDisablePanelThrottlingPropertyInfo(out var scheduler);
            disableThrottlingPropInfo?.SetValue(scheduler, enabled);
        }

        bool GetDisablePanelThrottling()
        {
            var disableThrottlingPropInfo = GetDisablePanelThrottlingPropertyInfo(out var scheduler);
            return (bool)disableThrottlingPropInfo.GetValue(scheduler);
        }

        FieldInfo GetDisablePanelThrottlingPropertyInfo(out object scheduler)
        {
            var panel = GetRootVisualElement().panel;
            var schedulerPropInfo = panel.GetType().GetProperty("scheduler", BindingFlags.Instance | BindingFlags.NonPublic);
            scheduler = schedulerPropInfo?.GetValue(panel);

            Assume.That(scheduler?.GetType().Name, Is.EqualTo("TimerEventScheduler"), "Scheduler is not a TimerEventScheduler");
            return scheduler?.GetType().GetField("disableThrottling", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public void RepaintImmediately()
        {
            var repaintMethodInfo = m_InspectorWindow.GetType().GetMethod("RepaintImmediately", BindingFlags.Instance | BindingFlags.NonPublic);
            repaintMethodInfo?.Invoke(m_InspectorWindow, null);
        }

        public void Focus() => m_InspectorWindow.Focus();

        public void Close()
        {
            SetDisablePanelThrottling(m_OriginalPanelThrottling);
            SetDisableInspectorElementThrottling(m_OriginalInspectorThrottling);
            m_InspectorWindow.Close();
        }

        public VisualElement GetRootVisualElement() => m_InspectorWindow.rootVisualElement;
    }
}

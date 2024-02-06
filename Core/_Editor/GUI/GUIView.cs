using System;
using System.Reflection;
using UnityEditor;

namespace UnitySimplifiedEditor
{
    // Courtesy of users on UnityForums: https://forum.unity.com/threads/extend-editorgui.465968/
    public static class GUIView
    {
        private static readonly PropertyInfo CurrentInfo;
        private static readonly MethodInfo RepaintImmediatelyInfo;

        public static EditorWindow Current { get; }

        static GUIView()
        {
            var guiViewType = Type.GetType("UnityEditor.GUIView, UnityEditor");
            var hostViewType = Type.GetType("UnityEditor.HostView, UnityEditor");

            if (guiViewType == null)
                throw new NullReferenceException(nameof(guiViewType));
            if (hostViewType == null)
                throw new NullReferenceException(nameof(hostViewType));

            CurrentInfo = guiViewType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
            RepaintImmediatelyInfo = guiViewType.GetMethod("RepaintImmediately", BindingFlags.Public | BindingFlags.Instance);
            
            var actualView = hostViewType.GetProperty("actualView", BindingFlags.NonPublic | BindingFlags.Instance);
            if (actualView != null && CurrentInfo != null)
                Current = (EditorWindow)actualView.GetValue(CurrentInfo.GetValue(null, null), null);
        }

        public static void Repaint() => Current.Repaint();
        public static void RepaintImmediately() => RepaintImmediatelyInfo.Invoke(CurrentInfo.GetValue(null, null), null);
    }
}
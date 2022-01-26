#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;

namespace UnitySimplifiedEditor
{
    // Courtesy of users on UnityForums: https://forum.unity.com/threads/extend-editorgui.465968/
    public static class GUIView
    {
        private static readonly PropertyInfo currentInfo;
        private static readonly MethodInfo repaintImmediatelyInfo;
        private static readonly EditorWindow current;

        public static EditorWindow Current => current;

        static GUIView()
        {
            var guiViewType = Type.GetType("UnityEditor.GUIView, UnityEditor");
            var hostView = Type.GetType("UnityEditor.HostView, UnityEditor");

            currentInfo = guiViewType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
            repaintImmediatelyInfo = guiViewType.GetMethod("RepaintImmediately", BindingFlags.Public | BindingFlags.Instance);
            
            var actualView = hostView.GetProperty("actualView", BindingFlags.NonPublic | BindingFlags.Instance);
            current = (EditorWindow)actualView.GetValue(currentInfo.GetValue(null, null), null);
        }

        public static void RepaintImmediately()
        {   repaintImmediatelyInfo.Invoke(currentInfo.GetValue(null, null), null);   }
    }
}

#endif
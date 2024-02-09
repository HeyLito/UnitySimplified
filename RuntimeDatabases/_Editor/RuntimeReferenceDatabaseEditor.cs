#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.RuntimeDatabases;

namespace UnitySimplifiedEditor.RuntimeDatabases
{
    [CustomEditor(typeof(RuntimeReferenceDatabase))]
    public class RuntimeReferenceDatabaseEditor : Editor
    {
        //private string _search = "";
        //protected override void OnEnable() => base.OnEnable();

        //public override void OnInspectorGUI() => DisplayReferences(Target.WrappedReferences);

        //protected void DisplayReferences(Dictionary<Type, ValueTuple<Dictionary<string, object>, Dictionary<object, string>>> wrappedRefs)
        //{
        //    if (wrappedRefs.Count > 0)
        //    {
        //        EditorGUILayout.BeginHorizontal();
        //        if (GUILayout.Button("Cleanup"))
        //            wrappedRefs.Clear();
        //        GUILayout.FlexibleSpace();
        //        _search = EditorGUILayout.TextField(_search, GUI.skin.FindStyle("ToolbarSeachTextField"));
        //        if (_search.Length > 0 && GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
        //        {
        //            _search = "";
        //            GUI.FocusControl(null);
        //        }
        //        EditorGUILayout.EndHorizontal();
        //    }

        //    Color color = GUI.backgroundColor;
        //    EditorGUILayout.BeginVertical(Box);
        //    GUI.backgroundColor = color;
        //    foreach (var reference in wrappedRefs)
        //    {
        //        if (reference.Value.Item1.Count > 0 || reference.Value.Item2.Count > 0)
        //        {
        //            EditorGUILayout.LabelField($"Typeof<b>({reference.Key.Name})</b>", IDStyle);
        //            GUI.backgroundColor -= new Color(0.35f, 0.35f, 0.35f, 0);
        //            EditorGUILayout.BeginVertical(Box);
        //            GUI.backgroundColor = color;
        //            foreach (var pair in reference.Value.Item1)
        //            {
        //                if (_search.Length == 0 || pair.Key.Contains(_search) || pair.Value.ToString().Contains(_search))
        //                {
        //                    EditorGUILayout.LabelField($"ID: <i>{pair.Key}</i>", IDStyle);
        //                    EditorGUI.indentLevel++;
        //                    EditorGUILayout.LabelField($"<b>{(pair.Value != null ? pair.Value : "NULL")}</b>", UnityObjectStyle);
        //                    EditorGUI.indentLevel--;
        //                }
        //                //Rect rect = EditorGUILayout.BeginHorizontal("Button");
        //                //if (GUI.Button(rect, GUIContent.none, EditorStyles.textField) && values[i] != null)
        //                //    OnFieldClick(values[i]);

        //                //if (values[i] != null || values[i].GetType() != typeof(V))
        //                //    EditorGUILayout.LabelField($"<b>{values[i].name}</b>", UnityObjectStyle);
        //                //else EditorGUILayout.LabelField("NULL", ErrorStyle);
        //                //GUILayout.FlexibleSpace();
        //                //EditorGUILayout.EndHorizontal();
        //                //if (i + 1 >= keys.Count)
        //                //    EditorGUILayout.Space(2.5f);
        //            }
        //            EditorGUILayout.EndVertical();
        //        }
        //    }
        //    EditorGUILayout.EndVertical();
        //}
    }
}

#endif
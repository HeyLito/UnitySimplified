#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;
using UnitySimplified.GamePrefs;
using UnitySimplified.Serialization;

namespace UnitySimplifiedEditor.GamePrefs
{
    public class GamePrefSettingsWindow : EditorWindow
    {
        private string _localPath = "";
        private string _persistentPath = "";



        public static bool DebugMode { get; private set; }



        public static void OpenWindow()
        {
            var window = GetWindow<GamePrefSettingsWindow>(true, "GamePrefs Creator", true);
            window.ShowAuxWindow();
        }


        private void OnEnable()
        {
            string previousPath = DataManager.TargetDataPath;
            DataManager.TargetDataPath = DataManager.DefaultPath;
            DataManager.LoadDatabase();
            DataManager.ContainsFile("GamePrefs", out string fileName, out string filePath, out _);
            DataManager.TargetDataPath = previousPath;
            _persistentPath = Path.Combine(filePath, fileName);

            foreach (var directory in Directory.CreateDirectory(Application.dataPath).GetDirectories())
            foreach (var file in directory.GetFiles())
                if (file.Name == $"{nameof(GamePrefLocalDatabase)}.asset")
                    _localPath = file.FullName;
        }

        private void OnGUI()
        {
            GUIStyle titleStyle = new(EditorStyles.boldLabel) { richText = true };
            EditorGUILayout.Space(2);
            Rect last = EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIContent labelContent = new("Settings");
            EditorGUILayout.LabelField(labelContent, titleStyle, GUILayout.MaxWidth(titleStyle.CalcSize(labelContent).x));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6);

            EditorGUILayout.LabelField("Local GamePrefs", GUILayout.Width(EditorGUIUtility.labelWidth));
            GUIContent dynamicPrefsContent = new(!string.IsNullOrEmpty(_localPath) ? _localPath : "Missing");
            EditorGUI.indentLevel++;
            EditorGUI.SelectableLabel(GUILayoutUtility.GetRect(dynamicPrefsContent, EditorStyles.helpBox), dynamicPrefsContent.text, EditorStyles.helpBox);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Persistent GamePrefs", GUILayout.Width(EditorGUIUtility.labelWidth));
            GUIContent staticPrefsContent = new(!string.IsNullOrEmpty(_persistentPath) ? _persistentPath : "Missing");
            EditorGUI.indentLevel++;
            EditorGUI.SelectableLabel(GUILayoutUtility.GetRect(staticPrefsContent, EditorStyles.helpBox), staticPrefsContent.text, EditorStyles.helpBox);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Debug")))
            {
                DebugMode = !DebugMode;
                GetWindow<GamePrefWindow>(false, "", false).Repaint();
            }
            if (GUILayout.Button(new GUIContent("Reload GamePrefs")))
                GamePref.Reload();

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            
            if (Event.current.type != EventType.Repaint)
                return;
            var newPosition = position;
            newPosition.height = last.height;
            position = newPosition;
        }
    }
}

#endif
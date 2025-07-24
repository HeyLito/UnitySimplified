#if UNITY_EDITOR

using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
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
            var window = GetWindow<GamePrefSettingsWindow>(true, "GamePref Settings", true);
            window.ShowAuxWindow();
        }


        private void OnEnable()
        {
            if (!FileDatabase.TryGetDatabase(DataManager.DefaultPath, out FileDatabase fileDatabase))
            {
                fileDatabase = new FileDatabase(DataManager.DefaultPath);
                fileDatabase.SaveDatabase();
            }
            fileDatabase.ContainsFile("GamePrefs", out string _, out string filePath, out _);
            _persistentPath = filePath;

            foreach (var directory in Directory.CreateDirectory(Application.dataPath).GetDirectories())
                foreach (var file in directory.GetFiles())
                    if (file.Name == $"{nameof(GamePrefLocalDatabase)}.asset")
                        _localPath = file.FullName;
        }

        private void OnGUI()
        {
            var titleStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true };
            var linkLabel = new GUIStyle(EditorStyles.linkLabel) { richText = true };
            EditorGUILayout.Space(2);

            Rect last;
            using (var verticalScope = new EditorGUILayout.VerticalScope())
            {
                last = verticalScope.rect;

                using (_ = new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUIContent labelContent = new("Settings");
                    EditorGUILayout.LabelField(labelContent, titleStyle, GUILayout.MaxWidth(titleStyle.CalcSize(labelContent).x));
                    GUILayout.FlexibleSpace();
                }

                GUILayout.Space(6);

                var localPrefsHasPath = !string.IsNullOrWhiteSpace(_localPath)/* && AssetDatabase.LoadAssetAtPath<Object>(_localPath)*/;
                var localPrefsName = localPrefsHasPath ? "<i><u>Local GamePrefs</u></i>" : "Local GamePrefs";
                var localPrefsStyle = localPrefsHasPath ? linkLabel : EditorStyles.label;
                if (GUILayout.Button(localPrefsName, localPrefsStyle, GUILayout.Width(EditorGUIUtility.labelWidth)))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(AssetsRelativePath(_localPath));
                    EditorGUIUtility.PingObject(Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(AssetsRelativePath(_localPath)));
                }
                GUILayout.Space(2);
                GUIContent dynamicPrefsContent = new(localPrefsHasPath ? _localPath : "Missing");
                EditorGUI.indentLevel++;
                EditorGUI.SelectableLabel(GUILayoutUtility.GetRect(dynamicPrefsContent, EditorStyles.helpBox), dynamicPrefsContent.text, EditorStyles.helpBox);
                EditorGUI.indentLevel--;

                GUILayout.Space(4);

                var persistentPrefsHasPath = !string.IsNullOrWhiteSpace(_persistentPath);
                var persistentPrefsName = persistentPrefsHasPath ? "<i><u>Persistent GamePrefs</u></i>" : "Persistent GamePrefs";
                var persistentPrefsStyle = persistentPrefsHasPath ? linkLabel : EditorStyles.label;
                if (GUILayout.Button(persistentPrefsName, persistentPrefsStyle, GUILayout.Width(EditorGUIUtility.labelWidth)))
                {
                    if (persistentPrefsHasPath && Event.current.type == EventType.Used)
                    {
                        var directory = new FileInfo(_persistentPath).Directory?.FullName;
                        if (directory != null)
                            Process.Start(directory);
                    }
                }
                GUILayout.Space(2);
                GUIContent staticPrefsContent = new(persistentPrefsHasPath ? _persistentPath : "Missing");
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
            }

            if (Event.current.type != EventType.Repaint)
                return;
            var newPosition = position;
            newPosition.height = last.height;
            position = newPosition;
        }




        public static string AssetsRelativePath(string absolutePath)
        {
            var pathSeparatorPattern = $"[{Path.PathSeparator}]|[{Path.PathSeparator}{Path.PathSeparator}]";
            var directorySeparatorPattern = $"[{Path.DirectorySeparatorChar}]|[{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}]";
            var directoryAltSeparatorPattern = $"[{Path.AltDirectorySeparatorChar}]|[{Path.AltDirectorySeparatorChar}{Path.AltDirectorySeparatorChar}]";
            var regex = new Regex($"{pathSeparatorPattern}|{directorySeparatorPattern}|{directoryAltSeparatorPattern}");

            var dataPath = regex.Replace(Application.dataPath, Path.DirectorySeparatorChar.ToString());
            absolutePath = regex.Replace(absolutePath, Path.DirectorySeparatorChar.ToString());
            if (absolutePath.StartsWith(dataPath))
                return "Assets" + absolutePath[dataPath.Length..];
            return "";
        }
    }
}

#endif
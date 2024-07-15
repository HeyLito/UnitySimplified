#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified;
using UnitySimplified.GamePrefs;
// ReSharper disable InvertIf

namespace UnitySimplifiedEditor.GamePrefs
{
    public class GamePrefCreatorWindow : EditorWindow
    {
        [SerializeField]
        private string key;
        [SerializeField, SerializeReference]
        private BaseLocalGamePref activeValue;

        private SerializedObject _serializedObject;
        private SerializedProperty _keyProp;
        private SerializedProperty _activeValueProp;

        private readonly List<(BaseLocalGamePref localValue, Type valueType)> _values = new();
        private string[] _popupOptions = Array.Empty<string>();
        private int _popupIndex;

        private GUIStyle _groupPaddingStyle;
        private GUIStyle _wordWrapLabelStyle;
        private Vector2 _scrollPosition = Vector2.zero;

        public GUIStyle GroupPaddingStyle => _groupPaddingStyle ??= new GUIStyle { padding = new RectOffset(6, 6, 6, 6) };
        public GUIStyle WordWrapLabelStyle => _wordWrapLabelStyle ??= new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true, alignment = TextAnchor.MiddleCenter };

        public static void OpenWindow() => GetWindow<GamePrefCreatorWindow>(false, "GamePrefs Creator", true);

        private void OnEnable()
        {
            _values.Clear();
            foreach (var assembly in ApplicationUtility.GetAssemblies())
                foreach (var type in assembly.GetTypes())
                    if (IsSubclassOfLocalValue(type, out var genericArgumentType) && type != typeof(LocalGamePref<>))
                        _values.Add(((BaseLocalGamePref)Activator.CreateInstance(type), genericArgumentType));

            List<string> valueTypeNames = new() { "None" };
            valueTypeNames.AddRange(_values.Select(windowLocalValue => windowLocalValue.valueType.Name));
            _popupOptions = valueTypeNames.ToArray();

            _serializedObject = new SerializedObject(this);
            _keyProp = _serializedObject.FindProperty(nameof(key));
            _activeValueProp = _serializedObject.FindProperty(nameof(activeValue));
        }

        private void OnGUI()
        {
            using var groupScope = new EditorGUILayout.VerticalScope(GroupPaddingStyle);
            using var scrollViewScope = new EditorGUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scrollViewScope.scrollPosition;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_keyProp);

            var popupResult = EditorGUILayout.Popup("Value Type", _popupIndex, _popupOptions);
            if (popupResult != _popupIndex)
            {
                if (popupResult != 0)
                    activeValue = (BaseLocalGamePref)Activator.CreateInstance(_values.FirstOrDefault(x => x.valueType.Name == _popupOptions[popupResult]).localValue.GetType());
                else activeValue = null;
                _serializedObject.ApplyModifiedProperties();
                _serializedObject.Update();
                _popupIndex = popupResult;
            }
            if (popupResult != 0)
                EditorGUILayout.PropertyField(_activeValueProp.FindPropertyRelative("value"), true);

            scrollViewScope.Dispose();

            GUILayout.Space(5);
            EditorGUI.BeginDisabledGroup(!CanCreate());
            if (GUILayout.Button("Create GamePref"))
            {
#pragma warning disable UNT0018
                GamePrefLocalDatabase.Instance.AddGamePrefData(CreateGamePrefData(
                    GamePref.DoTryGetGamePrefFromKey(_keyProp.stringValue, out var data) ? data.identifier : GetNewPref().Identifier,
                    _keyProp.stringValue,
                    _activeValueProp.FindPropertyRelative("value").ExposeProperty(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, true)));
#pragma warning restore UNT0018
                EditorUtility.SetDirty(GamePrefLocalDatabase.Instance);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                _serializedObject.Update();
            }
        }
        private static GamePref GetNewPref() => GamePref.Create();
        private static GamePrefData CreateGamePrefData(string persistentIdentifier, string prefKey, object prefValue) => new(persistentIdentifier, prefKey, prefValue);
        private static GamePrefData CreateGamePrefData(GamePrefData gamePrefData) => new (gamePrefData);

        private bool CanCreate()
        {
            if (string.IsNullOrEmpty(_keyProp.stringValue))
                return false;
            if (_popupIndex == 0)
                return false;

            if (GamePrefLocalDatabase.Instance.HasKey(_keyProp.stringValue))
            {
                GUIContent labelContent = new("<color=#FF6666>A Local GamePref of the same key name already exists!</color>");
                WordWrapLabelStyle.CalcMinMaxWidth(labelContent, out _, out float maxWidth);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(labelContent, WordWrapLabelStyle, GUILayout.MaxWidth(Mathf.Min(maxWidth, position.width)));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                return false;
            }

            if (GamePref.DoTryGetGamePrefFromKey(_keyProp.stringValue, out var data))
            {
                if (_activeValueProp.ExposeProperty(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, true) is not BaseLocalGamePref localGamePref)
                    return false;
                if (localGamePref.ValueType != data.GetValueType())
                {
                    GUIContent labelContent = new("<color=#FF6666>Constructed GamePref does not match value type of persistent GamePref!</color>");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(labelContent, WordWrapLabelStyle, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                    return false;
                }
            }

            return true;
        }

        private static bool IsSubclassOfLocalValue(Type type, out Type genericArgumentType)
        {
            Type genericType = typeof(LocalGamePref<>);
            while (type != null && type != typeof(object))
            {
                Type previousType = type;
                var currentType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (genericType == currentType)
                {
                    genericArgumentType = previousType.GetGenericArguments()[0];
                    return true;
                }
                type = type.BaseType;
            }
            genericArgumentType = null;
            return false;
        }
    }
}

#endif
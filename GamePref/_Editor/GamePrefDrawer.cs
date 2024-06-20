using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.GamePrefs;

// ReSharper disable Unity.PerformanceCriticalCodeNullComparison
// ReSharper disable AccessToModifiedClosure

namespace UnitySimplifiedEditor.GamePrefs
{
    [CustomPropertyDrawer(typeof(GamePref))]
    public class GamePrefDrawer : PropertyDrawer
    {
        private readonly PropertyDrawerElementFilter<(int, int)> _menuSelections = new();
        private GUIStyle _objectFieldStyle;
        private GUIStyle _objectFieldButtonStyle;

        private GUIStyle ObjectFieldStyle => _objectFieldStyle ??= new GUIStyle("ObjectField");
        private GUIStyle ObjectFieldButtonStyle => _objectFieldButtonStyle ??= new GUIStyle("ObjectFieldButton");


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GamePrefLocalDatabase gamePrefLocalDatabase = GamePrefLocalDatabase.Instance;
            if (gamePrefLocalDatabase == null)
                throw new NullReferenceException(nameof(gamePrefLocalDatabase));

            var sortedPrefs = new List<KeyValuePair<string, GamePrefData>>(gamePrefLocalDatabase.GetGamePrefs());

            Event evt = Event.current;
            SerializedProperty gamePrefIdentifierProp = property.FindPropertyRelative("identifier");

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            Rect buttonRect = new(fieldRect.x + fieldRect.width - 1 - 18, fieldRect.y + 1, 18, 16);
            bool hasIdentifier = gamePrefLocalDatabase.TryGetFromIdentifier(gamePrefIdentifierProp.stringValue, out var gamePrefData);
            int controlIdentifier = GUIUtility.GetControlID(FocusType.Keyboard);

            _menuSelections.EvaluateGUIEventChanges(evt);
            var output = _menuSelections.GetFilteredElement(property, (-2, -2));
            int currentIndex = output.Item1;
            int selectedIndex = output.Item2;

            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button != 0 || !fieldRect.Contains(evt.mousePosition))
                        break;
                    if (buttonRect.Contains(evt.mousePosition))
                    {
                        GenericMenu options = new();
                        options.AddItem(new GUIContent("None"), string.IsNullOrEmpty(gamePrefIdentifierProp.stringValue), () => _menuSelections.SetElement(property, (currentIndex, -1)));
                        if (sortedPrefs.Count > 0)
                            options.AddSeparator("");

                        sortedPrefs.Sort(GamePrefWindow.SortPairs);
                        for (int i = 0; i < sortedPrefs.Count; i++)
                        {
                            int index = i;
                            currentIndex = gamePrefIdentifierProp.stringValue == sortedPrefs[i].Value.identifier ? index : currentIndex;
                            options.AddItem(new GUIContent(sortedPrefs[i].Value.key), currentIndex == index, () => _menuSelections.SetElement(property, (currentIndex, index)));
                            _menuSelections.SetElement(property, (currentIndex, selectedIndex));
                        }
                        options.DropDown(fieldRect);
                        break;
                    }
                    if (hasIdentifier && evt.clickCount == 2)
                    {
                        var window = EditorWindow.GetWindow<GamePrefWindow>();
                        window.titleContent = new GUIContent("GamePrefs");
                        window.Show();
                        window.Focus();
                        window.PingPref(gamePrefData.key);
                        break;
                    }
                    evt.Use();
                    GUIUtility.keyboardControl = controlIdentifier;

                    break;
                case EventType.KeyDown:
                    break;

                case EventType.Repaint:
                    string labelText = (!string.IsNullOrEmpty(gamePrefIdentifierProp.stringValue) ? hasIdentifier ? gamePrefData.key : "Missing" : "None") + $" ({nameof(GamePref)})";
                    ObjectFieldStyle.Draw(fieldRect, new GUIContent(labelText), fieldRect.Contains(evt.mousePosition), false, false, controlIdentifier == GUIUtility.keyboardControl);
                    ObjectFieldButtonStyle.Draw(buttonRect, GUIContent.none, buttonRect.Contains(evt.mousePosition), false, false, false);
                    break;
            }

            if (selectedIndex > -2 && currentIndex != selectedIndex)
            {
                sortedPrefs.Sort(GamePrefWindow.SortPairs);

                if (selectedIndex == -1)
                {
                    gamePrefIdentifierProp.stringValue = "";
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }
                else
                {
                    gamePrefIdentifierProp.stringValue = sortedPrefs[selectedIndex].Value.identifier;
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }
                _menuSelections.SetElement(property, (selectedIndex, selectedIndex));
            }
        }
    }
}
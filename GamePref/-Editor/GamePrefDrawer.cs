#if UNITY_EDITOR

using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.Serialization;

namespace UnitySimplifiedEditor.Serialization
{
    [CustomPropertyDrawer(typeof(GamePref))]
    public class GamePrefDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, (int, int)> _menuSelectionsByPaths = new Dictionary<string, (int, int)>();
        private GamePrefStorage _storage = null;
        private GUIStyle _objectFieldStyle = null;
        private GUIStyle _objectFieldButtonStyle = null;

        private GUIStyle ObjectFieldStyle => _objectFieldStyle ??= new GUIStyle("ObjectField");
        private GUIStyle ObjectFieldButtonStyle => _objectFieldButtonStyle ??= new GUIStyle("ObjectFieldButton");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {   return base.GetPropertyHeight(property, label);   }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_storage == null)
            {
                _storage = GamePrefStorage.Instance;
                if (_storage == null)
                    return;
                GamePrefWindow.onGamePrefsUpdated += GUIView.Current.Repaint;
            }

            var sortedPrefs = new List<KeyValuePair<string, GamePrefData>>(_storage.GetGamePrefs());

            Event evt = Event.current;
            GamePref pref = property.ExposeProperty(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) as GamePref;
            GamePrefData gamePrefData = null;
            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            Rect buttonRect = new Rect(fieldRect.x + fieldRect.width - 1 - 18, fieldRect.y + 1, 18, 16);
            bool hasID = pref != null && _storage.HasID(pref.PersistentIdentifier, out gamePrefData);
            int controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            int currentIndex = -2, selectedIndex = -2;
            if (_menuSelectionsByPaths.TryGetValue(property.propertyPath, out (int, int) tuple))
            {
                currentIndex = tuple.Item1;
                selectedIndex = tuple.Item2;
            }
            else _menuSelectionsByPaths[property.propertyPath] = (-2, -2);


            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 0 && fieldRect.Contains(evt.mousePosition))
                    {
                        if (buttonRect.Contains(evt.mousePosition))
                        {
                            GenericMenu options = new GenericMenu();
                            options.AddItem(new GUIContent("None"), pref == null, () => _menuSelectionsByPaths[property.propertyPath] = (currentIndex, -1));
                            if (sortedPrefs.Count > 0)
                                options.AddSeparator("");

                            sortedPrefs.Sort(GamePrefWindow.SortPairs);
                            for (int i = 0; i < sortedPrefs.Count; i++)
                            {
                                int index = i;
                                currentIndex = pref != null && pref.PersistentIdentifier == sortedPrefs[i].Value.persistentIdentifier ? index : currentIndex;
                                options.AddItem(new GUIContent(sortedPrefs[i].Value.prefKey), currentIndex == index, () => _menuSelectionsByPaths[property.propertyPath] = (currentIndex, index));
                                _menuSelectionsByPaths[property.propertyPath] = (currentIndex, selectedIndex);
                            }
                            options.DropDown(fieldRect);
                            break;
                        }
                        if (hasID && evt.clickCount == 2)
                        {
                            EditorWindow window = EditorWindow.GetWindow<GamePrefWindow>();
                            window.titleContent = new GUIContent("GamePrefs");
                            PingLabel.Ping(window, new GUIContent(gamePrefData.prefKey), true);
                            break;
                        }
                        evt.Use();
                        GUIUtility.keyboardControl = controlID;

                    }
                    break;
                case EventType.KeyDown:
                    break;
                case EventType.Repaint:
                    string labelText = pref != null ? hasID ? $"{gamePrefData.prefKey} ({typeof(GamePref).Name})" : $"Missing ({typeof(GamePref).Name})" : $"None ({typeof(GamePref).Name})";
                    ObjectFieldStyle.Draw(fieldRect, new GUIContent(labelText), fieldRect.Contains(evt.mousePosition), false, false, controlID == GUIUtility.keyboardControl);
                    ObjectFieldButtonStyle.Draw(buttonRect, GUIContent.none, buttonRect.Contains(evt.mousePosition), false, false, false);
                    break;
            }

            if (selectedIndex > -2 && currentIndex != selectedIndex)
            {
                sortedPrefs.Sort(GamePrefWindow.SortPairs);
                SerializedProperty persistentIdentifierProp = property.FindPropertyRelative("persistentIdentifier");

                if (selectedIndex == -1)
                {
                    persistentIdentifierProp.stringValue = "";
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }
                else
                {
                    persistentIdentifierProp.stringValue = sortedPrefs[selectedIndex].Value.persistentIdentifier;
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }
                _menuSelectionsByPaths[property.propertyPath] = (selectedIndex, selectedIndex);
            }
        }
    }
}

#endif
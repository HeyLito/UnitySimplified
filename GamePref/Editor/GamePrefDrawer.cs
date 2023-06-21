using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.Serialization;

namespace UnitySimplifiedEditor.Serialization
{
    [CustomPropertyDrawer(typeof(GamePref))]
    public class GamePrefDrawer : PropertyDrawer
    {
        private readonly PropertyDrawerElementFilter<(int, int)> _menuSelections = new PropertyDrawerElementFilter<(int, int)>();
        private GamePrefStorage _storage = null;
        private GUIStyle _objectFieldStyle = null;
        private GUIStyle _objectFieldButtonStyle = null;

        private GUIStyle ObjectFieldStyle => _objectFieldStyle ??= new GUIStyle("ObjectField");
        private GUIStyle ObjectFieldButtonStyle => _objectFieldButtonStyle ??= new GUIStyle("ObjectFieldButton");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => base.GetPropertyHeight(property, label);
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

            (FieldInfo, object) gamePrefInfoTuple = property.ExposePropertyInfo(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, out int arrayIndex);
            GamePref gamePref = (arrayIndex > -1 ? (gamePrefInfoTuple.Item1.GetValue(gamePrefInfoTuple.Item2)as IList)[arrayIndex] : gamePrefInfoTuple.Item1.GetValue(gamePrefInfoTuple.Item2)) as GamePref;
            GamePrefData gamePrefData = null;
            Event evt = Event.current;

            if (arrayIndex > -1 && label.text == gamePref.PersistentIdentifier)
                label.text = $"Element {arrayIndex}";

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            Rect buttonRect = new Rect(fieldRect.x + fieldRect.width - 1 - 18, fieldRect.y + 1, 18, 16);
            bool hasID = gamePref != null && _storage.HasID(gamePref.PersistentIdentifier, out gamePrefData);
            int controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            int currentIndex = -2, selectedIndex = -2;

            _menuSelections.EvaluateGUIEventChanges(evt);
            var output = _menuSelections.GetFilteredElement(property, (-2, -2));
            currentIndex = output.Item1;
            selectedIndex = output.Item2;

            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 0 && fieldRect.Contains(evt.mousePosition))
                    {
                        if (buttonRect.Contains(evt.mousePosition))
                        {
                            GenericMenu options = new GenericMenu();
                            options.AddItem(new GUIContent("None"), gamePref == null, () => _menuSelections.SetElement(property, (currentIndex, -1)));
                            if (sortedPrefs.Count > 0)
                                options.AddSeparator("");

                            sortedPrefs.Sort(GamePrefWindow.SortPairs);
                            for (int i = 0; i < sortedPrefs.Count; i++)
                            {
                                int index = i;
                                currentIndex = gamePref != null && gamePref.PersistentIdentifier == sortedPrefs[i].Value.PersistentIdentifier ? index : currentIndex;
                                options.AddItem(new GUIContent(sortedPrefs[i].Value.PrefKey), currentIndex == index, () => _menuSelections.SetElement(property, (currentIndex, index)));
                                _menuSelections.SetElement(property, (currentIndex, selectedIndex));
                            }
                            options.DropDown(fieldRect);
                            break;
                        }
                        if (hasID && evt.clickCount == 2)
                        {
                            EditorWindow window = EditorWindow.GetWindow<GamePrefWindow>();
                            window.titleContent = new GUIContent("GamePrefs");
                            PingLabel.Ping(window, new GUIContent(gamePrefData.PrefKey), true);
                            break;
                        }
                        evt.Use();
                        GUIUtility.keyboardControl = controlID;

                    }
                    break;
                case EventType.KeyDown:
                    break;
                case EventType.Repaint:
                    string labelText = gamePref != null ? hasID ? $"{gamePrefData.PrefKey} ({typeof(GamePref).Name})" : $"Missing ({typeof(GamePref).Name})" : $"None ({typeof(GamePref).Name})";
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
                    persistentIdentifierProp.stringValue = sortedPrefs[selectedIndex].Value.PersistentIdentifier;
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }
                _menuSelections.SetElement(property, (selectedIndex, selectedIndex));
            }
        }
    }
}
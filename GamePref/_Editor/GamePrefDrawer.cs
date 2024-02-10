using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.Serialization;
using UnitySimplified.RuntimeDatabases;

namespace UnitySimplifiedEditor.Serialization
{
    [CustomPropertyDrawer(typeof(GamePref))]
    public class GamePrefDrawer : PropertyDrawer
    {
        private readonly PropertyDrawerElementFilter<(int, int)> _menuSelections = new();
        private GamePrefStorage _database;
        private GUIStyle _objectFieldStyle = null;
        private GUIStyle _objectFieldButtonStyle = null;

        private GUIStyle ObjectFieldStyle => _objectFieldStyle ??= new GUIStyle("ObjectField");
        private GUIStyle ObjectFieldButtonStyle => _objectFieldButtonStyle ??= new GUIStyle("ObjectFieldButton");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => base.GetPropertyHeight(property, label);
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_database == null)
            {
                _database = GamePrefStorage.Instance;
                if (_database == null)
                    return;
                GamePrefWindow.onGamePrefsUpdated += GUIView.Current.Repaint;
            }

            var sortedPrefs = new List<KeyValuePair<string, GamePrefData>>(_database.GetGamePrefs());

            Event evt = Event.current;
            (FieldInfo, object) gamePrefInfoTuple = property.ExposePropertyInfo(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, out int arrayIndex);
            GamePref gamePref = (arrayIndex > -1 ? (gamePrefInfoTuple.Item1.GetValue(gamePrefInfoTuple.Item2)as IList)[arrayIndex] : gamePrefInfoTuple.Item1.GetValue(gamePrefInfoTuple.Item2)) as GamePref;
            GamePrefData gamePrefData = null;

            if (arrayIndex > -1 && label.text == gamePref.Identifier)
                label.text = $"Element {arrayIndex}";

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            Rect buttonRect = new Rect(fieldRect.x + fieldRect.width - 1 - 18, fieldRect.y + 1, 18, 16);
            bool hasIdentifier = gamePref != null && _database.TryGetFromIdentifier(gamePref.Identifier, out gamePrefData);
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
                        options.AddItem(new GUIContent("None"), gamePref == null, () => _menuSelections.SetElement(property, (currentIndex, -1)));
                        if (sortedPrefs.Count > 0)
                            options.AddSeparator("");

                        sortedPrefs.Sort(GamePrefWindow.SortPairs);
                        for (int i = 0; i < sortedPrefs.Count; i++)
                        {
                            int index = i;
                            currentIndex = gamePref != null && gamePref.Identifier == sortedPrefs[i].Value.Identifier ? index : currentIndex;
                            options.AddItem(new GUIContent(sortedPrefs[i].Value.Key), currentIndex == index, () => _menuSelections.SetElement(property, (currentIndex, index)));
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
                        window.PingPref(gamePrefData.Key);
                        break;
                    }
                    evt.Use();
                    GUIUtility.keyboardControl = controlIdentifier;

                    break;
                case EventType.KeyDown:
                    break;

                case EventType.Repaint:
                    string labelText = (gamePref != null ? hasIdentifier ? gamePrefData.Key : "Missing" : "None") + $" ({nameof(GamePref)})";
                    ObjectFieldStyle.Draw(fieldRect, new GUIContent(labelText), fieldRect.Contains(evt.mousePosition), false, false, controlIdentifier == GUIUtility.keyboardControl);
                    ObjectFieldButtonStyle.Draw(buttonRect, GUIContent.none, buttonRect.Contains(evt.mousePosition), false, false, false);
                    break;
            }

            if (selectedIndex > -2 && currentIndex != selectedIndex)
            {
                sortedPrefs.Sort(GamePrefWindow.SortPairs);
                SerializedProperty identifierProp = property.FindPropertyRelative("_identifier");

                if (selectedIndex == -1)
                {
                    identifierProp.stringValue = "";
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }
                else
                {
                    identifierProp.stringValue = sortedPrefs[selectedIndex].Value.Identifier;
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }
                _menuSelections.SetElement(property, (selectedIndex, selectedIndex));
            }
        }
    }
}
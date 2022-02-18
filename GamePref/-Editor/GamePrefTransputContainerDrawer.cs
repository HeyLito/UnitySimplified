#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnitySimplified.Serialization;

namespace UnitySimplifiedEditor.Serialization
{
    [CustomPropertyDrawer(typeof(GamePrefTransput.InfoContainer))]
    public class GamePrefsTransputContainerDrawer : PropertyDrawer
    {
        private const float _spacing = 2;
        private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int propCount = 1;
            float height = 0;
            if (property.isExpanded)
            {
                GamePrefData gamePrefData = null;
                GamePref gamePref = property.FindPropertyRelative("gamePref").ExposeProperty(_flags) as GamePref;
                bool hasID = gamePref != null && GamePrefStorage.Instance.HasID(gamePref.PersistentIdentifier, out gamePrefData);

                if (hasID)
                {
                    propCount += 2;
                    height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative($"on{GamePrefTransput.InfoContainer.TypeToTransputType(gamePrefData.GetPrefType())}ValueChanged"));
                }
                else
                {
                    propCount += 4;
                    height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative($"on{Enum.GetName(typeof(GamePrefTransput.TypeOfTransput), property.FindPropertyRelative("transputType").enumValueIndex)}ValueChanged"));
                }
            }
            height += EditorGUIUtility.singleLineHeight * propCount;
            return height + _spacing * propCount;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty gamePrefProp = property.FindPropertyRelative("gamePref");
            SerializedProperty gamePrefKeyProp = property.FindPropertyRelative("gamePrefKey");
            GamePref gamePref = gamePrefProp.ExposeProperty(_flags) as GamePref;
            GamePrefData gamePrefData = null;
            bool hasID = gamePref != null && GamePrefStorage.Instance.HasID(gamePref.PersistentIdentifier, out gamePrefData);
            bool isExpanded = property.isExpanded;

            Rect previous = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            label.text = hasID ? $"{gamePrefData.prefKey} ({typeof(GamePref).Name})" : !string.IsNullOrEmpty(gamePrefKeyProp.stringValue) ? $"{gamePrefKeyProp.stringValue} (string key)" : label.text;
            if (isExpanded = EditorGUI.Foldout(previous, isExpanded, label, true))
            {
                SerializedProperty loadOnProp = property.FindPropertyRelative("loadOn");
                SerializedProperty onValueChangedProp;

                Rect gamePrefRect = previous = new Rect(previous.x, previous.y + previous.height + _spacing, previous.width, EditorGUI.GetPropertyHeight(gamePrefKeyProp));

                EditorGUI.PropertyField(gamePrefRect, gamePrefProp);

                if (hasID)
                    onValueChangedProp = property.FindPropertyRelative($"on{GamePrefTransput.InfoContainer.TypeToTransputType(gamePrefData.GetPrefType())}ValueChanged");
                else
                {
                    SerializedProperty transputTypeProp = property.FindPropertyRelative("transputType");
                    onValueChangedProp = property.FindPropertyRelative($"on{Enum.GetName(typeof(GamePrefTransput.TypeOfTransput), transputTypeProp.enumValueIndex)}ValueChanged");

                    Rect gamePrefKeyRect = previous = new Rect(previous.x, previous.y + previous.height + _spacing, previous.width, EditorGUI.GetPropertyHeight(gamePrefKeyProp));
                    Rect transputTypeRect = previous = new Rect(previous.x, previous.y + previous.height + _spacing, previous.width, EditorGUI.GetPropertyHeight(transputTypeProp));

                    EditorGUI.PropertyField(gamePrefKeyRect, gamePrefKeyProp);
                    EditorGUI.PropertyField(transputTypeRect, transputTypeProp);
                }

                Rect loadOnRect = previous = new Rect(previous.x, previous.y + previous.height + _spacing, previous.width, EditorGUI.GetPropertyHeight(loadOnProp));
                Rect onValueChangedRect = previous = new Rect(previous.x, previous.y + previous.height + _spacing, previous.width, EditorGUI.GetPropertyHeight(onValueChangedProp));

                EditorGUI.PropertyField(onValueChangedRect, onValueChangedProp);
                EditorGUI.PropertyField(loadOnRect, loadOnProp);
            }

            if (isExpanded != property.isExpanded)
            {
                property.isExpanded = isExpanded;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

#endif
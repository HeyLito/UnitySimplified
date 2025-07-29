using UnityEngine;

#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnitySimplified;
#endif

namespace UnitySimplified
{
    public class SerializeReferenceMenuAttribute : PropertyAttribute { }
}

#if UNITY_EDITOR
namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(SerializeReferenceMenuAttribute))]
    public class SerializeReferenceMenuDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label, true);
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect headerRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };

            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && headerRect.Contains(Event.current.mousePosition))
            {
                GenericMenu genericMenu = new GenericMenu();
                foreach (var option in GetOptions(property))
                    genericMenu.AddItem(option.content, false, option.function);
                genericMenu.ShowAsContext();
            }

            EditorGUI.PropertyField(position, property, true);
        }

        private IEnumerable<(GUIContent content, GenericMenu.MenuFunction function)> GetOptions(SerializedProperty property)
        {
            yield return (new GUIContent("Set as NULL"), () => { property.managedReferenceValue = null; property.serializedObject.ApplyModifiedProperties(); });

            var fieldType = fieldInfo.FieldType;
            var finalType = fieldType.IsGenericType ? fieldType.GenericTypeArguments.First() : fieldType.IsArray ? fieldType.GetElementType() : fieldType;
            if (finalType == null)
                yield break;
            var unityObjectType = typeof(UnityEngine.Object);

            foreach (var assembly in ApplicationUtility.GetAssemblies())
            {
                foreach (var type in ApplicationUtility.GetTypesFromAssembly(assembly))
                {
                    if (!finalType.IsAssignableFrom(type))
                        continue;
                    if (finalType == type)
                        continue;
                    if (unityObjectType.IsAssignableFrom(type))
                        continue;
                    yield return (new GUIContent($"Set as {type.Name}"), () =>
                    {
                        property.managedReferenceValue = Activator.CreateInstance(type);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }
        }
    }
}
#endif
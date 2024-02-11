#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnitySimplified.SpriteAnimator;

[CustomPropertyDrawer(typeof(SpriteAnimation), true)]
public class SpriteAnimationDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var targetDepth = property.depth + 1;
        float totalHeight = 0;


        var propertyIterator = property.GetEnumerator();
        while (propertyIterator.MoveNext())
        {
            var currentProp = (SerializedProperty)propertyIterator.Current;
            if (currentProp == null || currentProp.depth != targetDepth)
                continue;

            totalHeight += 2;
            totalHeight += EditorGUI.GetPropertyHeight(currentProp);
        }
        if (propertyIterator is IDisposable disposable)
            disposable.Dispose();
        return totalHeight;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var targetDepth = property.depth + 1;
        var previousRect = new Rect(position) { height = 0 };


        var propertyIterator = property.GetEnumerator();
        while (propertyIterator.MoveNext())
        {
            var currentProp = (SerializedProperty)propertyIterator.Current;
            if (currentProp == null || currentProp.depth != targetDepth)
                continue;

            previousRect = new Rect(previousRect) { y = previousRect.yMax + 2, height = EditorGUI.GetPropertyHeight(currentProp) };
            EditorGUI.PropertyField(previousRect, currentProp);
        }
        if (propertyIterator is IDisposable disposable)
            disposable.Dispose();
    }
}

#endif
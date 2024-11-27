using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnitySimplified.Audio;
#endif

namespace UnitySimplified.Audio
{
    [Serializable]
    public struct RangedFloat
    {
        public float min;
        public float max;

        public RangedFloat(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CustomRange : Attribute
    {
        public CustomRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
        public float Min { get; private set; }
        public float Max { get; private set; }
    }
}

#if UNITY_EDITOR
namespace UnitySimplifiedEditor.Audio
{
    [CustomPropertyDrawer(typeof(RangedFloat))]
    class RangedFloatDrawer : PropertyDrawer
    {
        private const float _labelWidth = 40f;
        private float _min;
        private float _max;
        private float _minRange;
        private float _maxRange;
        private SerializedProperty _minProp;
        private SerializedProperty _maxProp;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            _minProp = property.FindPropertyRelative("min");
            _maxProp = property.FindPropertyRelative("max");

            _min = _minProp.floatValue;
            _max = _maxProp.floatValue;

            var range = (CustomRange)fieldInfo.GetCustomAttributes(typeof(CustomRange), true).FirstOrDefault();
            if (range != null)
            {
                _minRange = range.Min;
                _maxRange = range.Max;
            }
            else
            {
                _minRange = 0;
                _maxRange = 1;
            }

            var rangeBoundsLabel1Rect = new Rect(position);
            rangeBoundsLabel1Rect.width = _labelWidth;
            GUI.Label(rangeBoundsLabel1Rect, new GUIContent(_min.ToString("F2")));
            position.xMin += _labelWidth;

            var rangeBoundsLabel2Rect = new Rect(position);
            rangeBoundsLabel2Rect.xMin = rangeBoundsLabel2Rect.xMax - _labelWidth;
            GUI.Label(rangeBoundsLabel2Rect, new GUIContent(_max.ToString("F2")));
            position.xMax -= _labelWidth;

            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(position, ref _min, ref _max, _minRange, _maxRange);
            if (EditorGUI.EndChangeCheck())
            {
                _minProp.floatValue = _min;
                _maxProp.floatValue = _max;
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
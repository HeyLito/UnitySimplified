#if UNITY_EDITOR

using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified;

namespace UnitySimplifiedEditor
{
    [CustomPropertyDrawer(typeof(VisualStatement.Condition))]
    public class ConditionDrawer: PropertyDrawer
    {
        #region FIELDS
        private readonly RectOffset _border = new RectOffset(7, 7, 8, 12);
        private readonly Dictionary<string, ValueTuple<int, bool, SerializedProperty>> _validTargets = new Dictionary<string, (int, bool, SerializedProperty)>();
        private readonly int _spacing = 4;
        private bool _dragged = false;
        private bool _up = false;
        #endregion

        #region METHODS
        #region PROPERTY_DRAWER
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {   return base.GetPropertyHeight(property, label) * 3 + _border.top + _border.bottom + 12;   }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            VisualStatement.Condition condition = property.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic) as VisualStatement.Condition;
            Event evt = Event.current;
            if (condition == null)
                return;
            switch (evt.type)
            {
                case EventType.MouseDrag:
                    _dragged = true;
                    break;

                case EventType.MouseUp:
                    if (_dragged)
                        _up = true;
                    else _dragged = _up = false;
                    break;

                case EventType.Repaint:
                    if (_up)
                    {
                        _validTargets.Clear();
                        _dragged = _up = false;
                    }
                    break;
            }

            var lhsProp = property.FindPropertyRelative("lhs");
            var rhsProp = property.FindPropertyRelative("rhs");
            var operatorProp = property.FindPropertyRelative("relationalOperator");
            var lhsValueReferenceType = (VisualStatement.Operand.ReferenceType)lhsProp.FindPropertyRelative("referenceType").enumValueIndex;
            var lhsValuePath = lhsProp.FindPropertyRelative("valuePath").stringValue;
            var rhsValueReferenceType = (VisualStatement.Operand.ReferenceType)rhsProp.FindPropertyRelative("referenceType").enumValueIndex;
            var rhsValuePath = rhsProp.FindPropertyRelative("valuePath").stringValue;
            bool isValid = CheckIfTargetIsValid(property);
            int controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            int indent = EditorGUI.indentLevel;
            float operatorWidth = isValid ? position.width * 0.05f + 35 : 0;
            float labelWidth = (position.width - operatorWidth - _spacing) / 2;
            string[] operators = GetOperatorAsStrings(condition, operatorProp, isValid);

            Rect boxHeaderRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + 2);
            Rect boxBgRect = new Rect(boxHeaderRect.x, boxHeaderRect.y + boxHeaderRect.height, boxHeaderRect.width, EditorGUIUtility.singleLineHeight + boxHeaderRect.height + _border.top + _border.bottom);
            Rect innerBoxBgRect = new Rect(boxBgRect.x + 1, boxBgRect.y + 5, boxBgRect.width - 2, boxBgRect.height - 10);
            Rect lhsRect = new Rect(boxBgRect.x + _border.left, boxBgRect.y + _border.top, labelWidth - _border.left - _spacing / 2, EditorGUIUtility.singleLineHeight * 2);
            Rect operatorRect = new Rect(boxBgRect.x + labelWidth, boxBgRect.y + boxBgRect.height / 4, operatorWidth, EditorGUIUtility.singleLineHeight);
            Rect rhsRect = new Rect(boxBgRect.x + _spacing / 2 + labelWidth + operatorWidth, boxBgRect.y + _border.top, labelWidth - _border.left, EditorGUIUtility.singleLineHeight * 2);
            
            GUIStyle boxHeaderStyle = new GUIStyle("RL Header");
            GUIStyle boxBgStyle = new GUIStyle("RL Background");
            GUIStyle elementBgStyle = new GUIStyle("RL Element");
            GUIStyle operatorStyle = new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter };

            EditorGUI.indentLevel = 0;
            EditorGUI.BeginProperty(boxHeaderRect, GUIContent.none, property);
            GUI.Box(boxHeaderRect, GUIContent.none, boxHeaderStyle);
            EditorGUI.EndProperty();
            GUI.Box(boxBgRect, GUIContent.none, boxBgStyle);
            switch (evt.type) 
            {
                case EventType.Repaint:
                    Color color = GUI.backgroundColor;
                    GUI.backgroundColor = !isValid ? GUIUtility.keyboardControl == controlID ? new Color(1.75f, 0.01f, 0.01f, 1f) : new Color(1f, 0.65f, 0.65f, 1f) : color;
                    elementBgStyle.Draw(innerBoxBgRect, false, GUIUtility.keyboardControl == controlID, true, true);
                    GUI.backgroundColor = color;
                    break;
            }
            EditorGUI.HandlePrefixLabel(boxBgRect, new Rect(boxHeaderRect.x + 8, boxHeaderRect.y, boxHeaderRect.width, boxHeaderRect.height), new GUIContent(property.displayName));

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(lhsRect, lhsProp);
            if (EditorGUI.EndChangeCheck()/* || Event.current.commandName == PopupInfo.commandMessage*/)
            {
                HandleOperandChange(lhsProp, lhsValueReferenceType, lhsValuePath, rhsProp, rhsValueReferenceType);
                InitializeTargetValidation(property);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(operatorRect, GUIContent.none, operatorProp);
            int popupIndex = 0;
            if (operatorWidth > 0)
                popupIndex = EditorGUI.Popup(operatorRect, operatorProp.enumValueIndex < operators.Length ? operatorProp.enumValueIndex : 0, operators, operatorStyle);
            if (EditorGUI.EndChangeCheck() || popupIndex != operatorProp.enumValueIndex)
            {
                operatorProp.enumValueIndex = popupIndex;
                InitializeTargetValidation(property);
            }
            EditorGUI.EndProperty();

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rhsRect, rhsProp);
            if (EditorGUI.EndChangeCheck()/* || Event.current.commandName == PopupInfo.commandMessage*/)
            {
                HandleOperandChange(rhsProp, rhsValueReferenceType, rhsValuePath, lhsProp, lhsValueReferenceType);
                InitializeTargetValidation(property);
            }

            if (evt.type == EventType.MouseDown)
            {
                if (evt.button == 0 && innerBoxBgRect.Contains(evt.mousePosition))
                {
                    GUIUtility.keyboardControl = controlID;
                    evt.Use();
                }
            }
            EditorGUI.indentLevel = indent;
        }
        #endregion

        private bool CheckIfTargetIsValid(SerializedProperty property)
        {
            //if (_validTargets.TryGetValue(property.propertyPath, out (bool, bool, SerializedProperty) value))
            //    return value.Item1;
            //else return InitializeTargetValidation(property);

            bool removeUnused = false;
            bool valid;
            if (_validTargets.TryGetValue(property.propertyPath, out (int, bool, SerializedProperty) tuple))
            {
                if (tuple.Item1 > 1)
                    removeUnused = true;
                _validTargets[property.propertyPath] = (tuple.Item1 + 1, tuple.Item2, tuple.Item3);
                valid = tuple.Item2;
            }
            else valid = InitializeTargetValidation(property);

            if (removeUnused)
            {
                List<string> unused = new List<string>();
                List<string> used = new List<string>();
                foreach (var pair in _validTargets)
                {
                    if (pair.Value.Item1 == 0)
                        unused.Add(pair.Key);
                    else used.Add(pair.Key);
                }

                for (int i = 0; i < unused.Count; i++)
                    _validTargets.Remove(unused[i]);
                for (int i = 0; i < used.Count; i++)
                {
                    ValueTuple<int, bool, SerializedProperty> indexedTuple = _validTargets[used[i]];
                    _validTargets[used[i]] = (0, indexedTuple.Item2, indexedTuple.Item3);
                }
            }
            return valid;
        }
        private bool InitializeTargetValidation(SerializedProperty property)
        {
            bool valid = (property.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic) as VisualStatement.Condition).IsValid();
            _validTargets[property.propertyPath] = (0, valid, property);
            return valid;
        }
        private void HandleOperandChange(SerializedProperty targetProp, VisualStatement.Operand.ReferenceType targetRefType, string targetOldPath, SerializedProperty otherProp, VisualStatement.Operand.ReferenceType otherRefType) 
        {
            var targetOperand = targetProp.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic) as VisualStatement.Operand;
            var otherOperand = otherProp.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic) as VisualStatement.Operand;
            
            // Set the other operand's values to default if the target operand has a new member path.
            if (otherRefType == VisualStatement.Operand.ReferenceType.Value && targetOldPath != targetProp.FindPropertyRelative("valuePath").stringValue)
            {
                otherProp.serializedObject.ApplyModifiedProperties();
                otherProp.serializedObject.Update();
                otherOperand.Intialize(null, targetOperand.ValueType);
                EditorUtility.SetDirty(otherProp.serializedObject.targetObject);
                return;
            }

            var targetUpdatedRefType = (VisualStatement.Operand.ReferenceType)targetProp.FindPropertyRelative("referenceType").enumValueIndex;
            // Check if the target-operand's reference was changed.
            if (targetUpdatedRefType != targetRefType)
            {
                // Set target-operand's values if it is a ValueType and the other operand is a FieldType
                if (targetUpdatedRefType == VisualStatement.Operand.ReferenceType.Value && otherRefType == VisualStatement.Operand.ReferenceType.Field)
                {
                    targetProp.serializedObject.ApplyModifiedProperties();
                    targetProp.serializedObject.Update();
                    targetOperand.Intialize(null, otherOperand.ValueType);
                    EditorUtility.SetDirty(targetProp.serializedObject.targetObject);
                    return;
                }
                // Set the other operand's values if it is a ValueType and the target operand is a FieldType
                if (otherRefType == VisualStatement.Operand.ReferenceType.Value && targetUpdatedRefType == VisualStatement.Operand.ReferenceType.Field)
                {
                    otherProp.serializedObject.ApplyModifiedProperties();
                    otherProp.serializedObject.Update();
                    otherOperand.Intialize(null, targetOperand.ValueType);
                    EditorUtility.SetDirty(otherProp.serializedObject.targetObject);
                    return;
                }

                // Reset the other operand's value to default if previous if-checks were unsuccessful.
                if (otherRefType == VisualStatement.Operand.ReferenceType.Value)
                {
                    otherProp.serializedObject.ApplyModifiedProperties();
                    otherProp.serializedObject.Update();
                    otherOperand.Intialize(null, null);
                    EditorUtility.SetDirty(otherProp.serializedObject.targetObject);
                }
            }
        }
        private string[] GetOperatorAsStrings(VisualStatement.Condition condition, SerializedProperty operatorProperty, bool isValid)
        {
            List<string> operators = new List<string>();
            var previous = operatorProperty.enumValueIndex;
            operatorProperty.enumValueIndex = (int)default(VisualStatement.Condition.RelationalOperator);
            if (isValid)
            {
                foreach (var enumValue in Enum.GetValues(typeof(VisualStatement.Condition.RelationalOperator)))
                {
                    if (condition.AcceptsOperator((VisualStatement.Condition.RelationalOperator)enumValue))
                        switch ((VisualStatement.Condition.RelationalOperator)enumValue)
                        {
                            case VisualStatement.Condition.RelationalOperator.EqualTo:
                                operators.Add("==");
                                break;
                            case VisualStatement.Condition.RelationalOperator.NotEqualTo:
                                operators.Add("!=");
                                break;
                            case VisualStatement.Condition.RelationalOperator.GreaterThan:
                                operators.Add(">");
                                break;
                            case VisualStatement.Condition.RelationalOperator.GreaterThanOrEqualTo:
                                operators.Add(">=");
                                break;
                            case VisualStatement.Condition.RelationalOperator.LessThan:
                                operators.Add("<");
                                break;
                            case VisualStatement.Condition.RelationalOperator.LessThanOrEqualTo:
                                operators.Add("<=");
                                break;
                        }
                }
            }
            operatorProperty.enumValueIndex = previous;
            return operators.ToArray();
        }
        #endregion
    }
}

#endif
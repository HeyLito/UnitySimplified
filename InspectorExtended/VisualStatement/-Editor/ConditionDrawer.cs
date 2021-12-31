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
        private RectOffset _border = new RectOffset(7, 7, 8, 12);
        private int _spacing = 4;
        #endregion

        #region METHODS
        #region PROPERTY_DRAWER
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {   return base.GetPropertyHeight(property, label) * 3 + _border.top + _border.bottom + 12;   }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var condition = property.ExposeProperty(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) as VisualStatement.Condition;
            if (condition == null)
                return;
            var lhsProp = property.FindPropertyRelative("lhs");
            var rhsProp = property.FindPropertyRelative("rhs");
            var operatorProp = property.FindPropertyRelative("relationalOperator");
            var lhsValueReferenceType = (VisualStatement.Operand.ReferenceType)lhsProp.FindPropertyRelative("referenceType").enumValueIndex;
            var lhsValuePath = lhsProp.FindPropertyRelative("valuePath").stringValue;
            var rhsValueReferenceType = (VisualStatement.Operand.ReferenceType)rhsProp.FindPropertyRelative("referenceType").enumValueIndex;
            var rhsValuePath = rhsProp.FindPropertyRelative("valuePath").stringValue;

            GUIStyle boxHeaderStyle = new GUIStyle("RL Header");
            GUIStyle boxBgStyle = new GUIStyle("RL Background");
            GUIStyle elementBgStyle = new GUIStyle("RL Element");
            GUIStyle operatorStyle = new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter };
            
            string[] operators = GetOperatorAsStrings(condition, operatorProp);

            int controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            int indent = EditorGUI.indentLevel;
            float operatorWidth = condition.IsValid() ? position.width * 0.05f + 35 : 0;
            float labelWidth = (position.width - operatorWidth - _spacing) / 2;

            Rect boxHeaderRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + 2);
            Rect boxBgRect = new Rect(boxHeaderRect.x, boxHeaderRect.y + boxHeaderRect.height, boxHeaderRect.width, EditorGUIUtility.singleLineHeight + boxHeaderRect.height + _border.top + _border.bottom);
            Rect innerBoxBgRect = new Rect(boxBgRect.x + 1, boxBgRect.y + 5, boxBgRect.width - 2, boxBgRect.height - 10);
            Rect lhsRect = new Rect(boxBgRect.x + _border.left, boxBgRect.y + _border.top, labelWidth - _border.left - _spacing / 2, EditorGUIUtility.singleLineHeight * 2);
            Rect operatorRect = new Rect(boxBgRect.x + labelWidth, boxBgRect.y + boxBgRect.height / 4, operatorWidth, EditorGUIUtility.singleLineHeight);
            Rect rhsRect = new Rect(boxBgRect.x + _spacing / 2 + labelWidth + operatorWidth, boxBgRect.y + _border.top, labelWidth - _border.left, EditorGUIUtility.singleLineHeight * 2);

            EditorGUI.indentLevel = 0;
            EditorGUI.BeginProperty(boxHeaderRect, GUIContent.none, property);
            GUI.Box(boxHeaderRect, GUIContent.none, boxHeaderStyle);
            EditorGUI.EndProperty();
            GUI.Box(boxBgRect, GUIContent.none, boxBgStyle);
            switch (Event.current.type) 
            {
                case EventType.Repaint:
                    Color color = GUI.backgroundColor;
                    GUI.backgroundColor = !condition.IsValid() ? GUIUtility.keyboardControl == controlID ? new Color(1.75f, 0.01f, 0.01f, 1f) : new Color(1f, 0.65f, 0.65f, 1f) : color;
                    elementBgStyle.Draw(innerBoxBgRect, false, GUIUtility.keyboardControl == controlID, true, true);
                    GUI.backgroundColor = color;
                    break;
            }
            EditorGUI.HandlePrefixLabel(boxBgRect, new Rect(boxHeaderRect.x + 8, boxHeaderRect.y, boxHeaderRect.width, boxHeaderRect.height), new GUIContent(property.displayName));

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(lhsRect, lhsProp);
            if (EditorGUI.EndChangeCheck() || Event.current.commandName == PopupInfo.commandMessage)
                HandleOperandChange((lhsProp, lhsValueReferenceType, lhsValuePath), rhsProp);

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(operatorRect, GUIContent.none, operatorProp);
            int popupIndex = 0;
            if (operatorWidth > 0)
                popupIndex = EditorGUI.Popup(operatorRect, operatorProp.enumValueIndex < operators.Length ? operatorProp.enumValueIndex : 0, operators, operatorStyle);
            if (EditorGUI.EndChangeCheck() || popupIndex != operatorProp.enumValueIndex)
            {
                operatorProp.enumValueIndex = popupIndex;
                operatorProp.serializedObject.ApplyModifiedProperties();
                operatorProp.serializedObject.Update();
            }
            EditorGUI.EndProperty();

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rhsRect, rhsProp);
            if (EditorGUI.EndChangeCheck() || Event.current.commandName == PopupInfo.commandMessage)
                HandleOperandChange((rhsProp, rhsValueReferenceType, rhsValuePath), lhsProp);

            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0 && innerBoxBgRect.Contains(Event.current.mousePosition))
                {
                    GUIUtility.keyboardControl = controlID;
                    Event.current.Use();
                }
            }
            EditorGUI.indentLevel = indent;
        }
        #endregion

        private void HandleOperandChange(ValueTuple<SerializedProperty, VisualStatement.Operand.ReferenceType, string> target, SerializedProperty other) 
        {
            var targetOperand = target.Item1.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic) as VisualStatement.Operand;
            var otherOperand = other.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic) as VisualStatement.Operand;
            if (target.Item3 != target.Item1.FindPropertyRelative("valuePath").stringValue)
            {
                other.serializedObject.ApplyModifiedProperties();
                other.serializedObject.Update();
                otherOperand.Intialize(null, targetOperand.ValueType);
                EditorUtility.SetDirty(other.serializedObject.targetObject);
                return;
            }
            var targetReferenceType = (VisualStatement.Operand.ReferenceType)target.Item1.FindPropertyRelative("referenceType").enumValueIndex;
            var otherReferenceType = (VisualStatement.Operand.ReferenceType)other.FindPropertyRelative("referenceType").enumValueIndex;
            if (targetReferenceType != target.Item2)
            {
                if (targetReferenceType == VisualStatement.Operand.ReferenceType.Value && otherReferenceType == VisualStatement.Operand.ReferenceType.Field)
                {
                    targetOperand.Intialize(null, otherOperand.ValueType);
                    EditorUtility.SetDirty(target.Item1.serializedObject.targetObject);
                }
                else if (otherReferenceType == VisualStatement.Operand.ReferenceType.Value && targetReferenceType == VisualStatement.Operand.ReferenceType.Field)
                {
                    otherOperand.Intialize(null, targetOperand.ValueType);
                    EditorUtility.SetDirty(other.serializedObject.targetObject);
                }
                return;
            }
        }
        private string[] GetOperatorAsStrings(VisualStatement.Condition condition, SerializedProperty operatorProperty)
        {
            List<string> operators = new List<string>();
            var previous = operatorProperty.enumValueIndex;
            operatorProperty.enumValueIndex = (int)default(VisualStatement.Condition.RelationalOperator);
            if (condition.IsValid())
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
            operatorProperty.enumValueIndex = previous;
            return operators.ToArray();
        }
        #endregion
    }
}

#endif
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnitySimplified;

namespace UnitySimplifiedEditor
{
    [CustomPropertyDrawer(typeof(VisualStatement))]
    public class VisualStatementDrawer : PropertyDrawer
    {
        private class VisualStatementElement
        {
            public enum ElementType { Condition, LogicalOperator }
            public ElementType elementType;
            public SerializedProperty property;
            public VisualStatement.Condition condition;
            public VisualStatement.LogicalOperator logicalOperator;

            public VisualStatementElement(SerializedProperty property, VisualStatement.Condition condition)
            {
                elementType = ElementType.Condition;
                this.property = property;
                this.condition = condition;
            }
            public VisualStatementElement(SerializedProperty property, VisualStatement.LogicalOperator logicalOperator)
            {
                elementType = ElementType.LogicalOperator;
                this.property = property;
                this.logicalOperator = logicalOperator;
            }
        }

        #region FIELDS
        private readonly PropertyDrawerElementFilter<ReorderableList> _reorderableLists = new PropertyDrawerElementFilter<ReorderableList>();
        private readonly Color _guiColorIntensityDiff = new Color(0.15f, 0.15f, 0.125f, 0);
        private readonly int _conditionHeight = 100;
        private readonly int _logicalOperatorHeight = 20;
        private ReorderableList _targetList = null;
        private SerializedProperty _prop;
        private Color _guiColor = new Color();
        #endregion

        #region METHODS
        #region PROPERTY_DRAWER
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            VisualStatement visualStatement = property.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic) as VisualStatement;
            float height = visualStatement != null && visualStatement.ConditionsCount > 0 ? (_conditionHeight + _logicalOperatorHeight + 4) * visualStatement.ConditionsCount + 9 : EditorGUIUtility.singleLineHeight * 2 + 16;

            return EditorGUIUtility.singleLineHeight + height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            _prop = property;

            _reorderableLists.EvaluateGUIEventChanges(evt);
            _reorderableLists.InvokeActionIfChanged(() => ValidatePropertyArrays(_prop));
            _targetList = _reorderableLists.GetFilteredElement(_prop, RecontructList(_prop));
            if (_targetList != null)
            {
                _targetList.DoList(position);
                if (evt.GetTypeForControl(controlID) == EventType.MouseDown)
                {
                    if (_targetList.displayRemove && evt.button == 0 && !new Rect(position.x, position.y, position.width, position.height - _targetList.footerHeight).Contains(evt.mousePosition))
                    {
                        _targetList.displayRemove = _targetList.draggable = false;
                        GUIUtility.keyboardControl = 0;
                        GUI.changed = true;
                    }
                }

            }
        }
        #endregion

        private ReorderableList RecontructList(SerializedProperty property)
        {
            List<VisualStatementElement> visualElementList = CreateListOfElements(property);
            var list = new ReorderableList(visualElementList, typeof(VisualStatementElement), false, true, true, false)
            {
                elementHeightCallback = ElementHeightCallback,
                onReorderCallbackWithDetails = ReorderCallback,
                onSelectCallback = SelectCallback,
                onAddCallback = AddCallback,
                onRemoveCallback = RemoveCallback,
                drawElementCallback = DrawListElementCallback,
                drawHeaderCallback = DrawHeaderCallback
            };
            return list;
        }
        private string[] GetOperatorsAsStrings()
        {
            List<string> operators = new List<string>();
            foreach (var enumValue in Enum.GetValues(typeof(VisualStatement.LogicalOperator)))
                operators.Add(enumValue.ToString());
            return operators.ToArray();
        }
        private void ValidatePropertyArrays(SerializedProperty property)
        {
            var conditionsProp = property.FindPropertyRelative("conditions");
            var logicalOperatorsProp = property.FindPropertyRelative("logicalOperators");

            if (logicalOperatorsProp.arraySize != conditionsProp.arraySize - 1 && conditionsProp.arraySize > 0)
            {
                logicalOperatorsProp.arraySize = conditionsProp.arraySize - 1;
                logicalOperatorsProp.serializedObject.ApplyModifiedProperties();
                logicalOperatorsProp.serializedObject.Update();
            }
            _reorderableLists.SetElement(property, RecontructList(property));
        }

        private void HandleReorderSwap(SerializedProperty property, ReorderableList list, int oldIndex, int newIndex)
        {
            var visualStatement = property.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic) as VisualStatement;
            var arrayInfo = visualStatement.GetType().GetField("conditions", VisualStatementUtility.flags | BindingFlags.NonPublic);
            var arrayList = arrayInfo.GetValue(visualStatement) as IList;
            int conditionsOldIndex = visualStatement.ConditionsCount - 1 - (list.count - (1 + oldIndex)) / 2;
            int conditionsNewIndex = visualStatement.ConditionsCount - 1 - (list.count - (1 + newIndex)) / 2;
            var oldIndexedValue = arrayList[conditionsOldIndex];
            var newIndexedValue = arrayList[conditionsNewIndex];
            arrayList[conditionsOldIndex] = newIndexedValue;
            arrayList[conditionsNewIndex] = oldIndexedValue;
            arrayInfo.SetValue(visualStatement, arrayList);
        }
        private void HandleReorderMove(SerializedProperty property, ReorderableList list, int oldIndex, int newIndex)
        {
            var visualStatement = property.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic) as VisualStatement;
            var arrayInfo = visualStatement.GetType().GetField("conditions", VisualStatementUtility.flags | BindingFlags.NonPublic);
            var arrayList = arrayInfo.GetValue(visualStatement) as IList;
            int conditionsOldIndex = visualStatement.ConditionsCount - 1 - (list.count - (1 + oldIndex)) / 2;
            int conditionsNewIndex = visualStatement.ConditionsCount - 1 - (list.count - (1 + newIndex)) / 2;
            var oldIndexedValue = arrayList[conditionsOldIndex];
            if (oldIndex < newIndex)
                for (int i = conditionsOldIndex; i < conditionsNewIndex; i++)
                    arrayList[i] = arrayList[i + 1];
            else for (int i = conditionsOldIndex; i > conditionsNewIndex; i--)
                    arrayList[i] = arrayList[i - 1];
            arrayList[conditionsNewIndex] = oldIndexedValue;
            arrayInfo.SetValue(visualStatement, arrayList);
        }

        private List<VisualStatementElement> CreateListOfElements(SerializedProperty serializedProperty)
        {
            List<VisualStatementElement> list = new List<VisualStatementElement>();
            VisualStatementElement.ElementType next = VisualStatementElement.ElementType.Condition;
            var conditionsProp = serializedProperty.FindPropertyRelative("conditions");
            var logicalOperatorsProp = serializedProperty.FindPropertyRelative("logicalOperators");

            for (int i = 0, a = 0, b = 0; i < conditionsProp.arraySize + logicalOperatorsProp.arraySize && a <= conditionsProp.arraySize && b <= logicalOperatorsProp.arraySize; i++)
            {
                switch (next)
                {
                    case VisualStatementElement.ElementType.Condition:
                        var conditionsIndexedProp = conditionsProp.GetArrayElementAtIndex(a);
                        list.Add(new VisualStatementElement(conditionsIndexedProp, (VisualStatement.Condition)conditionsIndexedProp.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic)));

                        next = VisualStatementElement.ElementType.LogicalOperator;
                        a++;
                        break;

                    case VisualStatementElement.ElementType.LogicalOperator:
                        var logicalOperatorsIndexedProp = logicalOperatorsProp.GetArrayElementAtIndex(b);
                        list.Add(new VisualStatementElement(logicalOperatorsIndexedProp, (VisualStatement.LogicalOperator)logicalOperatorsIndexedProp.ExposeProperty(VisualStatementUtility.flags | BindingFlags.NonPublic)));

                        next = VisualStatementElement.ElementType.Condition;
                        b++;
                        break;

                    default:
                        return null;
                }
            }

            return list;
        }

        #region CALLBACKS
        private float ElementHeightCallback(int index)
        {
            var element = _targetList.list[index] as VisualStatementElement;
            switch (element.elementType)
            {
                case VisualStatementElement.ElementType.Condition:
                    return _conditionHeight;
                case VisualStatementElement.ElementType.LogicalOperator:
                    return _logicalOperatorHeight;
            }
            return 0;
        }
        private void ReorderCallback(ReorderableList list, int oldIndex, int newIndex)
        {
            if (_prop == null || list.index >= list.count || (list.list[list.index] as VisualStatementElement).elementType == VisualStatementElement.ElementType.LogicalOperator)
                return;

            HandleReorderMove(_prop, list, oldIndex, newIndex);
            _prop.serializedObject.ApplyModifiedProperties();
            _prop.serializedObject.Update();
            EditorUtility.SetDirty(_prop.serializedObject.targetObject);

            _reorderableLists.SetElement(_prop, RecontructList(_prop));
        }
        private void SelectCallback(ReorderableList list)
        {
            var element = list.list[list.index] as VisualStatementElement;
            switch (element.elementType)
            {
                case VisualStatementElement.ElementType.Condition:
                    list.draggable = list.displayRemove = true;
                    break;

                case VisualStatementElement.ElementType.LogicalOperator:
                    list.draggable = list.displayRemove = false;
                    break;
            }
        }
        private void AddCallback(ReorderableList list)
        {
            if (_prop == null)
                return;

            var conditionsProp = _prop.FindPropertyRelative("conditions");
            var logicalOperatorsProp = _prop.FindPropertyRelative("logicalOperators");

            conditionsProp.arraySize++;
            if (conditionsProp.arraySize > 1)
                logicalOperatorsProp.arraySize++;
            else conditionsProp.GetArrayElementAtIndex(conditionsProp.arraySize - 1).FindPropertyRelative("lhs").FindPropertyRelative("referenceType").enumValueIndex = (int)VisualStatement.Operand.ReferenceType.Field;
            _prop.serializedObject.ApplyModifiedProperties();
            _prop.serializedObject.Update();

            _reorderableLists.SetElement(_prop, RecontructList(_prop));
        }
        private void RemoveCallback(ReorderableList list)
        {
            if (_prop == null || list.index >= list.count ||(list.list[list.index] as VisualStatementElement).elementType == VisualStatementElement.ElementType.LogicalOperator)
                return;
            
            var conditionsProp = _prop.FindPropertyRelative("conditions");
            var logicalOperatorsProp = _prop.FindPropertyRelative("logicalOperators");

            if (list.count > 0)
            {
                if (list.count >= 3)
                {
                    if (list.index == 0)
                    {
                        conditionsProp.DeleteArrayElementAtIndex(0);
                        logicalOperatorsProp.DeleteArrayElementAtIndex(0);
                    }
                    else
                    {
                        conditionsProp.DeleteArrayElementAtIndex(conditionsProp.arraySize - 1 - (list.count - (1 + list.index)) / 2);
                        logicalOperatorsProp.DeleteArrayElementAtIndex(logicalOperatorsProp.arraySize - 1 - (list.count - (1 + list.index)) / 2);
                    }
                }
                else if (list.index == 0)
                    conditionsProp.DeleteArrayElementAtIndex(0);

                _prop.serializedObject.ApplyModifiedProperties();
                _prop.serializedObject.Update();

                _reorderableLists.RemoveElement(_prop);
            }

        }
        private void DrawHeaderCallback(Rect rect)
        {
            int indent = EditorGUI.indentLevel;
            float extraSpacing = 2;
            Rect enabledRect = new Rect(rect.x, rect.y, 14, rect.height);
            Rect labelRect = new Rect(enabledRect.x + enabledRect.width + extraSpacing, enabledRect.y, rect.width - enabledRect.width - extraSpacing, rect.height);
            EditorGUI.indentLevel = 0;
            EditorGUI.PropertyField(enabledRect, _prop.FindPropertyRelative("enabled"), GUIContent.none);
            EditorGUI.BeginProperty(labelRect, GUIContent.none, _prop);
            EditorGUI.LabelField(labelRect, _prop != null ? _prop.displayName : "");
            EditorGUI.EndProperty();
            EditorGUI.indentLevel = indent;
        }
        private void DrawListElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_prop == null || _targetList == null)
                return;

            VisualStatementElement element = _targetList.list[index] as VisualStatementElement;
            Event evt = Event.current;
            float operatorWidth = rect.width * 0.30f;
            float remainderWidth = (rect.width - operatorWidth) / 2;

            Rect visualStatementRect = new Rect(rect.x, rect.y + 12, rect.width, rect.height);
            Rect operatorRect = new Rect(rect.x + remainderWidth, rect.y + 2, operatorWidth, EditorGUIUtility.singleLineHeight);

            GUIStyle operatorStyle = new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter };
            _guiColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(_guiColor.r - _guiColorIntensityDiff.r, _guiColor.g - _guiColorIntensityDiff.g, _guiColor.b - _guiColorIntensityDiff.b, _guiColor.a - _guiColorIntensityDiff.a);
            switch (element.elementType) 
            {
                case VisualStatementElement.ElementType.Condition:
                    if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
                        _targetList.draggable = _targetList.displayRemove = true;

                    EditorGUI.PropertyField(visualStatementRect, element.property);
                    break;

                case VisualStatementElement.ElementType.LogicalOperator:
                    if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
                        _targetList.draggable = _targetList.displayRemove = false;

                    EditorGUI.BeginChangeCheck();
                    string[] operators = GetOperatorsAsStrings();
                    int popup = EditorGUI.Popup(operatorRect, element.property.enumValueIndex, operators, operatorStyle);
                    if (EditorGUI.EndChangeCheck())
                    {
                        element.property.enumValueIndex = popup;
                        element.property.serializedObject.ApplyModifiedProperties();
                        element.property.serializedObject.Update();
                    }
                    break;
            }
            GUI.backgroundColor = _guiColor;
        }
        #endregion
        #endregion
    }
}
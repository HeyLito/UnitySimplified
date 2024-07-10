#if UNITY_EDITOR

using System;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnitySimplified.SpriteAnimator.Controller;
using static UnitySimplified.SpriteAnimator.AnimationTransition;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [CustomPropertyDrawer(typeof(ControllerTransition))]
    public class ControllerTransitionDrawer : PropertyDrawer
    {
        private readonly PropertyDrawableReorderableList _drawableLists = new();
        private FieldInfo _scheduleRemoveInfo = null;
        private PropertyInfo _isOverMaxMultiEditLimitInfo = null;
        private MethodInfo _invalidateCacheRecursiveInfo = null;

#if UNITY_2022_1_OR_NEWER
        public FieldInfo ScheduleRemoveInfo => _scheduleRemoveInfo ??= typeof(ReorderableList).GetField("m_scheduleRemove", BindingFlags.Instance | BindingFlags.NonPublic);
#else
            public FieldInfo ScheduleRemoveInfo => _scheduleRemoveInfo ??= typeof(ReorderableList).GetField("scheduleRemove", BindingFlags.Instance | BindingFlags.NonPublic);
#endif
        public PropertyInfo IsOverMaxMultiEditLimitInfo => _isOverMaxMultiEditLimitInfo ??= typeof(ReorderableList).GetProperty("isOverMaxMultiEditLimit", BindingFlags.Instance | BindingFlags.NonPublic);
        public MethodInfo InvalidateCacheRecursiveInfo => _invalidateCacheRecursiveInfo ??= typeof(ReorderableList).GetMethod("InvalidateCacheRecursive", BindingFlags.Instance | BindingFlags.NonPublic);



        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            float height = 0;
            height += 2;
            height += EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                SerializedProperty fixedEntryTypeProp = property.FindPropertyRelative("_fixedEntryType");
                SerializedProperty fixedEntryDurationProp = property.FindPropertyRelative("_fixedEntryDuration");
                SerializedProperty transitionOffsetProp = property.FindPropertyRelative("_transitionOffset");
                SerializedProperty controllerConditionsProp = property.FindPropertyRelative("_conditions");

                height += 2;
                height += EditorGUI.GetPropertyHeight(fixedEntryTypeProp);
                height += 2;
                height += EditorGUI.GetPropertyHeight(fixedEntryDurationProp);
                height += 2;
                height += EditorGUI.GetPropertyHeight(transitionOffsetProp);
                height += 2;
                height += _drawableLists.GetList(controllerConditionsProp, () => InitializeReorderableList(controllerConditionsProp)).GetHeight();
            }

            return height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SpriteAnimatorController controller = property.serializedObject.targetObject as SpriteAnimatorController;

            if (controller != null)
            {
                SerializedProperty inIdentifierProp = property.FindPropertyRelative("_inIdentifier");
                SerializedProperty outIdentifierProp = property.FindPropertyRelative("_outIdentifier");

                controller.TryGetStateFromIdentifier(inIdentifierProp.stringValue, out var inControllerState);
                controller.TryGetStateFromIdentifier(outIdentifierProp.stringValue, out var outControllerState);

                if (inControllerState != null && outControllerState != null)
                {
                    EditorGUI.BeginChangeCheck();

                    Rect previousRect = new(position) { height = 0 };

                    previousRect.y += 2;
                    Rect transitionHeaderRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height, height = EditorGUIUtility.singleLineHeight };
                    transitionHeaderRect.x += 10;
                    if (property.isExpanded = EditorGUI.Foldout(transitionHeaderRect, property.isExpanded, $"{inControllerState.Name} -> {outControllerState.Name}", true, EditorStyles.boldLabel))
                    {
                        SerializedProperty fixedEntryTypeProp = property.FindPropertyRelative("_fixedEntryType");
                        SerializedProperty fixedEntryDurationProp = property.FindPropertyRelative("_fixedEntryDuration");
                        SerializedProperty transitionOffsetProp = property.FindPropertyRelative("_transitionOffset");
                        SerializedProperty controllerConditionsProp = property.FindPropertyRelative("_conditions");

                        previousRect.y += 2;
                        Rect fixedEntryTypeRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height, height = EditorGUI.GetPropertyHeight(fixedEntryTypeProp) };
                        EditorGUI.PropertyField(fixedEntryTypeRect, fixedEntryTypeProp);

                        EditorGUI.BeginDisabledGroup(fixedEntryTypeProp.enumValueIndex == (int)FixedEntry.None);
                        previousRect.y += 2;
                        Rect fixedEntryDurationRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height, height = EditorGUI.GetPropertyHeight(fixedEntryDurationProp) };
                        EditorGUI.PropertyField(fixedEntryDurationRect, fixedEntryDurationProp);
                        EditorGUI.EndDisabledGroup();

                        previousRect.y += 2;
                        Rect transitionOffsetRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height, height = EditorGUI.GetPropertyHeight(transitionOffsetProp) };
                        EditorGUI.PropertyField(transitionOffsetRect, transitionOffsetProp);

                        previousRect.y += 2;
                        ReorderableList controllerConditionsList = _drawableLists.GetList(controllerConditionsProp, () => InitializeReorderableList(controllerConditionsProp));
                        Rect controllerConditionsRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height, height = controllerConditionsList.GetHeight() };
                        controllerConditionsList.DoList(controllerConditionsRect);
                    }


                    if (EditorGUI.EndChangeCheck())
                        property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private ReorderableList InitializeReorderableList(SerializedProperty serializedProperty)
        {
            ReorderableList list = new(serializedProperty.serializedObject, serializedProperty) { displayAdd = true, displayRemove = true };
            Rect headerRect = default;
            list.drawHeaderCallback = (rect) =>
            {
                headerRect = rect;
                EditorGUI.BeginProperty(rect, GUIContent.none, list.serializedProperty);
                EditorGUI.LabelField(rect, new GUIContent("Conditions", list.serializedProperty.tooltip));
                EditorGUI.EndProperty();
            };
            list.drawElementCallback = (rect, index, _, _) =>
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, list.serializedProperty.GetArrayElementAtIndex(index), true);
                if (EditorGUI.EndChangeCheck())
                {
                    list.serializedProperty.serializedObject.ApplyModifiedProperties();
                    list.serializedProperty.serializedObject.Update();
                }
            };
            list.onAddDropdownCallback = (a, b) => OnListAddDropdown(list, a);
            list.elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index));
            list.drawFooterCallback = (rect) => DrawListFooter(list, new Rect(rect) { x = rect.x + 13, y = headerRect.y });
            return list;
        }

        private void OnListAddDropdown(ReorderableList list, Rect buttonRect)
        {
            SerializedProperty controllerParametersProp = list.serializedProperty.serializedObject.FindProperty("_parameters").FindPropertyRelative("_items");

            GenericMenu genericMenu = new GenericMenu();
            for (int i = 0; i < controllerParametersProp.arraySize; i++)
            {
                BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var controllerParameterProp = controllerParametersProp.GetArrayElementAtIndex(i);
                var controllerParameter = controllerParameterProp.ExposeProperty(bindingFlags, true) as ControllerParameter;
                genericMenu.AddItem(new GUIContent(controllerParameter.Name), false, () =>
                {
                    BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

                    int nextIndex = list.serializedProperty.arraySize;
                    if (list.serializedProperty.ExposePropertyInfo(bindingFlags, out FieldInfo listInfo, out object previousObj, out _, true))
                    {
                        var exposedList = listInfo.GetValue(previousObj) as IList;
                        if (exposedList != null)
                        {
                            var controller = list.serializedProperty.serializedObject.targetObject as SpriteAnimatorController;
                            Undo.RecordObject(controller, "Add Controller Condition");
                            exposedList.Add(new ControllerCondition(controllerParameterProp.ExposeProperty(bindingFlags, true) as ControllerParameter));
                            list.serializedProperty.serializedObject.ApplyModifiedProperties();
                            list.serializedProperty.serializedObject.Update();
                        }
                        else throw new NullReferenceException();
                    }
                    else throw new NotSupportedException();
                });
            }
            genericMenu.DropDown(buttonRect);
        }

        private void DrawListFooter(ReorderableList list, Rect rect)
        {
            float num = rect.xMax - 10f;
            float num2 = num - 8f;
            if (list.displayAdd)
                num2 -= 25f;
            if (list.displayRemove)
                num2 -= 25f;

            rect = new Rect(num2, rect.y, num - num2, rect.height);
            Rect rectForAdd = new Rect(num2 + 4f, rect.y, 25f, 16f);
            Rect rectForRemove = new Rect(num - 29f, rect.y, 25f, 16f);

            if (Event.current.type == EventType.Repaint)
                ReorderableList.defaultBehaviours.footerBackground.Draw(rect, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);

            if (list.displayAdd)
            {
                EditorGUI.DisabledScope disabledScopeForAdd = new EditorGUI.DisabledScope((list.onCanAddCallback != null && !list.onCanAddCallback(list)) || (bool)IsOverMaxMultiEditLimitInfo.GetValue(list));
                try
                {
                    if (GUI.Button(rectForAdd, (list.onAddDropdownCallback != null) ? ReorderableList.defaultBehaviours.iconToolbarPlusMore : ReorderableList.defaultBehaviours.iconToolbarPlus, ReorderableList.defaultBehaviours.preButton))
                    {
                        if (list.onAddDropdownCallback != null)
                        {
                            list.onAddDropdownCallback(rectForAdd, list);
                        }
                        else if (list.onAddCallback != null)
                        {
                            list.onAddCallback(list);
                        }
                        else
                        {
                            ReorderableList.defaultBehaviours.DoAddButton(list);
                        }

                        list.onChangedCallback?.Invoke(list);
                        InvalidateCacheRecursiveInfo.Invoke(list, null);
                    }
                }
                finally
                {
                    ((IDisposable)disabledScopeForAdd).Dispose();
                }
            }

            if (list.displayRemove)
            {
                EditorGUI.DisabledScope disabledScopeForRemove = new EditorGUI.DisabledScope(list.index < 0 || list.index >= list.count || (list.onCanRemoveCallback != null && !list.onCanRemoveCallback(list)) || (bool)IsOverMaxMultiEditLimitInfo.GetValue(list));
                try
                {
                    if (GUI.Button(rectForRemove, ReorderableList.defaultBehaviours.iconToolbarMinus, ReorderableList.defaultBehaviours.preButton) || (GUI.enabled && (bool)ScheduleRemoveInfo.GetValue(list)))
                    {
                        if (list.onRemoveCallback == null)
                        {
                            ReorderableList.defaultBehaviours.DoRemoveButton(list);
                        }
                        else
                        {
                            list.onRemoveCallback(list);
                        }

                        list.onChangedCallback?.Invoke(list);
                        InvalidateCacheRecursiveInfo.Invoke(list, null);
                        GUI.changed = true;
                    }
                }
                finally
                {
                    ((IDisposable)disabledScopeForRemove).Dispose();
                }
            }

            ScheduleRemoveInfo.SetValue(list, false);
        }
    }
}

#endif
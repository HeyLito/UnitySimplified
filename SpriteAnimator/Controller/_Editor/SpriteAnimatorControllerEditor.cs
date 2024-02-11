#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnitySimplified.SpriteAnimator.Controller;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [CustomEditor(typeof(SpriteAnimatorController))]
    public class SpriteAnimatorControllerEditor : Editor
    {
        private GUIStyle _centerStyle;
        private int _optionsLength = 0;
        private string[] _fromSelectionOptions = Array.Empty<string>();
        private string[] _toSelectionOptions = Array.Empty<string>();
        private int _fromCurrentSelection;
        private int _toCurrentSelection;

        private GUIStyle CenterStyle
        {
            get
            {
                if (_centerStyle != null)
                    return _centerStyle;
                _centerStyle = new(EditorStyles.label);
                _centerStyle.richText = true;
                _centerStyle.alignment = TextAnchor.MiddleCenter;
                return _centerStyle;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            float centerWidth = 32;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("<b><i>Transition Creation</i></b>", CenterStyle);

            Rect horizontalRect = EditorGUILayout.BeginHorizontal();
            Rect fromSelectionPopupRect = new Rect(horizontalRect) { width = (horizontalRect.width - centerWidth) / 2 };
            Rect centerRect = new Rect(horizontalRect) { width = centerWidth, x = fromSelectionPopupRect.x + fromSelectionPopupRect.width };
            Rect toSelectionPopupRect = new Rect(horizontalRect) { width = (horizontalRect.width - centerWidth) / 2, x = centerRect.x + centerRect.width };

            EditorGUILayout.Space(18);
            int optionsLength = GetDisplayOptionsLength();
            if (optionsLength != _optionsLength)
            {
                _fromSelectionOptions = new string[optionsLength];
                _toSelectionOptions = new string[optionsLength];
            }
            SetDisplayOptions(_fromSelectionOptions);
            SetDisplayOptions(_toSelectionOptions);
            _fromCurrentSelection = EditorGUI.Popup(fromSelectionPopupRect, _fromCurrentSelection, _fromSelectionOptions);
            EditorGUI.LabelField(centerRect, "->", CenterStyle);
            _toCurrentSelection = EditorGUI.Popup(toSelectionPopupRect, _toCurrentSelection, _toSelectionOptions);
            EditorGUILayout.EndHorizontal();
            bool selectionIsValid = _fromCurrentSelection > 0 && _toCurrentSelection > 0 && _fromCurrentSelection != _toCurrentSelection;
            bool canMakeConnection = false;
            SpriteAnimatorController controller = serializedObject.targetObject as SpriteAnimatorController;
            ControllerState inState = null;
            ControllerState outState = null;
            if (selectionIsValid)
            {
                if (controller.TryGetStateFromName(_fromSelectionOptions[_fromCurrentSelection], out inState) && controller.TryGetStateFromName(_toSelectionOptions[_toCurrentSelection], out outState))
                    canMakeConnection = inState.CanConnectTo(outState);
                else canMakeConnection = false;
            }
            EditorGUI.BeginDisabledGroup(!selectionIsValid || !canMakeConnection);
            if (GUILayout.Button("Create Transition"))
            {
                Undo.RecordObject(controller, "Create Controller Transition");
                controller.CreateTransition(inState, outState, out _);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
            EditorGUI.EndDisabledGroup();


            EditorGUILayout.EndVertical();
        }

        private int GetDisplayOptionsLength() => (target as SpriteAnimatorController).States.Count + 1;
        private void SetDisplayOptions(string[] displayedOptions)
        {
            var controller = target as SpriteAnimatorController;
            if (displayedOptions.Length > 0)
            {
                int index = 0;
                displayedOptions[0] = "None";
                foreach (var state in controller.States)
                {
                    if (index < displayedOptions.Length)
                        displayedOptions[index++ + 1] = state.Name;
                    else break;
                }
            }
        }
    }
}

#endif
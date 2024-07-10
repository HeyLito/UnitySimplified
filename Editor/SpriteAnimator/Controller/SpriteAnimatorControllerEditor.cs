using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.SpriteAnimator.Controller;

namespace UnitySimplifiedEditor.SpriteAnimator.Controller
{
    [CustomEditor(typeof(SpriteAnimatorController))]
    public class SpriteAnimatorControllerEditor : Editor
    {
        private GUIStyle _centerStyle;
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
                _centerStyle = new GUIStyle(EditorStyles.label);
                _centerStyle.richText = true;
                _centerStyle.alignment = TextAnchor.MiddleCenter;
                return _centerStyle;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (serializedObject.targetObject is not SpriteAnimatorController controller)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                ControllerState inState = null;
                ControllerState outState = null;

                EditorGUILayout.LabelField("<b><i>Transition Creation</i></b>", CenterStyle);
                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    float centerWidth = 32;
                    Rect fromSelectionPopupRect = new(horizontalScope.rect) { width = (horizontalScope.rect.width - centerWidth) / 2 };
                    Rect centerRect = new(horizontalScope.rect) { width = centerWidth, x = fromSelectionPopupRect.x + fromSelectionPopupRect.width };
                    Rect toSelectionPopupRect = new(horizontalScope.rect) { width = (horizontalScope.rect.width - centerWidth) / 2, x = centerRect.x + centerRect.width };

                    EditorGUILayout.Space(18);
                    int optionsLength = GetDisplayOptionsLength();
                    if (optionsLength > -1)
                    {
                        _fromSelectionOptions = new string[optionsLength];
                        _toSelectionOptions = new string[optionsLength];
                    }
                    SetDisplayOptions(_fromSelectionOptions);
                    SetDisplayOptions(_toSelectionOptions);
                    _fromCurrentSelection = EditorGUI.Popup(fromSelectionPopupRect, _fromCurrentSelection, _fromSelectionOptions);
                    EditorGUI.LabelField(centerRect, "->", CenterStyle);
                    _toCurrentSelection = EditorGUI.Popup(toSelectionPopupRect, _toCurrentSelection, _toSelectionOptions);
                }

                bool selectionIsValid = _fromCurrentSelection > 0 && _toCurrentSelection > 0 && _fromCurrentSelection != _toCurrentSelection;
                bool canMakeConnection = selectionIsValid &&
                                         controller.TryGetStateFromName(_fromSelectionOptions[_fromCurrentSelection], out inState) &&
                                         controller.TryGetStateFromName(_toSelectionOptions[_toCurrentSelection], out outState) &&
                                         CanCreateTransition(inState, outState);

                using (new EditorGUI.DisabledGroupScope(!selectionIsValid || !canMakeConnection))
                {
                    if (!GUILayout.Button("Create Transition"))
                        return;
                    Undo.RecordObject(controller, "Create Controller Transition");
                    controller.InternalTransitions.Add(new ControllerTransition(controller, inState, outState));
                    ((ISerializationCallbackReceiver)controller).OnAfterDeserialize();
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }
            }
        }

        private int GetDisplayOptionsLength() => target is SpriteAnimatorController controller ? controller.States.Count + 1 : -1;
        private void SetDisplayOptions(IList<string> displayedOptions)
        {
            if (target is not SpriteAnimatorController controller)
                return;
            if (displayedOptions.Count <= 0)
                return;
            int index = 0;
            displayedOptions[0] = "None";
            foreach (var state in controller.States)
            {
                if (index < displayedOptions.Count)
                    displayedOptions[index++ + 1] = state.Name;
                else break;
            }
        }

        public bool CanCreateTransition(ControllerState inControllerState, ControllerState outControllerState)
        {
            if (inControllerState == null ||
                outControllerState == null ||
                inControllerState == outControllerState)
                return false;
            if (string.IsNullOrEmpty(inControllerState.GetIdentifier()) ||
                string.IsNullOrEmpty(outControllerState.GetIdentifier()) ||
                inControllerState.GetIdentifier() == outControllerState.GetIdentifier())
                return false;
            return inControllerState.CanConnectTo(outControllerState);
        }
    }
}
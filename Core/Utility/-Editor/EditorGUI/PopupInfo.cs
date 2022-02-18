#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace UnitySimplifiedEditor
{
    internal sealed class PopupInfo
    {
        private EditorWindow _guiView = null;
        private int _controlID = 0;
        private int _selected;

        public const string commandMessage = "CustomPopupMenuChanged";
        public static PopupInfo instance = null;



        public PopupInfo(int controlID)
        {
            _controlID = controlID;
            _guiView = GUIView.Current;
        }



        public static int GetValueForControl(int controlID, int selected)
        {
            Event evt = Event.current;
            if (evt.type == EventType.ExecuteCommand && evt.commandName == commandMessage)
            {
                if (instance == null)
                {
                    Debug.LogError("Popup menu has no instance");
                    return selected;
                }
                if (instance._controlID == controlID)
                {
                    GUI.changed = selected != instance._selected;
                    selected = instance._selected;
                    instance = null;
                    evt.Use();
                }
            }
            return selected;
        }
        public void SetValueDelegate(int selected)
        {
            _selected = selected;
            if (_guiView != null)
                _guiView.SendEvent(EditorGUIUtility.CommandEvent(commandMessage));
        }
    }
}

#endif
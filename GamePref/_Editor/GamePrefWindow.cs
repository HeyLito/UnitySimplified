using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnitySimplified.GamePrefs;
using UnitySimplified.Collections;

namespace UnitySimplifiedEditor.GamePrefs
{
    public class GamePrefWindow : EditorWindow
    {
        private enum GamePrefType { Local, Persistent }

        private readonly SerializableDictionary<string, bool> _propertiesInEditByIDs = new();

        private Texture2D _creatorTex;
        private Texture2D _settingsTex;
        private Texture2D _referencePageTex;
        private GUIStyle _wordWrapLabelStyle;

        private Vector2 _scrollPos;
        private Rect _scrollViewRect;
        private bool _isEditing;
        private bool _wantsToPing;
        private string _pingPrefKey;

        private SortedDictionary<string, GamePrefData> _localPrefsByIdentifier = new();
        private SortedDictionary<string, GamePrefData> _persistentPrefsByIdentifier = new();
        private readonly OrderedHashset<string> _combinedIdentifiers = new();

        public event Action OnWindowUpdated;



        public Texture2D CreatorTex => _creatorTex = _creatorTex != null ? _creatorTex : EditorGUIUtility.Load("Customized") as Texture2D;
        public Texture2D SettingsTex => _settingsTex = _settingsTex != null ? _settingsTex : EditorGUIUtility.Load("d__Popup") as Texture2D;
        public Texture2D ReferencePageTex => _referencePageTex = _referencePageTex != null ? _referencePageTex : EditorGUIUtility.Load("d__Help") as Texture2D;

        public GUIStyle WordWrapLabelStyle => _wordWrapLabelStyle ??= new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true };



        [MenuItem("Window/UnitySimplified/GamePrefs")]
        public static void OpenWindow()
        {
            EditorWindow window = GetWindow<GamePrefWindow>(false, "GamePrefs", true);
            window.Show();
        }

        private void OnEnable()
        {

            EditorGUIExtended.OnPingBegan += () => { _scrollPos.y = Mathf.Max(0, (EditorGUIExtended.PingPosition.y + EditorGUIExtended.PingPosition.height + _scrollViewRect.height / 2) - _scrollViewRect.height); };
            GamePref.OnValuesChanged += OnReload;
            GamePrefLocalDatabase.Instance.OnValuesChanged += OnReload;
            OnWindowUpdated += OnReload;

            GamePref.Reload();
        }
        private void OnDisable()
        {
            GamePref.OnValuesChanged -= OnReload;
            GamePrefLocalDatabase.Instance.OnValuesChanged -= OnReload;
            OnWindowUpdated -= OnReload;
        }
        private void OnDestroy()
        {
            if (HasOpenInstances<GamePrefCreatorWindow>())
            {
                GamePrefCreatorWindow window = GetWindow<GamePrefCreatorWindow>();
                if (window != null)
                    window.Close();
            }
            if (HasOpenInstances<GamePrefSettingsWindow>())
            {
                GamePrefSettingsWindow window = GetWindow<GamePrefSettingsWindow>();
                if (window != null)
                    window.Close();
            }
        }

        private void OnGUI()
        {
            Event evt = Event.current;
            EditorGUIUtility.labelWidth = Mathf.Max((26 + position.width) * 0.45f - 40, 120);

            DrawToolbar();

            wantsMouseMove = true;
            GUIContent labelContent = new();

            using (EditorGUILayout.ScrollViewScope scrollViewScope = new(_scrollPos))
            {
                _scrollPos = scrollViewScope.scrollPosition;


                EditorGUILayout.BeginVertical("box");
                labelContent.text = "Local GamePrefs";
                EditorGUILayout.LabelField(labelContent, EditorStyles.boldLabel, GUILayout.Width(EditorStyles.boldLabel.CalcSize(labelContent).x));
                foreach (var pref in _localPrefsByIdentifier.Values)
                {
                    if (_combinedIdentifiers.Contains(pref.identifier))
                        continue;

                    EditorGUILayout.BeginVertical("box");
                    DrawGamePref(GamePrefType.Local, pref, _isEditing);
                    EditorGUILayout.EndVertical();

                    if (GUI.changed)
                        break;
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical("box");
                labelContent.text = "Persistent GamePrefs";
                EditorGUILayout.LabelField(labelContent, EditorStyles.boldLabel, GUILayout.Width(EditorStyles.boldLabel.CalcSize(labelContent).x));
                foreach (var pref in _persistentPrefsByIdentifier.Values)
                {
                    if (_combinedIdentifiers.Contains(pref.identifier))
                        continue;

                    EditorGUILayout.BeginVertical("box");
                    DrawGamePref(GamePrefType.Persistent, pref, _isEditing);
                    EditorGUILayout.EndVertical();

                    if (GUI.changed)
                        break;
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical("box");
                labelContent.text = "Combined GamePrefs";
                EditorGUILayout.LabelField(labelContent, EditorStyles.boldLabel, GUILayout.Width(EditorStyles.boldLabel.CalcSize(labelContent).x));
                foreach (var identifier in _combinedIdentifiers)
                {
                    EditorGUILayout.BeginVertical("box");
                    DrawGamePrefCombined(identifier, _isEditing);
                    EditorGUILayout.EndVertical();

                    if(GUI.changed)
                        break;
                }
                EditorGUILayout.EndVertical();
            }

            if (evt.type == EventType.Repaint)
                _scrollViewRect = GUILayoutUtility.GetLastRect();
        }
        private void DrawToolbar()
        {
            Color color = GUI.color;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);


            Rect dropDownRect = GUILayoutUtility.GetRect(new GUIContent("Clear"), EditorStyles.toolbarDropDown);
            if (EditorGUI.DropdownButton(dropDownRect, new GUIContent("Clear"), FocusType.Keyboard))
            {
                GenericMenu contextMenu = new GenericMenu();
                contextMenu.AddItem(new GUIContent("Clear All"), false, delegate { GamePrefLocalDatabase.Instance.RemoveAll(); EditorUtility.SetDirty(GamePrefLocalDatabase.Instance); GamePref.DeleteAll(); GamePref.Overwrite(); });
                contextMenu.AddItem(new GUIContent("Clear Dynamic GamePrefs"), false, delegate { GamePrefLocalDatabase.Instance.RemoveAll(); EditorUtility.SetDirty(GamePrefLocalDatabase.Instance); OnWindowUpdated?.Invoke(); });
                contextMenu.AddItem(new GUIContent("Clear Static GamePrefs"), false, delegate { GamePref.DeleteAll(); GamePref.Overwrite(); });
                contextMenu.DropDown(dropDownRect);
            }

            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Edit", EditorStyles.toolbarButton, GUILayout.MaxWidth(50)))
                _isEditing = !_isEditing;

            GUILayout.Space(1);

            if (GUILayout.Button(new GUIContent(image: CreatorTex, tooltip: "Open creator window."), EditorStyles.toolbarButton))
                GamePrefCreatorWindow.OpenWindow();

            if (GUILayout.Button(new GUIContent(image: SettingsTex, tooltip: ""), EditorStyles.toolbarButton))
                GamePrefSettingsWindow.OpenWindow();

            if (GUILayout.Button(new GUIContent(image: ReferencePageTex, tooltip: "Open Reference for GamePrefs"), EditorStyles.toolbarButton))
                Application.OpenURL("https://github.com/HeyLito/UnitySimplified/wiki/Scripting-API:-Game-Prefs");

            GUI.color = color;
            EditorGUILayout.EndHorizontal();
        }
        private void DrawGamePref(GamePrefType gamePrefType, GamePrefData data, bool isEditing)
        {
            Event evt = Event.current;
            GUIContent labelContent = new($"{data.key}{(GamePrefSettingsWindow.DebugMode ? $", ({data.value.GetType().Name})" : "")}");

            Rect labelRect = GUILayoutUtility.GetRect(labelContent, EditorStyles.label, GUILayout.MaxWidth(position.width - 32));

            bool isEditingThisPref = false;
            if (isEditing && !GamePrefSettingsWindow.DebugMode)
                if (_propertiesInEditByIDs.TryGetValue(data.identifier, out isEditingThisPref))
                    isEditingThisPref = _propertiesInEditByIDs[data.identifier] = EditorGUI.Foldout(labelRect, isEditingThisPref, labelContent.text, true);
                else _propertiesInEditByIDs[data.identifier] = false;
            else EditorGUI.LabelField(labelRect, labelContent);

            if (isEditingThisPref || GamePrefSettingsWindow.DebugMode)
            {
                EditorGUI.indentLevel += 2;
                if (GamePrefSettingsWindow.DebugMode)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Identifier"), GUILayout.Width(EditorGUIUtility.labelWidth));
                    EditorGUI.indentLevel -= 2;
                    EditorGUI.SelectableLabel(GUILayoutUtility.GetRect(new GUIContent(data.identifier), WordWrapLabelStyle), data.identifier, WordWrapLabelStyle);
                    EditorGUI.indentLevel += 2;
                    EditorGUILayout.EndHorizontal();
                }
                string newKey = EditorGUILayout.DelayedTextField(new GUIContent("Key Name"), data.key);
                if (newKey != data.key)
                {
                    if (gamePrefType == GamePrefType.Local)
                    {
                        data.key = newKey;
                        GamePrefLocalDatabase.Instance.OverwriteGamePref(data);
                        EditorUtility.SetDirty(GamePrefLocalDatabase.Instance);
                        GUI.changed = true;
                    }
                    if (gamePrefType.HasFlag(GamePrefType.Persistent))
                    {
                        data.key = newKey;
                        GamePref.Overwrite();
                    }
                }

                if (gamePrefType == GamePrefType.Local)
                {
                    object defaultValue = EditorGUILayoutExtended.ObjectField(new GUIContent("Default Value"), data.value, data.value.GetType());
                    if (!data.value.Equals(defaultValue))
                    {
                        data.value = defaultValue;
                        GamePrefLocalDatabase.Instance.OverwriteGamePref(data);
                        EditorUtility.SetDirty(GamePrefLocalDatabase.Instance);
                    }
                }
                if (gamePrefType.HasFlag(GamePrefType.Persistent))
                {
                    object value = EditorGUILayoutExtended.ObjectField(new GUIContent("Value"), data.value, data.value.GetType());
                    if (!data.value.Equals(value))
                    {
                        data.value = value;
                        GamePref.Overwrite();
                    }
                }
                EditorGUI.indentLevel -= 2;
            }

            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 1 && labelRect.Contains(Event.current.mousePosition))
                    {
                        void RemoveLocalData()
                        {
                            GamePrefLocalDatabase.Instance.Remove(data.identifier);
                            EditorUtility.SetDirty(GamePrefLocalDatabase.Instance);
                            OnWindowUpdated?.Invoke();
                        }
                        void RemovePersistentData()
                        {
                            GamePref.Delete(data);
                            GamePref.Overwrite();
                        }

                        GenericMenu contextMenu = new();
                        switch (gamePrefType)
                        {
                            case GamePrefType.Local:
                                contextMenu.AddItem(new GUIContent("Remove"), false, RemoveLocalData);
                                break;

                            case GamePrefType.Persistent:
                                contextMenu.AddItem(new GUIContent("Remove"), false, RemovePersistentData);
                                contextMenu.AddItem(new GUIContent("Convert to Combined"), false, () =>
                                {
                                    GamePrefLocalDatabase.Instance.AddGamePrefData(CreateGamePrefData(data));
                                    EditorUtility.SetDirty(GamePrefLocalDatabase.Instance);
                                    OnWindowUpdated?.Invoke();
                                });
                                break;
                        }
                        contextMenu.ShowAsContext();
                    }
                    break;
            }

            if (Event.current.type == EventType.Repaint && labelContent.text == _pingPrefKey)
            {
                bool canStart = _wantsToPing;
                bool canDraw = EditorGUIExtended.IsPinging;
                Rect pingPosition = default;
                if (canStart || canDraw)
                {
                    pingPosition = !isEditing || GamePrefSettingsWindow.DebugMode
                        ? labelRect
                        : new Rect(labelRect.x + 13, labelRect.y, labelRect.width, labelRect.height);
                }
                if (canStart)
                {
                    _wantsToPing = false;
                    EditorGUIExtended.PingStart(pingPosition, labelContent);
                }
                if (canDraw)
                {
                    EditorGUIExtended.PingUpdate(pingPosition);
                    Repaint();
                }
            }
        }

        private void DrawGamePrefCombined(string identifier, bool isEditing)
        {
            var localPref = _localPrefsByIdentifier[identifier];
            var persistentPref = _persistentPrefsByIdentifier[identifier];

            if (localPref.key != persistentPref.key)
                return;
            var key = localPref.key;
            
            Event evt = Event.current;
            GUIContent labelContent = new($"{localPref.key}{(GamePrefSettingsWindow.DebugMode ? $", ({localPref.value.GetType().Name})" : "")}");

            Rect labelRect = GUILayoutUtility.GetRect(labelContent, EditorStyles.label, GUILayout.MaxWidth(position.width - 32));

            bool isEditingThisPref = false;
            if (isEditing && !GamePrefSettingsWindow.DebugMode)
                if (_propertiesInEditByIDs.TryGetValue(identifier, out isEditingThisPref))
                    isEditingThisPref = _propertiesInEditByIDs[identifier] = EditorGUI.Foldout(labelRect, isEditingThisPref, labelContent.text, true);
                else _propertiesInEditByIDs[identifier] = false;
            else EditorGUI.LabelField(labelRect, labelContent);

            if (isEditingThisPref || GamePrefSettingsWindow.DebugMode)
            {
                EditorGUI.indentLevel += 2;
                if (GamePrefSettingsWindow.DebugMode)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Identifier"), GUILayout.Width(EditorGUIUtility.labelWidth));
                    EditorGUI.indentLevel -= 2;
                    EditorGUI.SelectableLabel(GUILayoutUtility.GetRect(new GUIContent(identifier), WordWrapLabelStyle), identifier, WordWrapLabelStyle);
                    EditorGUI.indentLevel += 2;
                    EditorGUILayout.EndHorizontal();
                }
                string newKey = EditorGUILayout.DelayedTextField(new GUIContent("Key Name"), key);
                if (newKey != key)
                {
                    localPref.key = newKey;
                    GamePrefLocalDatabase.Instance.OverwriteGamePref(localPref);
                    EditorUtility.SetDirty(GamePrefLocalDatabase.Instance);

                    persistentPref.key = newKey;
                    GamePref.Overwrite();

                    GUI.changed = true;
                }

                object defaultValue = EditorGUILayoutExtended.ObjectField(new GUIContent("Default Value"), localPref.value, localPref.value.GetType());
                if (!localPref.value.Equals(defaultValue))
                {
                    localPref.value = defaultValue;
                    GamePrefLocalDatabase.Instance.OverwriteGamePref(localPref);
                    EditorUtility.SetDirty(GamePrefLocalDatabase.Instance);
                }

                object value = EditorGUILayoutExtended.ObjectField(new GUIContent("Value"), persistentPref.value, persistentPref.value.GetType());
                if (!persistentPref.value.Equals(value))
                {
                    persistentPref.value = value;
                    GamePref.Overwrite();
                }
                EditorGUI.indentLevel -= 2;
            }

            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 1 && labelRect.Contains(Event.current.mousePosition))
                    {
                        void RemoveLocalData()
                        {
                            GamePrefLocalDatabase.Instance.Remove(localPref.identifier);
                            EditorUtility.SetDirty(GamePrefLocalDatabase.Instance);
                            OnWindowUpdated?.Invoke();
                        }
                        void RemovePersistentData()
                        {
                            GamePref.Delete(persistentPref);
                            GamePref.Overwrite();
                        }

                        GenericMenu contextMenu = new();
                        contextMenu.AddItem(new GUIContent("Remove/All"), false, delegate { RemoveLocalData(); RemovePersistentData(); });
                        contextMenu.AddItem(new GUIContent("Remove/Local Data"), false, RemoveLocalData);
                        contextMenu.AddItem(new GUIContent("Remove/Persistent Data"), false, RemovePersistentData);
                        contextMenu.ShowAsContext();
                    }
                    break;
            }

            if (Event.current.type == EventType.Repaint && labelContent.text == _pingPrefKey)
            {
                bool canStart = _wantsToPing;
                bool canDraw = EditorGUIExtended.IsPinging;
                Rect pingPosition = default;
                if (canStart || canDraw)
                {
                    pingPosition = !isEditing || GamePrefSettingsWindow.DebugMode
                        ? labelRect
                        : new Rect(labelRect.x + 13, labelRect.y, labelRect.width, labelRect.height);
                }
                if (canStart)
                {
                    _wantsToPing = false;
                    EditorGUIExtended.PingStart(pingPosition, labelContent);
                }
                if (canDraw)
                {
                    EditorGUIExtended.PingUpdate(pingPosition);
                    Repaint();
                }
            }
        }

        private static GamePrefData CreateGamePrefData(GamePrefData gamePrefData) => Activator.CreateInstance(typeof(GamePrefData), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { gamePrefData }, null, null) as GamePrefData;
        private void OnReload()
        {
            _localPrefsByIdentifier = new SortedDictionary<string, GamePrefData>(GamePrefLocalDatabase.Instance.GetGamePrefs().Values.ToDictionary(x => x.identifier));
            _persistentPrefsByIdentifier = new SortedDictionary<string, GamePrefData>(GamePref.All.ToDictionary(x => x.identifier));
            _combinedIdentifiers.Clear();

            foreach (var localPrefByIdentifier in _localPrefsByIdentifier)
                if (_persistentPrefsByIdentifier.ContainsKey(localPrefByIdentifier.Key))
                    _combinedIdentifiers.Add(localPrefByIdentifier.Key);
            Repaint();
        }
        internal void PingPref(string prefKey)
        {
            _wantsToPing = true;
            _pingPrefKey = prefKey;
        }

        internal static int SortPairs(KeyValuePair<string, GamePrefData> lhs, KeyValuePair<string, GamePrefData> rhs)
        {
            //int result = lhs.Value.prefValue.GetType().Name.CompareTo(rhs.Value.prefValue.GetType().Name);
            //if (result != 0)
            //    return result;
            //return lhs.Value.prefKey.CompareTo(rhs.Value.prefKey);

            int result = lhs.Value.key.CompareTo(rhs.Value.key);
            if (result == -1)
                return result;
            return lhs.Value.value.GetType().Name.CompareTo(rhs.Value.value.GetType().Name);
        }
    }
}
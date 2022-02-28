#if UNITY_EDITOR

using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified;
using UnitySimplified.Serialization;
using System.IO;

namespace UnitySimplifiedEditor.Serialization
{
    public class GamePrefWindow : EditorWindow
    {
        private enum ValueType { Boolean, Integer, Float, String }
        private enum GamePrefType { None = 1 << 1, Dynamic = 1 << 3, Static = 1 << 2 }
        private enum WindowTabs { None = default, Creator, Settings, Info, Description, ScriptingAPI }

        [SerializeField] private GamePref dummyPref;

        private Dictionary<string, GamePrefData> _dynamicGamePrefsByIDs = new Dictionary<string, GamePrefData>();
        private Dictionary<string, GamePrefData> _dynamicGamePrefsByKeys = new Dictionary<string, GamePrefData>();
        private Dictionary<string, GamePrefData> _staticPrefsByIDs = new Dictionary<string, GamePrefData>();
        private Dictionary<string, GamePrefData> _staticPrefsByKeys = new Dictionary<string, GamePrefData>();
        private Dictionary<string, bool> _editingStatusesByIDs = new Dictionary<string, bool>();
        private PriorityQueue<int, ((string, GamePrefData), (string, GamePrefData))> _queuedPrefs = new PriorityQueue<int, ((string, GamePrefData), (string, GamePrefData))>();
        private List<KeyValuePair<string, GamePrefData>> _sortedDynamics = new List<KeyValuePair<string, GamePrefData>>();
        private List<KeyValuePair<string, GamePrefData>> _sortedPersistants = new List<KeyValuePair<string, GamePrefData>>();

        private Texture2D _configTex;
        private Texture2D _configOnTex;
        private Texture2D _plusTex;
        private Texture2D _minusTex;
        private Texture2D _infoTex;
        private Texture2D _infoOnTex;
        private GUIStyle _windowStyle;
        private GUIStyle _borderStyle;
        private GUIStyle _wordWrapLabelStyle;

        private const float _aDiff = 0.25f, _bDiff = -0.15f;
        private Vector2 _scrollPos = new Vector2();
        private Vector2 _settingsScrollPos = new Vector2();
        private Rect _scrollViewRect = new Rect();
        private bool _isEditing = false;
        private bool _debugMode = false;
        private string _dynamicGamePrefsPath = "";
        private string _staticGamePrefsPath = "";
        private string _tempKey = "";
        private object _tempValue = null;
        private ValueType _tempType = default;
        private WindowTabs _mainWindowTab = default;
        private WindowTabs _settingsTab = WindowTabs.Description;

        public static Action onGamePrefsUpdated;

        public Color SelectedGUIColor => new Color(GUI.color.r + _aDiff, GUI.color.g + _aDiff, GUI.color.b + _aDiff, GUI.color.a);
        public Color DeselectedGUIColor => new Color(GUI.color.r + _bDiff, GUI.color.g + _bDiff, GUI.color.b + _bDiff, GUI.color.a);
        public Texture2D ConfigTex => _configTex = _configTex != null ? _configTex : EditorGUIUtility.Load("icons/d_TerrainInspector.TerrainToolSettings.png") as Texture2D;
        public Texture2D ConfigOnTex => _configOnTex = _configOnTex != null ? _configOnTex : EditorGUIUtility.Load("icons/d_TerrainInspector.TerrainToolSettings On.png") as Texture2D;
        public Texture2D PlusTex => _plusTex = _plusTex != null ? _plusTex : EditorGUIUtility.Load("icons/d_Toolbar Plus.png") as Texture2D;
        public Texture2D MinusTex => _minusTex = _minusTex != null ? _minusTex : EditorGUIUtility.Load("icons/d_Toolbar Minus.png") as Texture2D;
        public Texture2D InfoTex => _infoTex = _infoTex != null ? _infoTex : EditorGUIUtility.Load("icons/d_UnityEditor.InspectorWindow.png") as Texture2D;
        public Texture2D InfoOnTex => _infoOnTex = _infoOnTex != null ? _infoOnTex : EditorGUIUtility.Load("icons/d_UnityEditor.DebugInspectorWindow.png") as Texture2D;

        public GUIStyle WindowStyle
        {
            get
            {
                if (_windowStyle == null)
                {
                    int border = 14;
                    int overflow = 9;
                    var style = new GUIStyle();
                    style.name = nameof(WindowStyle);
                    style.border = new RectOffset(border, border, border, border);
                    style.overflow = new RectOffset(overflow, overflow, overflow, overflow);
                    style.fontSize = 13;
                    style.alignment = TextAnchor.MiddleCenter;
                    style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/popupwindowoff.png") as Texture2D;
                    style.normal.textColor = Color.white;
                    style.focused.background = EditorGUIUtility.Load("builtin skins/darkskin/images/popupwindowon.png") as Texture2D;
                    style.focused.textColor = Color.white;
                    _windowStyle = style;
                }
                return _windowStyle;
            }
        }
        public GUIStyle BorderStyle => _borderStyle == null ? _borderStyle = new GUIStyle("Wizard Box") { wordWrap = true, richText = true, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(4, 4, 2, 2) } : _borderStyle;
        public GUIStyle WordWrapLabelStyle => _wordWrapLabelStyle == null ? _wordWrapLabelStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true } : _wordWrapLabelStyle;

        [MenuItem("EasyVN/Windows/GamePrefs")]
        public static void OpenWindow()
        {
            EditorWindow window = GetWindow<GamePrefWindow>();
            window.Show();
            window.Focus();
            window.titleContent = new GUIContent("GamePrefs");
        }

        private void OnEnable()
        {
            _windowStyle = _borderStyle = _wordWrapLabelStyle = null;
            onGamePrefsUpdated += LoadKeys;
            GamePref.Load();
            PingLabel.OnPingBegan += () => _scrollPos.y = Mathf.Max(0, (PingLabel.LabelRect.y + PingLabel.LabelRect.height + _scrollViewRect.height / 2) - _scrollViewRect.height);
        }
        private void OnDisable()
        {   onGamePrefsUpdated -= LoadKeys;   }
        private void OnGUI()
        {
            Event evt = Event.current;
            bool changed = false;
            
            EditorGUIUtility.labelWidth = Mathf.Max((26 + position.width) * 0.45f - 40, 120);

            DrawToolbar();

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true };
            switch (_mainWindowTab)
            {
                case WindowTabs.Creator:
                    DrawCreatorWindow(titleStyle, ref changed);
                    break;
                case WindowTabs.Settings:
                    DrawSettingsWindow(titleStyle, ref changed);
                    break;
                case WindowTabs.Info:
                    DrawInfoWindow(titleStyle, ref changed);
                    break;
            }

            wantsMouseMove = true;
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if (!changed)
            {
                var tempQueuedPrefs = new PriorityQueue<int, ((string, GamePrefData), (string, GamePrefData))>(_queuedPrefs);
                EditorGUILayout.BeginVertical("box");
                GamePrefType gamePrefType = GamePrefType.None;
                for (int i = 0, count = tempQueuedPrefs.Count(); i < count; i++)
                {
                    var poppedPair = tempQueuedPrefs.PopPair();
                    GUIContent labelContent = new GUIContent();
                    string persistentID = "";
                    string prefKey = "";
                    object prefValue = null;
                    GamePrefType previousType = gamePrefType;

                    switch (poppedPair.Key)
                    {
                        case 2:
                            gamePrefType = GamePrefType.Dynamic | GamePrefType.Static;
                            if (previousType != gamePrefType)
                                labelContent.text = "Conjoined GamePrefs";
                            break;
                        case 1:
                            gamePrefType = GamePrefType.Dynamic;
                            if (previousType != gamePrefType)
                                labelContent.text = "Dynamic GamePrefs";
                            break;
                        case 0:
                            gamePrefType = GamePrefType.Static;
                            if (previousType != gamePrefType)
                                labelContent.text = "Static GamePrefs";
                            break;
                    }

                    if (previousType != gamePrefType)
                    {
                        if (previousType != GamePrefType.None)
                            EditorGUILayout.Space(16);
                        EditorGUILayout.LabelField(labelContent, EditorStyles.boldLabel, GUILayout.Width(EditorStyles.boldLabel.CalcSize(labelContent).x));
                    }

                    if (!gamePrefType.HasFlag(GamePrefType.None))
                    {
                        if (gamePrefType.HasFlag(GamePrefType.Dynamic))
                        {
                            persistentID = poppedPair.Value.Item1.Item1;
                            prefKey = poppedPair.Value.Item1.Item2.prefKey;
                            prefValue = poppedPair.Value.Item1.Item2.prefValue;
                        }
                        else if (gamePrefType.HasFlag(GamePrefType.Static))
                        {
                            persistentID = poppedPair.Value.Item2.Item1;
                            prefKey = poppedPair.Value.Item2.Item2.prefKey;
                            prefValue = poppedPair.Value.Item2.Item2.prefValue;
                        }
                        DrawGamePref(gamePrefType, poppedPair.Value.Item1.Item2, poppedPair.Value.Item2.Item2, persistentID, prefKey, prefValue, _isEditing, ref changed);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            if (evt.type == EventType.Repaint)
                _scrollViewRect = GUILayoutUtility.GetLastRect();
            if (changed)
                GUIView.Current.Repaint();
        }

        private void DrawCreatorWindow(GUIStyle style, ref bool changed)
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginVertical(WindowStyle);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIContent titleContent = new GUIContent("Dynamic GamePref Creator");
            EditorGUILayout.LabelField(titleContent, style, GUILayout.MaxWidth(style.CalcSize(titleContent).x));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6);

            bool dynamicExists = _dynamicGamePrefsByKeys.ContainsKey(_tempKey);
            bool staticExists = _staticPrefsByKeys.TryGetValue(_tempKey, out GamePrefData staticPrefData);
            bool staticExistsWithDifferentType = staticExists && !TypeIsSupported(staticPrefData.GetPrefType(), _tempType);

            _tempKey = EditorGUILayout.TextField(new GUIContent("Key"), _tempKey);
            if (dynamicExists)
            {
                Color color = GUI.color;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(Mathf.Max((40 + position.width) * 0.45f - 40, 125));
                EditorGUI.indentLevel++;
                GUI.color = new Color(1f, 0.4f, 0.4f, 1f);
                EditorGUILayout.LabelField(new GUIContent("A Dynamic GamePref of the same key name already exists!"), WordWrapLabelStyle);
                GUI.color = color;
                EditorGUI.indentLevel--;
                EditorGUILayout.EndHorizontal();
            }

            ValueType valueType = (ValueType)EditorGUILayout.EnumPopup(new GUIContent("Value Type"), _tempType);
            if (!dynamicExists && staticExistsWithDifferentType)
            {
                Color color = GUI.color;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(Mathf.Max((40 + position.width) * 0.45f - 40, 125));
                EditorGUI.indentLevel++;
                GUI.color = new Color(1f, 0.4f, 0.4f, 1f);
                EditorGUILayout.LabelField(new GUIContent("The existing Static GamePref of the same key is not compatible with this value type."), WordWrapLabelStyle);
                GUI.color = color;
                EditorGUI.indentLevel--;
                EditorGUILayout.EndHorizontal();
            }

            if (valueType != _tempType || _tempValue == null)
                _tempValue = GetDefaultValueOfSupportedType(valueType);
            _tempType = valueType;

            _tempValue = EditorGUILayoutExtras.ObjectField(new GUIContent("Value"), _tempValue);

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_tempKey) || dynamicExists || staticExistsWithDifferentType);
            if (GUILayout.Button("Create GamePref", GUILayout.Width(Mathf.Max(position.width * 0.45f - 25, 120))))
            {
                if (_tempValue == null)
                    throw new Exception($"The value of the newly created {typeof(GamePref).Name} is NULL.");

                if (staticExists)
                    GamePrefStorage.Instance.AddGamePrefData(new GamePrefData(staticPrefData.persistentIdentifier, _tempKey, _tempValue));
                else GamePrefStorage.Instance.AddGamePrefData(new GamePrefData(GamePref.GetNewPref(), _tempKey, _tempValue));
                EditorUtility.SetDirty(GamePrefStorage.Instance);
                onGamePrefsUpdated?.Invoke();
                changed = true;
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }
        private void DrawSettingsWindow(GUIStyle style, ref bool changed)
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginVertical(WindowStyle);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIContent titleContent = new GUIContent("Settings");
            EditorGUILayout.LabelField(titleContent, style, GUILayout.MaxWidth(style.CalcSize(titleContent).x));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6);

            EditorGUILayout.LabelField("Dynamic GamePrefs", GUILayout.Width(EditorGUIUtility.labelWidth));
            GUIContent dynamicPrefsContent = new GUIContent(!string.IsNullOrEmpty(_dynamicGamePrefsPath) ? _dynamicGamePrefsPath : "Missing");
            EditorGUI.indentLevel++;
            EditorGUI.SelectableLabel(GUILayoutUtility.GetRect(dynamicPrefsContent, EditorStyles.helpBox), dynamicPrefsContent.text, EditorStyles.helpBox);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Static GamePrefs", GUILayout.Width(EditorGUIUtility.labelWidth));
            GUIContent staticPrefsContent = new GUIContent(!string.IsNullOrEmpty(_staticGamePrefsPath) ? _staticGamePrefsPath : DataManager.DefaultPath);
            EditorGUI.indentLevel++;
            EditorGUI.SelectableLabel(GUILayoutUtility.GetRect(staticPrefsContent, EditorStyles.helpBox), staticPrefsContent.text, EditorStyles.helpBox);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Debug")))
                _debugMode = !_debugMode;
            if (GUILayout.Button(new GUIContent("Refresh")))
                GamePref.Load();

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }
        private void DrawInfoWindow(GUIStyle style, ref bool changed)
        {
            Color color = GUI.color;
            Color selectedColor = SelectedGUIColor;
            Color deselectedColor = DeselectedGUIColor;

            string classTextColor(string text)
            { return $"<color=#68c4ac>{text}</color>"; }
            string methodTextColor(string text)
            { return $"<color=#e0dcac>{text}</color>"; }
            //string operatorTextColor(string text)
            //{ return $"<color=#6894d4>{text}</color>"; }
            //string localVariableTextColor(string text)
            //{ return $"<color=#a8dcfc>{text}</color>"; }


            EditorGUILayout.Space(2);
            EditorGUILayout.BeginVertical(WindowStyle);

            GUIContent tabContent = new GUIContent("");
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            tabContent.text = "Desription";
            GUI.color = _settingsTab == WindowTabs.Description ? selectedColor : deselectedColor;
            if (GUILayout.Button(tabContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(EditorStyles.toolbarButton.CalcSize(tabContent).x)))
                if (_settingsTab != WindowTabs.Description)
                {
                    _settingsTab = WindowTabs.Description;
                }
            tabContent.text = "Scripting API";
            GUI.color = _settingsTab == WindowTabs.ScriptingAPI ? selectedColor : deselectedColor;
            if (GUILayout.Button(tabContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(EditorStyles.toolbarButton.CalcSize(tabContent).x)))
                if (_settingsTab != WindowTabs.ScriptingAPI)
                {
                    _settingsTab = WindowTabs.ScriptingAPI;
                }
            EditorGUILayout.EndHorizontal();
            GUI.color = color;



            _settingsScrollPos = EditorGUILayout.BeginScrollView(_settingsScrollPos, GUILayout.Height(position.height - (GUILayoutUtility.GetLastRect().yMax + 40)));
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIContent titleContent = new GUIContent(_settingsTab.ToString());
            GUIStyle resizedStyle = new GUIStyle(style) { fontSize = style.fontSize + 4 };
            EditorGUILayout.LabelField(titleContent, resizedStyle, GUILayout.MaxWidth(resizedStyle.CalcSize(titleContent).x));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6);



            switch (_settingsTab)
            {
                case WindowTabs.Description:
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("GamePrefs", style);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"GamePrefs are the visual alternative to Unity's <b>{classTextColor("PlayerPrefs")}</b>. " +
                                                $"While retaining PlayerPrefs functionality, " +
                                                $"it also conveniently displays persistent data all within this window.", WordWrapLabelStyle);
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"GamePref functionality is divided by the two types: <b>Static</b> and <b>Dynamic</b>.", WordWrapLabelStyle);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 2);


                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("Static GamePrefs", style);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"Static GamePrefs is the data that is retrieved from the system's registry. " +
                                                $"The creation of a Static is at runtime by string keys or by a Dynamic GamePref.", WordWrapLabelStyle);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 2);


                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("Dynamic GamePrefs", style);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"Instead of retrieving data from the registry, " +
                                                $"Dynamic GamePrefs are retrieved locally within a <b>{classTextColor("ScriptableObject")}</b> asset file. " +
                                                $"Because of this, Dynamic GamePrefs serve as alternatives to keys.", WordWrapLabelStyle);
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"When a GamePref is serialized as a field, the Unity Editor allows to select a reference to an instanced Dynamic GamePref.", WordWrapLabelStyle);
                    EditorGUILayout.PropertyField(new SerializedObject(this).FindProperty("dummyPref"), new GUIContent("Example GamePref"));
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"<b>Note:</b> Dynamic GamePrefs are created only through this window.", WordWrapLabelStyle);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    break;



                case WindowTabs.ScriptingAPI:
                    void drawContainerInfo(int labelSpacing, string firstLabelText, string secondLabelText)
                    {
                        GUIStyle groupStyle = new GUIStyle() { margin = BorderStyle.margin, padding = new RectOffset(BorderStyle.padding.left + 2, BorderStyle.padding.right + 2, BorderStyle.padding.top + 4, BorderStyle.padding.bottom + 4), wordWrap = true, overflow = new RectOffset() };
                        Rect groupRect = EditorGUI.IndentedRect(EditorGUILayout.BeginHorizontal(groupStyle));
                        GUILayoutUtility.GetRect(new GUIContent(secondLabelText), groupStyle, GUILayout.MinWidth(400));
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.LabelField(new Rect(5 + groupRect.x, groupRect.y, groupRect.x + labelSpacing, groupRect.height), firstLabelText, BorderStyle);
                        EditorGUI.LabelField(new Rect(5 + groupRect.x + labelSpacing, groupRect.y, groupRect.width - labelSpacing - 10, groupRect.height), secondLabelText, BorderStyle);
                    }

                    EditorGUILayout.LabelField("<b>Properties</b>", style);
                    GUILayout.Space(2);
                    drawContainerInfo(120, "PersistentIdentifier", "Returns the GamePref's unique id. Primarily used internally with Dynamic GamePrefs");


                    EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 1);


                    EditorGUILayout.LabelField("<b>Public Methods</b>", style);
                    GUILayout.Space(2);
                    drawContainerInfo(65, methodTextColor("GetValue"), "Returns the value corresponding to the Dynamic GamePref's key if it exists.");
                    drawContainerInfo(65, methodTextColor("SetValue"), "Set the value identified by the Dyanmic GamePref's key.");


                    EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 1);


                    EditorGUILayout.LabelField("<b>Static Methods</b>", style);
                    GUILayout.Space(2);
                    drawContainerInfo(90, methodTextColor("DeleteAll"), "Removes all Static GamePrefs from storage. \n\n<b>Note:</b> This converts all Conjoined GamePrefs back to Dynamic.");
                    drawContainerInfo(90, methodTextColor("DeleteKey"), "Removes the Static GamePref from storage with the given key. \n\n<b>Note:</b> This will convert the GamePref of this key to Dynamic if it was a Conjoined GamePref previously.");
                    drawContainerInfo(90, methodTextColor("GetValue"), "Returns the value corresponding to a key if it exists.");
                    drawContainerInfo(90, methodTextColor("SetValue"), "Sets the value identified by a given key.");
                    drawContainerInfo(90, methodTextColor("HasGamePref"), "Returns true if the given Dynamic GamePref exists in storage, otherwise returns false.");
                    drawContainerInfo(90, methodTextColor("HasKey"), "Returns true if the given key exists in storage, otherwise returns false.");
                    drawContainerInfo(90, methodTextColor("HasID"), "Returns true if the given unique identifer exists in storage, otherwise returns false.");
                    drawContainerInfo(90, methodTextColor("Save"), $"Writes all modified data to disk. \n\n<b>Note:</b> <b>{methodTextColor("Save")}</b> is preemptively hooked to <b>{classTextColor("Application")}.quitting</b>");
                    drawContainerInfo(90, methodTextColor("Load"), "Loads the data from storage");
                    break;
            }


            GUILayout.Space(10);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        private void DrawToolbar()
        {
            Color color = GUI.color;
            Color selectedColor = SelectedGUIColor;
            Color deselectedColor = DeselectedGUIColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);


            Rect dropDownRect = GUILayoutUtility.GetRect(new GUIContent("Clear"), EditorStyles.toolbarDropDown);
            if (EditorGUI.DropdownButton(dropDownRect, new GUIContent("Clear"), FocusType.Keyboard))
            {
                GenericMenu contextMenu = new GenericMenu();
                contextMenu.AddItem(new GUIContent("Clear All"), false, delegate { GamePrefStorage.Instance.RemoveAll(); EditorUtility.SetDirty(GamePrefStorage.Instance); GamePref.DeleteAll(); GamePref.Save(); });
                contextMenu.AddItem(new GUIContent("Clear Dynamic GamePrefs"), false, delegate { GamePrefStorage.Instance.RemoveAll(); EditorUtility.SetDirty(GamePrefStorage.Instance); onGamePrefsUpdated?.Invoke(); });
                contextMenu.AddItem(new GUIContent("Clear Persistants"), false, delegate { GamePref.DeleteAll(); GamePref.Save(); });
                contextMenu.DropDown(dropDownRect);
            }

            GUILayout.FlexibleSpace();
            
            GUI.color = _isEditing ? selectedColor : deselectedColor;
            if (GUILayout.Button("Edit", EditorStyles.toolbarButton, GUILayout.MaxWidth(50)))
                _isEditing = !_isEditing;

            GUILayout.Space(1);

            GUI.color = _mainWindowTab == WindowTabs.Creator ? selectedColor : deselectedColor;
            if (GUILayout.Button(new GUIContent(_mainWindowTab == WindowTabs.Creator ? MinusTex : PlusTex), EditorStyles.toolbarButton))
            {
                if (_mainWindowTab == WindowTabs.Creator)
                {
                    _tempKey = "";
                    _tempType = default;
                    _tempValue = null;
                    _mainWindowTab = WindowTabs.None;
                }
                else _mainWindowTab = WindowTabs.Creator;
            }

            GUI.color = _mainWindowTab == WindowTabs.Settings ? selectedColor : deselectedColor;
            if (GUILayout.Button(new GUIContent(_mainWindowTab == WindowTabs.Settings ? ConfigOnTex : ConfigTex), EditorStyles.toolbarButton))
            {
                if (_mainWindowTab == WindowTabs.Settings)
                    _mainWindowTab = WindowTabs.None;
                else _mainWindowTab = WindowTabs.Settings;
            }

            GUI.color = _mainWindowTab == WindowTabs.Info ? selectedColor : deselectedColor;
            if (GUILayout.Button(new GUIContent(_mainWindowTab == WindowTabs.Info ? InfoOnTex : InfoTex), EditorStyles.toolbarButton))
            {
                if (_mainWindowTab == WindowTabs.Info)
                    _mainWindowTab = WindowTabs.None;
                else _mainWindowTab = WindowTabs.Info;
            }

            GUI.color = color;
            EditorGUILayout.EndHorizontal();
        }
        private void DrawGamePref(GamePrefType gamePrefType, GamePrefData dynamicData, GamePrefData staticData, string persistentID, string prefKey, object prefValue, bool isEditing, ref bool changed)
        {
            Event evt = Event.current;
            GUIContent labelContent = new GUIContent($"{prefKey}{(_debugMode ? $", ({prefValue.GetType().Name})" : "")}");

            Rect groupRect = EditorGUILayout.BeginVertical("box");
            Rect labelRect = GUILayoutUtility.GetRect(labelContent, EditorStyles.label, GUILayout.MaxWidth(position.width - 32));

            bool isEditingThisPref = false;
            if (isEditing && !_debugMode)
                if (_editingStatusesByIDs.TryGetValue($"{persistentID}.{gamePrefType}", out isEditingThisPref))
                    isEditingThisPref = _editingStatusesByIDs[$"{persistentID}.{gamePrefType}"] = EditorGUI.Foldout(labelRect, isEditingThisPref, labelContent.text, true);
                else _editingStatusesByIDs[$"{persistentID}.{gamePrefType}"] = false;
            else EditorGUI.LabelField(labelRect, labelContent);

            if (isEditingThisPref || _debugMode)
            {
                EditorGUI.indentLevel += 2;
                if (_debugMode)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("PersistentID"), GUILayout.Width(EditorGUIUtility.labelWidth));
                    EditorGUI.indentLevel -= 2;
                    EditorGUI.SelectableLabel(GUILayoutUtility.GetRect(new GUIContent(persistentID), WordWrapLabelStyle), persistentID, WordWrapLabelStyle);
                    EditorGUI.indentLevel += 2;
                    EditorGUILayout.EndHorizontal();
                }
                string newPrefKey = EditorGUILayout.DelayedTextField(new GUIContent("Key Name"), prefKey);
                if (newPrefKey != prefKey)
                {
                    if (gamePrefType.HasFlag(GamePrefType.Dynamic))
                    {
                        dynamicData.prefKey = newPrefKey;
                        GamePrefStorage.Instance.OverwriteGamePref(dynamicData);
                        EditorUtility.SetDirty(GamePrefStorage.Instance);
                        changed = true;
                    }
                    if (gamePrefType.HasFlag(GamePrefType.Static))
                    {
                        staticData.prefKey = newPrefKey;
                        GamePref.Save();
                    }
                }

                if (gamePrefType.HasFlag(GamePrefType.Dynamic))
                {
                    object defaultValue = EditorGUILayoutExtras.ObjectField(new GUIContent("Default Value"), dynamicData.prefValue);
                    if (!dynamicData.prefValue.Equals(defaultValue))
                    {
                        dynamicData.prefValue = defaultValue;
                        GamePrefStorage.Instance.OverwriteGamePref(dynamicData);
                        EditorUtility.SetDirty(GamePrefStorage.Instance);
                    }
                }
                if (gamePrefType.HasFlag(GamePrefType.Static))
                {
                    object value = EditorGUILayoutExtras.ObjectField(new GUIContent("Value"), staticData.prefValue);
                    if (!staticData.prefValue.Equals(value))
                    {
                        staticData.prefValue = value;
                        GamePref.Save();
                    }
                }
                EditorGUI.indentLevel -= 2;
            }

            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 1 && groupRect.Contains(Event.current.mousePosition))
                    {
                        void removeDynamic()
                        {
                            GamePrefStorage.Instance.Remove(persistentID);
                            EditorUtility.SetDirty(GamePrefStorage.Instance);
                            onGamePrefsUpdated?.Invoke();
                        }

                        void removePersistence()
                        {
                            _staticPrefsByIDs.Remove(persistentID);
                            _staticPrefsByKeys.Remove(prefKey);
                            GamePref.Save();
                        }
                        GenericMenu contextMenu = new GenericMenu();
                        if (gamePrefType.HasFlag(GamePrefType.Dynamic))
                        {
                            if (gamePrefType.HasFlag(GamePrefType.Static))
                            {
                                contextMenu.AddItem(new GUIContent("Remove/All"), false, delegate { removeDynamic(); if (gamePrefType.HasFlag(GamePrefType.Static)) removePersistence(); });
                                contextMenu.AddItem(new GUIContent("Remove/Dynamic"), false, removeDynamic);
                                contextMenu.AddItem(new GUIContent("Remove/Static"), false, removePersistence);
                            }
                            else contextMenu.AddItem(new GUIContent("Remove"), false, removeDynamic);
                        }
                        if (gamePrefType.HasFlag(GamePrefType.Static))
                        {
                            contextMenu.AddItem(new GUIContent("Remove"), false, removePersistence);
                            if (!gamePrefType.HasFlag(GamePrefType.Dynamic))
                            {
                                void convertToConjoined()
                                {
                                    GamePrefStorage.Instance.AddGamePrefData(new GamePrefData(staticData));
                                    EditorUtility.SetDirty(GamePrefStorage.Instance);
                                    onGamePrefsUpdated?.Invoke();
                                }
                                contextMenu.AddItem(new GUIContent("Convert to Conjoined"), false, convertToConjoined);
                            }
                        }
                        contextMenu.ShowAsContext();
                    }
                    break;
            }
            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
                PingLabel.SetCurrentLabel(labelContent, !isEditing || _debugMode ? labelRect : new Rect(labelRect.x + 13, labelRect.y, labelRect.width, labelRect.height));
        }

        private void LoadKeys()
        {
            var dynamicGamePrefs = GamePrefStorage.Instance.GetGamePrefs();
            _dynamicGamePrefsByIDs.Clear();
            _dynamicGamePrefsByKeys.Clear();
            foreach (var pair in dynamicGamePrefs)
            {
                _dynamicGamePrefsByIDs.Add(pair.Value.persistentIdentifier, pair.Value);
                _dynamicGamePrefsByKeys.Add(pair.Value.prefKey, pair.Value);
            }
            _staticPrefsByIDs = typeof(GamePref).GetField("_gamePrefsByIDs", BindingFlags.NonPublic | BindingFlags.Static).GetValue(typeof(GamePref)) as Dictionary<string, GamePrefData>;
            _staticPrefsByKeys = typeof(GamePref).GetField("_gamePrefsByKeys", BindingFlags.NonPublic | BindingFlags.Static).GetValue(typeof(GamePref)) as Dictionary<string, GamePrefData>;
            _sortedDynamics = new List<KeyValuePair<string, GamePrefData>>(_dynamicGamePrefsByIDs);
            _sortedDynamics.Sort(SortPairs);
            _sortedPersistants = new List<KeyValuePair<string, GamePrefData>>(_staticPrefsByIDs);
            _sortedPersistants.Sort(SortPairs);
            
            _queuedPrefs.Clear();
            HashSet<string> closed = new HashSet<string>();
            foreach (var pair in _sortedDynamics)
            {
                if (_staticPrefsByIDs.TryGetValue(pair.Key, out var otherValue))
                {
                    _queuedPrefs.Insert(2, ((pair.Key, pair.Value), (pair.Key, otherValue)));
                    closed.Add(pair.Key);
                }
                else
                {
                    _queuedPrefs.Insert(1, ((pair.Key, pair.Value), (null, null)));
                    closed.Add(pair.Key);
                }
            }
            foreach (var pair in _sortedPersistants)
                if (!closed.Contains(pair.Key))
                    _queuedPrefs.Insert(0, ((null, null), (pair.Key, pair.Value)));

            foreach (var directory in Directory.CreateDirectory(Application.dataPath).GetDirectories())
                foreach (var file in directory.GetFiles())
                    if (file.Name == "GamePrefStorage.asset")
                        _dynamicGamePrefsPath = file.FullName;

            DataManager.ContainsFile("GamePrefs", out _staticGamePrefsPath);
            Repaint();
        }

        internal static int SortPairs(KeyValuePair<string, GamePrefData> lhs, KeyValuePair<string, GamePrefData> rhs)
        {
            //int result = lhs.Value.prefValue.GetType().Name.CompareTo(rhs.Value.prefValue.GetType().Name);
            //if (result != 0)
            //    return result;
            //return lhs.Value.prefKey.CompareTo(rhs.Value.prefKey);

            int result = lhs.Value.prefKey.CompareTo(rhs.Value.prefKey);
            if (result == -1)
                return result;
            return lhs.Value.prefValue.GetType().Name.CompareTo(rhs.Value.prefValue.GetType().Name);
        }
        private object GetDefaultValueOfSupportedType(ValueType valueType)
        {
            switch (valueType)
            {
                case ValueType.Boolean:
                    return default(bool);
                case ValueType.Integer:
                    return default(int);
                case ValueType.Float:
                    return default(float);
                case ValueType.String:
                    return "";
                default:
                    return null;
            }
        }
        private ValueType ValueTypeFromType(Type type)
        {
            if (type == typeof(bool))
                return ValueType.Boolean;
            if (type == typeof(int))
                return ValueType.Integer;
            if (type == typeof(float))
                return ValueType.Float;
            if (type == typeof(string))
                return ValueType.String;
            else return (ValueType)(-1);
        }
        private bool TypeIsSupported(Type type, ValueType valueType)
        {
            switch (valueType)
            {
                case ValueType.Boolean:
                    return type == typeof(bool);
                case ValueType.Integer:
                    return type == typeof(int);
                case ValueType.Float:
                    return type == typeof(float);
                case ValueType.String:
                    return type == typeof(string);
                default:
                    return false;
            }
        }
    }
}

#endif
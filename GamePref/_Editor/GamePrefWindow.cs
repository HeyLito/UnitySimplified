using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.Collections;
using UnitySimplified.Serialization;
using UnitySimplified.RuntimeDatabases;

namespace UnitySimplifiedEditor.Serialization
{
    public class GamePrefWindow : EditorWindow
    {
        private enum ValueType { Boolean, Integer, Float, String }
        private enum GamePrefType { None = 1 << 1, Dynamic = 1 << 3, Static = 1 << 2 }
        private enum WindowTabs { None = default, Creator, Settings, Info, Description, ScriptingAPI }

        [SerializeField] private GamePref dummyPref;
        [SerializeField] private string dummyPrefName;

        private Dictionary<string, GamePrefData> _dynamicGamePrefsByIDs = new Dictionary<string, GamePrefData>();
        private Dictionary<string, GamePrefData> _dynamicGamePrefsByKeys = new Dictionary<string, GamePrefData>();

        private Dictionary<string, string> _staticIdentifiersByKeys = new();
        private Dictionary<string, GamePrefData> _staticGamePrefsByIdentifiers = new();

        private SerializableDictionary<string, bool> _propertiesInEditByIDs = new SerializableDictionary<string, bool>();
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
        public Texture2D ConfigTex => _configTex = _configTex != null ? _configTex : EditorGUIUtility.Load("icons/d_TerrainInspector.TerrainToolSettings On.png") as Texture2D;
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
            GamePref.onGamePrefUpdated += LoadKeys;
            onGamePrefsUpdated += LoadKeys;
            GamePref.Load();
            EditorGUIExtended.OnPingBegan += () =>
            {
                _mainWindowTab = WindowTabs.Creator;
                _scrollPos.y = Mathf.Max(0, (EditorGUIExtended.PingPosition.y + EditorGUIExtended.PingPosition.height + _scrollViewRect.height / 2) - _scrollViewRect.height);
            };
        }
        private void OnDisable() => GamePref.onGamePrefUpdated -= LoadKeys;

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
                for (int i = 0, count = tempQueuedPrefs.Count; i < count; i++)
                {
                    if (!tempQueuedPrefs.Pop(out KeyValuePair<int, ((string, GamePrefData), (string, GamePrefData))> pop))
                        continue;
                    GUIContent labelContent = new();
                    string persistentIdentifier = "";
                    string prefKey = "";
                    object prefValue = null;
                    GamePrefType previousType = gamePrefType;

                    switch (pop.Key)
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
                            persistentIdentifier = pop.Value.Item1.Item1;
                            prefKey = pop.Value.Item1.Item2.Key;
                            prefValue = pop.Value.Item1.Item2.Value;
                        }
                        else if (gamePrefType.HasFlag(GamePrefType.Static))
                        {
                            persistentIdentifier = pop.Value.Item2.Item1;
                            prefKey = pop.Value.Item2.Item2.Key;
                            prefValue = pop.Value.Item2.Item2.Value;
                        }
                        DrawGamePref(gamePrefType, pop.Value.Item1.Item2, pop.Value.Item2.Item2, persistentIdentifier, prefKey, prefValue, _isEditing, ref changed);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            dummyPrefName = EditorGUIExtended.EditableLabel(EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight), dummyPrefName);

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

            GamePrefData staticPrefData = null;
            bool dynamicExists = _dynamicGamePrefsByKeys.ContainsKey(_tempKey);
            bool staticExists = _staticIdentifiersByKeys.TryGetValue(_tempKey, out string staticIdentifier) && _staticGamePrefsByIdentifiers.TryGetValue(staticIdentifier, out staticPrefData);
            bool staticExistsWithDifferentType = staticExists && !TypeIsSupported(staticPrefData.ValueType, _tempType);

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
            _tempValue = EditorGUILayoutExtended.ObjectField(new GUIContent("Value"), _tempValue, _tempValue.GetType());

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_tempKey) || dynamicExists || staticExistsWithDifferentType);
            if (GUILayout.Button("Create GamePref", GUILayout.Width(Mathf.Max(position.width * 0.45f - 25, 120))))
            {
                if (_tempValue == null)
                    throw new Exception($"The value of the newly created {typeof(GamePref).Name} is NULL.");

                if (staticExists)
                    GamePrefStorage.Instance.AddGamePrefData(CreateGamePrefData(staticPrefData.Identifier, _tempKey, _tempValue));
                else GamePrefStorage.Instance.AddGamePrefData(CreateGamePrefData(GetNewPref().Identifier, _tempKey, _tempValue));
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

            string classTextColor(string text) => $"<color=#68c4ac>{text}</color>";
            string methodTextColor(string text) => $"<color=#e0dcac>{text}</color>";
            string keywordTextColor(string text) => $"<color=#529cc8>{text}</color>";
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
                    EditorGUILayout.LabelField($"GamePrefs are the visual alternative to Unity's <b>{classTextColor("PlayerPrefs")}</b>, " +
                                                $"yet also retaining the same functionality. " +
                                                $"GamePrefs are conveniently serialized to the editor, granting users ease-of-access to change its saved data. ", WordWrapLabelStyle);
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"GamePref data is stored in two different container types: <b>Static</b> and <b>Dynamic</b>.", WordWrapLabelStyle);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 2);

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("Static GamePrefs", style);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"GamePrefs of type Static contain data that is written to and read from a file. " +
                                                $"The creation of a Static GamePref is made only at runtime by invoking the static function <b>{classTextColor("GamePref")}</b>.<b>{methodTextColor("SetValue")}({keywordTextColor("string")} key)</b>, " +
                                                $"or by accessing the <b>{methodTextColor("SetValue")}()</b> function from a GamePref variable.", WordWrapLabelStyle);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 2);


                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("Dynamic GamePrefs", style);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"GamePrefs of type Dynamic contain data that is stored and retrieved locally from a <b>{classTextColor("ScriptableObject")}</b> asset file, therefore containing no runtime saved data. " +
                                                $"Because of this, Dynamic GamePrefs only serve as an alternative method to reference Static GamePrefs, or stored data. " +
                                                $"The creation of a Dynamic GamePref could only be made in edit time and only through this window.", WordWrapLabelStyle);
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField($"When a GamePref is serialized as a field, the Unity Editor allows reference attachment to an Dynamic GamePref.", WordWrapLabelStyle);
                    EditorGUILayout.PropertyField(new SerializedObject(this).FindProperty("dummyPref"), new GUIContent("Example GamePref"));
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
                contextMenu.AddItem(new GUIContent("Clear Static GamePrefs"), false, delegate { GamePref.DeleteAll(); GamePref.Save(); });
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
        private void DrawGamePref(GamePrefType gamePrefType, GamePrefData dynamicData, GamePrefData staticData, string identifier, string key, object prefValue, bool isEditing, ref bool changed)
        {
            Event evt = Event.current;
            GUIContent labelContent = new GUIContent($"{key}{(_debugMode ? $", ({prefValue.GetType().Name})" : "")}");

            Rect groupRect = EditorGUILayout.BeginVertical("box");
            Rect labelRect = GUILayoutUtility.GetRect(labelContent, EditorStyles.label, GUILayout.MaxWidth(position.width - 32));

            bool isEditingThisPref = false;
            if (isEditing && !_debugMode)
                if (_propertiesInEditByIDs.TryGetValue($"{identifier}.{gamePrefType}", out isEditingThisPref))
                    isEditingThisPref = _propertiesInEditByIDs[$"{identifier}.{gamePrefType}"] = EditorGUI.Foldout(labelRect, isEditingThisPref, labelContent.text, true);
                else _propertiesInEditByIDs[$"{identifier}.{gamePrefType}"] = false;
            else EditorGUI.LabelField(labelRect, labelContent);

            if (isEditingThisPref || _debugMode)
            {
                EditorGUI.indentLevel += 2;
                if (_debugMode)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("PersistentID"), GUILayout.Width(EditorGUIUtility.labelWidth));
                    EditorGUI.indentLevel -= 2;
                    EditorGUI.SelectableLabel(GUILayoutUtility.GetRect(new GUIContent(identifier), WordWrapLabelStyle), identifier, WordWrapLabelStyle);
                    EditorGUI.indentLevel += 2;
                    EditorGUILayout.EndHorizontal();
                }
                string newKey = EditorGUILayout.DelayedTextField(new GUIContent("Key Name"), key);
                if (newKey != key)
                {
                    if (gamePrefType.HasFlag(GamePrefType.Dynamic))
                    {
                        dynamicData.Key = newKey;
                        GamePrefStorage.Instance.OverwriteGamePref(dynamicData);
                        EditorUtility.SetDirty(GamePrefStorage.Instance);
                        changed = true;
                    }
                    if (gamePrefType.HasFlag(GamePrefType.Static))
                    {
                        staticData.Key = newKey;
                        GamePref.Save();
                    }
                }

                if (gamePrefType.HasFlag(GamePrefType.Dynamic))
                {
                    object defaultValue = EditorGUILayoutExtended.ObjectField(new GUIContent("Default Value"), dynamicData.Value, dynamicData.Value.GetType());
                    if (!dynamicData.Value.Equals(defaultValue))
                    {
                        dynamicData.Value = defaultValue;
                        GamePrefStorage.Instance.OverwriteGamePref(dynamicData);
                        EditorUtility.SetDirty(GamePrefStorage.Instance);
                    }
                }
                if (gamePrefType.HasFlag(GamePrefType.Static))
                {
                    object value = EditorGUILayoutExtended.ObjectField(new GUIContent("Value"), staticData.Value, staticData.Value.GetType());
                    if (!staticData.Value.Equals(value))
                    {
                        staticData.Value = value;
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
                            GamePrefStorage.Instance.Remove(identifier);
                            EditorUtility.SetDirty(GamePrefStorage.Instance);
                            onGamePrefsUpdated?.Invoke();
                        }

                        void removePersistence()
                        {
                            _staticGamePrefsByIdentifiers.Remove(identifier);
                            _staticIdentifiersByKeys.Remove(key);
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
                                void ConvertToConjoined()
                                {
                                    GamePrefStorage.Instance.AddGamePrefData(CreateGamePrefData(staticData));
                                    EditorUtility.SetDirty(GamePrefStorage.Instance);
                                    onGamePrefsUpdated?.Invoke();
                                }
                                contextMenu.AddItem(new GUIContent("Convert to Conjoined"), false, ConvertToConjoined);
                            }
                        }
                        contextMenu.ShowAsContext();
                    }
                    break;
            }
            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint && labelContent.text == _pingPrefKey)
            {
                bool canStart = _wantsToPing;
                bool canDraw = EditorGUIExtended.IsPinging;
                Rect pingPosition = default;
                if (canStart || canDraw)
                {
                    pingPosition = !isEditing || _debugMode
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


        private static GamePref GetNewPref() => typeof(GamePref).GetMethod("Create", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[0]) as GamePref;
        private static GamePrefData CreateGamePrefData(string persistentIdentifier, string prefKey, object prefValue) => Activator.CreateInstance(typeof(GamePrefData), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { persistentIdentifier, prefKey, prefValue }, null, null) as GamePrefData;
        private static GamePrefData CreateGamePrefData(GamePrefData gamePrefData) => Activator.CreateInstance(typeof(GamePrefData), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { gamePrefData }, null, null) as GamePrefData;
        private void LoadKeys()
        {
            var dynamicGamePrefs = GamePrefStorage.Instance.GetGamePrefs();
            _dynamicGamePrefsByIDs.Clear();
            _dynamicGamePrefsByKeys.Clear();
            foreach (var pair in dynamicGamePrefs)
            {
                _dynamicGamePrefsByIDs.Add(pair.Value.Identifier, pair.Value);
                _dynamicGamePrefsByKeys.Add(pair.Value.Key, pair.Value);
            }
            _staticIdentifiersByKeys = typeof(GamePref).GetField("IdentifiersByKeys", BindingFlags.NonPublic | BindingFlags.Static).GetValue(typeof(GamePref)) as Dictionary<string, string>;
            _staticGamePrefsByIdentifiers = typeof(GamePref).GetField("GamePrefsByIdentifiers", BindingFlags.NonPublic | BindingFlags.Static).GetValue(typeof(GamePref)) as Dictionary<string, GamePrefData>;
            _sortedDynamics = new List<KeyValuePair<string, GamePrefData>>(_dynamicGamePrefsByIDs);
            _sortedDynamics.Sort(SortPairs);
            _sortedPersistants = new List<KeyValuePair<string, GamePrefData>>(_staticGamePrefsByIdentifiers);
            _sortedPersistants.Sort(SortPairs);
            
            _queuedPrefs.Clear();
            HashSet<string> closed = new HashSet<string>();
            foreach (var pair in _sortedDynamics)
            {
                if (_staticGamePrefsByIdentifiers.TryGetValue(pair.Key, out var otherValue))
                {
                    _queuedPrefs.Add(2, ((pair.Key, pair.Value), (pair.Key, otherValue)));
                    closed.Add(pair.Key);
                }
                else
                {
                    _queuedPrefs.Add(1, ((pair.Key, pair.Value), (null, null)));
                    closed.Add(pair.Key);
                }
            }
            foreach (var pair in _sortedPersistants)
                if (!closed.Contains(pair.Key))
                    _queuedPrefs.Add(0, ((null, null), (pair.Key, pair.Value)));

            foreach (var directory in Directory.CreateDirectory(Application.dataPath).GetDirectories())
                foreach (var file in directory.GetFiles())
                    if (file.Name == "GamePrefStorage.asset")
                        _dynamicGamePrefsPath = file.FullName;

            DataManager.ContainsFile("GamePrefs", out _, out _staticGamePrefsPath, out _);
            Repaint();
        }

        private bool _wantsToPing;
        private string _pingPrefKey;
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

            int result = lhs.Value.Key.CompareTo(rhs.Value.Key);
            if (result == -1)
                return result;
            return lhs.Value.Value.GetType().Name.CompareTo(rhs.Value.Value.GetType().Name);
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
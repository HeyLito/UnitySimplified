#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnitySimplified.Audio;

namespace UnitySimplifiedEditor.Audio
{
    [CustomEditor(typeof(SequenceAudioEvent), true)]
    class SequenceAudioEventEditor : Editor
    {
        private SerializedProperty _fadeInCurveProp;
        private SerializedProperty _fadeOutCurveProp;
        private SerializedProperty _entriesProp;
        private AudioEventHelper _previewer;
        private ReorderableList _list;
        private GUIStyle _labelStyle;
        private bool _undoPreformed;
        private int _arrayCount;

        private GUIStyle LabelStyle => _labelStyle ??= new GUIStyle(EditorStyles.label) { richText = true };

        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();

            if (!_previewer)
                return;

            Rect position = GUILayoutUtility.GetLastRect();
            position.y += 4;
            position.xMin += 2;
            position.xMax -= 2;
            Rect iconRect = new(position) { width = 40, height = 40 };
            Rect nameRect = new(position) { x = iconRect.xMax + 4, width = LabelStyle.CalcSize(new GUIContent(serializedObject.targetObject.name)).x, height = EditorGUIUtility.singleLineHeight };
            Rect rightButtonsRect = new(position) { x = position.xMax - 45 - 2, xMax = position.xMax, height = 40 };
            Rect customLabelRect = new(position) { x = iconRect.xMax + 10, xMax = rightButtonsRect.xMin - 10, y = nameRect.yMax + 2, height = nameRect.height };

            //EditorGUI.DrawRect(iconRect, new Color(1, 1, 0, 0.5f));
            //EditorGUI.DrawRect(nameRect, new Color(0f, 1f, 1, 0.5f));
            //EditorGUI.DrawRect(rightButtonsRect, new Color(0, 1, 0, 0.5f));
            //EditorGUI.DrawRect(customLabelRect, new Color(.75f, .75f, 0, 0.5f));

            static string ContentText(string title, string message) => $"<i>{title}</i>:  <color=lime><b>{message}</b></color>";

            var customLabelContent = new GUIContent();
            if (_previewer && _previewer.IsPlaying)
            {
                if (_previewer.IsStopping)
                {
                    customLabelContent.text = ContentText("Now Stopping", "Sequence");
                    Repaint();
                }
                else customLabelContent.text = ContentText("Now Playing", "Sequence");
            }

            EditorGUI.LabelField(customLabelRect, customLabelContent, LabelStyle);
        }

        private void OnEnable()
        {
            _fadeInCurveProp = serializedObject.FindProperty("fadeInCurve");
            _fadeOutCurveProp = serializedObject.FindProperty("fadeOutCurve");
            _entriesProp = serializedObject.FindProperty("entries");
            Undo.undoRedoPerformed += OnUndoPreformed;

            _list = new ReorderableList(_entriesProp.serializedObject, _entriesProp)
            {
                elementHeightCallback = ElementHeightCallback,
                drawHeaderCallback = DrawHeaderCallback,
                drawElementCallback = DrawElementCallback,
            };
        }
        private void OnDisable()
        {
            if (_previewer)
            {
                _previewer.StopImmediately();
                DestroyImmediate(_previewer.gameObject);
                _previewer = null;
            }
            Undo.undoRedoPerformed -= OnUndoPreformed;
        }

        private void OnUndoPreformed() => _undoPreformed = true;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(_previewer && _previewer.IsPlaying);
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Fade In/Out Curves");
            EditorGUILayout.PropertyField(_fadeInCurveProp, GUIContent.none, true);
            EditorGUILayout.PropertyField(_fadeOutCurveProp, GUIContent.none, true);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            _arrayCount = _entriesProp.arraySize;
            EditorGUI.BeginChangeCheck();
#if UNITY_2020_1_OR_NEWER
            _list.DoLayoutList();
#else
            EditorGUILayout.PropertyField(_entriesProp);
#endif
            if (EditorGUI.EndChangeCheck() && _arrayCount < _entriesProp.arraySize)
            {
                _entriesProp.serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(_entriesProp.serializedObject.targetObject, "Set New Entries To Default Constructors");
                for (int i = _arrayCount; i < _entriesProp.arraySize; i++)
                {
                    (System.Reflection.FieldInfo, object) fieldInfoWrapper = _entriesProp.GetArrayElementAtIndex(i).ExposePropertyInfo(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, out _);
                    var iList = fieldInfoWrapper.Item1.GetValue(fieldInfoWrapper.Item2) as IList<SequenceAudioEntry>;
                    iList[i] = new SequenceAudioEntry();
                    fieldInfoWrapper.Item1.SetValue(fieldInfoWrapper.Item2, iList);
                }
            }
            EditorGUI.EndDisabledGroup();

            bool hasPreviewer = _previewer;
            if (hasPreviewer && !_previewer.IsPlaying && !_previewer.IsStopping)
            {
                DestroyImmediate(_previewer.gameObject);
                _previewer = null;
                hasPreviewer = false;
            }

            using (new EditorGUI.DisabledGroupScope(serializedObject.isEditingMultipleObjects))
            {
                using (new EditorGUI.DisabledGroupScope(hasPreviewer))
                {
                    if (GUILayout.Button("Preview"))
                    {
                        _previewer = ((AudioEvent)target).Play(default);
                        hasPreviewer = true;
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!hasPreviewer || !_previewer.IsPlaying || _previewer.IsStopping))
                {
                    if (GUILayout.Button("Stop"))
                    {
                        if (hasPreviewer)
                            _previewer.Stop();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!hasPreviewer || !_previewer.IsPlaying))
                {
                    if (GUILayout.Button("Stop Immediately"))
                    {
                        if (hasPreviewer)
                            _previewer.StopImmediately();
                    }
                }
            }

            if (EditorGUI.EndChangeCheck() || _undoPreformed)
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                _undoPreformed = false;
            }
        }
        private float ElementHeightCallback(int index) => EditorGUI.GetPropertyHeight(_list.serializedProperty.GetArrayElementAtIndex(index));
        private void DrawHeaderCallback(Rect rect) => EditorGUI.LabelField(new Rect(rect) { x = rect.x - 2 }, _list.serializedProperty.displayName);
        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused) => EditorGUI.PropertyField(rect, _list.serializedProperty.GetArrayElementAtIndex(index), true);
    }
}

#endif
//#if UNITY_EDITOR

//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using UnityEditorInternal;
//using UnitySimplified.Audio;

//namespace UnitySimplifiedEditor.Audio
//{
//    [CustomEditor(typeof(SequenceAudioEvent), true)]
//    class SequenceAudioEventEditor : Editor
//    {
//        private (GameObject container, AudioSource source, AudioReverbFilter filter, SequenceAudioEventHelper helper) _previewer;
//        private SerializedProperty _fadeInCurveProp;
//        private SerializedProperty _fadeOutCurveProp;
//        private SerializedProperty _entriesProp;
//        private ReorderableList _list;
//        private bool _undoPreformed;
//        private int _arrayCount;
//        private GUIStyle _labelStyle;
//        private Func<bool> _canRepaint;

//        [NonSerialized]
//        internal static SequenceAudioEntry audioSampleRequest;

//        private GUIStyle LabelStyle => _labelStyle ??= new GUIStyle(EditorStyles.label) { richText = true };

//        protected override void OnHeaderGUI()
//        {
//            base.OnHeaderGUI();

//            if (audioSampleRequest == null && !_previewer.helper)
//                return;

//            Rect position = GUILayoutUtility.GetLastRect();
//            position.y += 4;
//            position.xMin += 2;
//            position.xMax -= 2;
//            Rect iconRect = new(position) { width = 40, height = 40 };
//            Rect nameRect = new(position) { x = iconRect.xMax + 4, width = LabelStyle.CalcSize(new GUIContent(serializedObject.targetObject.name)).x, height = EditorGUIUtility.singleLineHeight };
//            Rect rightButtonsRect = new(position) { x = position.xMax - 45 - 2, xMax = position.xMax, height = 40};
//            Rect customLabelRect = new(position) { x = iconRect.xMax + 10, xMax = rightButtonsRect.xMin - 10, y = nameRect.yMax + 2, height = nameRect.height};

//            //EditorGUI.DrawRect(iconRect, new Color(1, 1, 0, 0.5f));
//            //EditorGUI.DrawRect(nameRect, new Color(0f, 1f, 1, 0.5f));
//            //EditorGUI.DrawRect(rightButtonsRect, new Color(0, 1, 0, 0.5f));
//            //EditorGUI.DrawRect(customLabelRect, new Color(.75f, .75f, 0, 0.5f));

//            static string ContentText(string title, string message) => $"<i>{title}</i>:  <color=lime><b>{message}</b></color>";

//            var customLabelContent = new GUIContent();
//            if (audioSampleRequest != null)
//                customLabelContent.text = ContentText("Now Playing", audioSampleRequest.Clip.name);
//            else if (_previewer.helper && _previewer.helper.IsPlaying)
//            {
//                if (_previewer.helper.IsFadingIn)
//                    customLabelContent.text = ContentText("Fading-In", "Sequence");
//                else if (_previewer.Item4.IsFadingOut)
//                    customLabelContent.text = ContentText("Fading-Out", "Sequence");
//                else customLabelContent.text = ContentText("Now Playing", "Sequence");
//            }

//            EditorGUI.LabelField(customLabelRect, customLabelContent, LabelStyle);
//        }

//        private void OnEnable()
//        {
//            _previewer.container = EditorUtility.CreateGameObjectWithHideFlags("Audio Previewer", HideFlags.HideAndDontSave);
//            _previewer.source = _previewer.container.AddComponent<AudioSource>();
//            _previewer.filter = _previewer.container.AddComponent<AudioReverbFilter>();
//            _fadeInCurveProp = serializedObject.FindProperty("fadeInCurve");
//            _fadeOutCurveProp = serializedObject.FindProperty("fadeOutCurve");
//            _entriesProp = serializedObject.FindProperty("entries");
//            Undo.undoRedoPerformed += OnUndoPreformed;

//            _list = new ReorderableList(_entriesProp.serializedObject, _entriesProp)
//            {
//                elementHeightCallback = ElementHeightCallback,
//                drawHeaderCallback = DrawHeaderCallback,
//                drawElementCallback = DrawElementCallback,
//            };
//        }
//        private void OnDisable()
//        {
//            StopEntryAudioSample();
//            if (_previewer.helper)
//                _previewer.helper.StopImmediately();
//            DestroyImmediate(_previewer.container);
//            Undo.undoRedoPerformed -= OnUndoPreformed;
//        }

//        private void OnUndoPreformed() => _undoPreformed = true;

//        public override void OnInspectorGUI()
//        {
//            EditorGUI.BeginChangeCheck();
//            EditorGUI.BeginDisabledGroup(audioSampleRequest != null || (_previewer.helper && _previewer.helper.IsPlaying));
//            EditorGUILayout.BeginHorizontal("box");
//            EditorGUILayout.LabelField("Fade In/Out Curves");
//            EditorGUILayout.PropertyField(_fadeInCurveProp, GUIContent.none, true);
//            EditorGUILayout.PropertyField(_fadeOutCurveProp, GUIContent.none, true);
//            EditorGUILayout.EndHorizontal();
//            GUILayout.Space(10);

//            _arrayCount = _entriesProp.arraySize;
//            EditorGUI.BeginChangeCheck();
//#if UNITY_2020_1_OR_NEWER
//            _list.DoLayoutList();
//#else
//            EditorGUILayout.PropertyField(_entriesProp);
//#endif
//            if (EditorGUI.EndChangeCheck() && _arrayCount < _entriesProp.arraySize)
//            {
//                _entriesProp.serializedObject.ApplyModifiedProperties();
//                Undo.RecordObject(_entriesProp.serializedObject.targetObject, "Set New Entries To Default Constructors");
//                for (int i = _arrayCount; i < _entriesProp.arraySize; i++)
//                {
//                    (System.Reflection.FieldInfo, object) fieldInfoWrapper = _entriesProp.GetArrayElementAtIndex(i).ExposePropertyInfo(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, out _);
//                    var iList = fieldInfoWrapper.Item1.GetValue(fieldInfoWrapper.Item2) as IList<SequenceAudioEntry>;
//                    iList[i] = new SequenceAudioEntry();
//                    fieldInfoWrapper.Item1.SetValue(fieldInfoWrapper.Item2, iList);
//                }
//            }

//            if (GUILayout.Button("Preview"))
//                HandleAmbianceAudioSample();
//            EditorGUI.EndDisabledGroup();

//            HandleEntryAudioSample();

//            EditorGUI.BeginDisabledGroup(audioSampleRequest == null && (!_previewer.helper || !_previewer.helper.IsPlaying || _previewer.helper.IsFadingOut));
//            if (GUILayout.Button("Stop"))
//                StopAllAudioSamples();
//            EditorGUI.EndDisabledGroup();

//            if (EditorGUI.EndChangeCheck() || _undoPreformed)
//            {
//                serializedObject.ApplyModifiedProperties();
//                serializedObject.Update();
//                _undoPreformed = false;
//            }

//            if (_canRepaint != null && _canRepaint.Invoke())
//                Repaint();
//        }

//        private void StopAllAudioSamples()
//        {
//            StopAmbianceAudioSample();
//            StopEntryAudioSample();
//        }

//        private void StopAmbianceAudioSample()
//        {
//            if (!_previewer.helper)
//                return;

//            _previewer.helper.Stop();
//            _canRepaint ??= () =>
//            {
//                if (!_previewer.helper.IsFadingIn && !_previewer.helper.IsFadingOut)
//                    _canRepaint = null;
//                return _previewer.helper.IsFadingIn || _previewer.helper.IsFadingOut;
//            };
//        }
//        private void StopEntryAudioSample()
//        {
//            if (audioSampleRequest == null)
//                return;

//            _previewer.source.clip = null;
//            audioSampleRequest = null;
//        }

//        private void HandleAmbianceAudioSample()
//        {
//            if (!_previewer.helper)
//                _previewer.helper = _previewer.container.AddComponent<SequenceAudioEventHelper>();
//            _previewer.helper.InitializeTarget(target as SequenceAudioEvent, () =>
//            {
//                var obj = EditorUtility.CreateGameObjectWithHideFlags("Sub Audio Sampler", HideFlags.DontSave);
//                var source = obj.AddComponent<AudioSource>();
//                var reverb = obj.AddComponent<AudioReverbFilter>();
//                source.playOnAwake = false;
//                reverb.reverbPreset = AudioReverbPreset.Off;
//                obj.transform.SetParent(_previewer.container.transform);
//                obj.transform.localPosition = Vector3.zero;
//                return obj;
//            });
//            _previewer.helper.Play();

//            _canRepaint ??= () => { if (!_previewer.helper.IsFadingIn && !_previewer.helper.IsFadingOut) _canRepaint = null; return _previewer.helper.IsFadingIn || _previewer.helper.IsFadingOut; };
//        }
//        private void HandleEntryAudioSample()
//        {
//            if (audioSampleRequest == null)
//                return;

//            if (_previewer.source && _previewer.source.clip == null)
//            {
//                _previewer.source.playOnAwake = false;
//                AudioEventUtility.ApplyAudioSourceValues(_previewer.source, audioSampleRequest);
//                AudioEventUtility.ApplyAudioReverbValues(_previewer.filter, audioSampleRequest);
//                _previewer.source.Play();
//            }
//            else if (!_previewer.source || _previewer.source.clip == audioSampleRequest.Clip && !_previewer.source.isPlaying)
//            {
//                if (_previewer.source)
//                    _previewer.source.clip = null;
//                audioSampleRequest = null;
//            }
//        }

//        private float ElementHeightCallback(int index) => EditorGUI.GetPropertyHeight(_list.serializedProperty.GetArrayElementAtIndex(index));
//        private void DrawHeaderCallback(Rect rect) => EditorGUI.LabelField(new Rect(rect) { x = rect.x - 2 }, _list.serializedProperty.displayName);
//        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused) => EditorGUI.PropertyField(rect, _list.serializedProperty.GetArrayElementAtIndex(index));
//    }
//}

//#endif
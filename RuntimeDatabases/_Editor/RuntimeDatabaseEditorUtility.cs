#if UNITY_EDITOR

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityObject = UnityEngine.Object;

namespace UnitySimplifiedEditor.RuntimeDatabases
{
    public static class RuntimeDatabaseEditorUtility
    {
        private static GUIStyle _boxStyle;
        private static GUIStyle _idStyle;
        private static GUIStyle _unityObjectStyle;
        private static GUIStyle _unityObjectErrorStyle;


        public static GUIStyle BoxStyle
        {
            get
            {
                if (_boxStyle != null)
                    return _boxStyle;
                _boxStyle = new GUIStyle(GUI.skin.textArea);
                _boxStyle.fontSize = 10;
                _boxStyle.normal.textColor = new Color(1, 1f, 0.5f, 1);
                _boxStyle.onNormal.textColor = _boxStyle.normal.textColor;
                _boxStyle.focused.textColor = _boxStyle.normal.textColor;
                _boxStyle.onFocused.textColor = _boxStyle.normal.textColor;
                _boxStyle.hover.textColor = _boxStyle.normal.textColor;
                _boxStyle.onHover.textColor = _boxStyle.normal.textColor;
                _boxStyle.active.textColor = new Color(_boxStyle.normal.textColor.r - 0.2f, _boxStyle.normal.textColor.g - 0.2f, _boxStyle.normal.textColor.b - 0.2f, _boxStyle.normal.textColor.a);
                _boxStyle.onActive.textColor = _boxStyle.active.textColor;
                _boxStyle.alignment = TextAnchor.UpperLeft;
                return _boxStyle;
            }
        }
        public static GUIStyle IDStyle
        {
            get
            {
                if (_idStyle != null)
                    return _idStyle;
                _idStyle = new GUIStyle();
                _idStyle.fontSize = 10;
                _idStyle.normal.textColor = Color.white;
                _idStyle.richText = true;
                _idStyle.alignment = TextAnchor.MiddleLeft;
                _idStyle.wordWrap = true;
                return _idStyle;
            }
        }
        public static GUIStyle UnityObjectStyle
        {
            get
            {
                if (_unityObjectStyle != null)
                    return _unityObjectStyle;
                _unityObjectStyle = new GUIStyle();
                _unityObjectStyle.fontSize = 10;
                _unityObjectStyle.normal.textColor = new Color(0.25f, 0.65f, 1f, 1);
                _unityObjectStyle.richText = true;
                _unityObjectStyle.alignment = TextAnchor.MiddleLeft;
                _unityObjectStyle.wordWrap = true;
                return _unityObjectStyle;
            }
        }
        public static GUIStyle UnityObjectErrorStyle
        {
            get
            {
                if (_unityObjectErrorStyle != null)
                    return _unityObjectErrorStyle;
                _unityObjectErrorStyle = new GUIStyle();
                _unityObjectErrorStyle.fontSize = 10;
                _unityObjectErrorStyle.normal.textColor = new Color(1, 0.65f, 0.25f, 1);
                _unityObjectErrorStyle.richText = true;
                _unityObjectErrorStyle.alignment = TextAnchor.MiddleLeft;
                return _unityObjectErrorStyle;
            }
        }


        public static IEnumerable<UnityObject> GetBuiltInAssets()
        {
            var assets = new List<UnityObject>();
            assets.AddRange(new[]
            {
                "Cube.fbx",
                "Capsule.fbx",
                "Cylinder.fbx",
                "Plane.fbx",
                "Sphere.fbx",
                "Quad.fbx",
            }.Select(Resources.GetBuiltinResource<Mesh>).Where(resource => resource != null));
            assets.AddRange(new[]
            {
                // --- Missing from function getter? ---
                // "FrameDebuggerRenderTargetDisplay.mat"
                // "SpatialMappingOcclusion.mat"
                // "SpatialMappingWireframe.mat"
                // -------------------------------------
                "Default-Diffuse.mat",
                "Default-Line.mat",
                "Default-Material.mat",
                "Default-Particle.mat",
                "Default-ParticleSystem.mat",
                "Default-Skybox.mat",
                "Default-Terrain-Diffuse.mat",
                "Default-Terrain-Specular.mat",
                "Default-Terrain-Standard.mat",
                "Sprites-Default.mat",
                "Sprites-Mask.mat",
            }.Select(AssetDatabase.GetBuiltinExtraResource<Material>).Where(resource => resource != null));
            assets.AddRange(new[]
            {
                "UI/Skin/UISprite.psd",
                "UI/Skin/Background.psd",
                "UI/Skin/InputFieldBackground.psd",
                "UI/Skin/Knob.psd",
                "UI/Skin/Checkmark.psd",
                "UI/Skin/DropdownArrow.psd",
                "UI/Skin/UIMask.psd",
            }.Select(AssetDatabase.GetBuiltinExtraResource<Sprite>).Where(resource => resource != null));
            return assets;
        }
        public static IEnumerable<UnityObject> GetAssetsInDirectories(params string[] extensions)
        {
            var assets = new List<UnityObject>();
            var assetsFromDirectories =
                Directory.EnumerateFiles("Assets", "*", SearchOption.AllDirectories)
                    .Where(file => extensions.Any(extension => extension.StartsWith(".") && file.ToLower().EndsWith(extension)));
            assets.AddRange(assetsFromDirectories.Select(AssetDatabase.LoadAssetAtPath<UnityObject>).Where(asset => asset));
            return assets;
        }

        public static ReorderableList ReorderableListTemplate(IList elements, Type elementsType, string headerText, Action actionOnClear = null, Action actionOnRepopulate = null) =>
            new(elements, elementsType, true, true, false, true)
            {
                multiSelect = true,
                drawHeaderCallback = position =>
                {
                    EditorGUI.LabelField(position, headerText);

                    GUIContent repopulateButtonContent = new("Repopulate");
                    GUIContent clearButtonContent = new("Clear");

                    float repopulateButtonWidth = EditorStyles.toolbarButton.CalcSize(repopulateButtonContent).x;
                    float clearButtonWidth = EditorStyles.toolbarButton.CalcSize(clearButtonContent).x;

                    Rect repopulateButtonRect = new(position)
                        { x = position.x + position.width - repopulateButtonWidth, width = repopulateButtonWidth };
                    Rect clearButtonRect = new(position)
                        { x = repopulateButtonRect.x - clearButtonWidth, width = clearButtonWidth };

                    if (GUI.Button(repopulateButtonRect, repopulateButtonContent, EditorStyles.toolbarButton))
                        actionOnRepopulate?.Invoke();
                    if (GUI.Button(clearButtonRect, clearButtonContent, EditorStyles.toolbarButton))
                        actionOnClear?.Invoke();
                }
            };
    }
}

#endif
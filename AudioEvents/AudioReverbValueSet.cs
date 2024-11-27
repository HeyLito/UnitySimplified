using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnitySimplified.Audio;
#endif

namespace UnitySimplified.Audio
{
    [Serializable]
	public struct AudioReverbValueSet : IAudioReverbTemplate
	{
		[SerializeField]
        private AudioReverbPreset preset;
		[SerializeField, Range(-10000, 0)]
        private float dryLevel;
		[SerializeField, Range(-10000, 0)]
        private float room;
		[SerializeField, Range(-10000, 0)]
        private float roomHF;
		[SerializeField, Range(-10000, 0)]
        private float roomLF;
		[SerializeField, Range(0.1f, 20)]
        private float decayTime;
		[SerializeField, Range(0.1f, 2)]
        private float decayHFRatio;
		[SerializeField, Range(-10000, 1000)]
        private float reflectionsLevel;
		[SerializeField, Range(0, 0.3f)]
        private float reflectionsDelay;
		[SerializeField, Range(-10000, 2000)]
        private float reverbLevel;
		[SerializeField, Range(0, 0.1f)]
        private float reverbDelay;
		[SerializeField, Range(1000, 20000)]
        private float hfReference;
		[SerializeField, Range(20, 1000)]
        private float lfReference;
		[SerializeField, Range(0, 100)]
        private float diffusion;
		[SerializeField, Range(0, 100)]
        private float density;

		public AudioReverbPreset Preset => preset;
		public readonly float DryLevel => dryLevel;
		public readonly float Room => room;
		public readonly float RoomHF => roomHF;
		public readonly float RoomLF => roomLF;
		public readonly float DecayTime => decayTime;
		public readonly float DecayHFRatio => decayHFRatio;
		public readonly float ReflectionsLevel => reflectionsLevel;
		public readonly float ReflectionsDelay => reflectionsDelay;
		public readonly float ReverbLevel => reverbLevel;
		public readonly float ReverbDelay => reverbDelay;
		public readonly float HFReference => hfReference;
		public readonly float LFReference => lfReference;
		public readonly float Diffusion => diffusion;
		public readonly float Density => density;

		public AudioReverbValueSet(AudioReverbPreset preset, bool isSetToOff = false)
		{
			this.preset = preset;
			dryLevel = room = roomHF = roomLF = decayTime = decayHFRatio = reflectionsLevel = reflectionsDelay = reverbLevel = reverbDelay = hfReference = lfReference = diffusion = density = 0;
			this = AudioEventUtility.GetReverbPresetValues(preset);
			if (isSetToOff)
				this.preset = AudioReverbPreset.Off;
		}

		public AudioReverbValueSet(AudioReverbPreset preset, float dryLevel, float room, float roomHF, float roomLF, float decayTime, float decayHFRatio, float reflectionsLevel, float reflectionsDelay, float reverbLevel, float reverbDelay, float hfReference, float lfReference, float diffusion, float density)
		{
			this.preset = preset;
			this.dryLevel = dryLevel;
			this.room = room;
			this.roomHF = roomHF;
			this.roomLF = roomLF;
			this.decayTime = decayTime;
			this.decayHFRatio = decayHFRatio;
			this.reflectionsLevel = reflectionsLevel;
			this.reflectionsDelay = reflectionsDelay;
			this.reverbLevel = reverbLevel;
			this.reverbDelay = reverbDelay;
			this.hfReference = hfReference;
			this.lfReference = lfReference;
			this.diffusion = diffusion;
			this.density = density;
		}
	}
}

#if UNITY_EDITOR
namespace UnitySimplifiedEditor.Audio
{
	[CustomPropertyDrawer(typeof(AudioReverbValueSet), true)]
	public class AudioReverbValueSetDrawer : PropertyDrawer
	{
		private SerializedProperty _presetProp = null;
		private SerializedProperty[] _numberProps = new SerializedProperty[14];
		private readonly string[] _numberPropNames = new[]
		{
			"dryLevel",
			"room",
			"roomHF",
			"roomLF",
			"decayTime",
			"decayHFRatio",
			"reflectionsLevel",
			"reflectionsDelay",
			"reverbLevel",
			"reverbDelay",
			"hfReference",
			"lfReference",
			"diffusion",
			"density"
		};

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			int propCount = 1;
			float height = EditorGUIUtility.singleLineHeight * propCount;
			if (property.isExpanded)
			{
				// 1 = presetProp
				propCount += 1 + _numberProps.Length;
				// 1 = initial propCount
				height += EditorGUIUtility.singleLineHeight * (propCount - 1);
			}
			return height + propCount * 2;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{

			Rect previousRect = new Rect(position) { height = 0 };
			Rect foldoutRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height + 2, height = EditorGUIUtility.singleLineHeight };

			EditorGUI.PropertyField(foldoutRect, property, label, false);

			EditorGUI.BeginChangeCheck();
			if (property.isExpanded)
			{
				EditorGUI.indentLevel++;


				_presetProp = property.FindPropertyRelative("preset");
				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField(previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height + 2, height = EditorGUI.GetPropertyHeight(_presetProp) }, _presetProp);
				if (EditorGUI.EndChangeCheck())
				{
					_presetProp.serializedObject.ApplyModifiedProperties();
					if ((AudioReverbPreset)_presetProp.enumValueIndex != AudioReverbPreset.Off && (AudioReverbPreset)_presetProp.enumValueIndex != AudioReverbPreset.User)
					{
						Undo.RecordObject(_presetProp.serializedObject.targetObject, "Set Reverb Values");
						(FieldInfo, object) fieldInfoWrapper = property.ExposePropertyInfo(BindingFlags.Instance | BindingFlags.NonPublic, out _);
						fieldInfoWrapper.Item1.SetValue(fieldInfoWrapper.Item2, AudioEventUtility.GetReverbPresetValues((AudioReverbPreset)_presetProp.enumValueIndex));
					}
				}

				EditorGUI.BeginDisabledGroup((AudioReverbPreset)_presetProp.enumValueIndex != AudioReverbPreset.User);
				for (int i = 0; i < _numberProps.Length && i < _numberPropNames.Length; i++)
				{
					_numberProps[i] = property.FindPropertyRelative(_numberPropNames[i]);
					previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height + 2, height = EditorGUI.GetPropertyHeight(_numberProps[i]) };
					EditorGUI.PropertyField(previousRect, _numberProps[i]);
				}
				EditorGUI.EndDisabledGroup();


				EditorGUI.indentLevel--;
			}
			if (EditorGUI.EndChangeCheck())
				property.serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
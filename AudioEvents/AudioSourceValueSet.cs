using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
using UnitySimplified.Audio;
#endif

namespace UnitySimplified.Audio
{
	[Serializable]
	public struct AudioSourceValueSet : IAudioSourceTemplate
	{
		[SerializeField] private AudioClip clip;
		[SerializeField] private AudioMixerGroup mixerGroup;
		[SerializeField, CustomRange(0, 1)] private RangedFloat volume;
		[SerializeField, CustomRange(-3, 3)] private RangedFloat pitch;
		[SerializeField] private bool loop;

		public AudioSourceValueSet(bool loop, float volume, float pitch)
		{
			clip = null;
            mixerGroup = null;
			this.loop = loop;
			this.volume.min = this.volume.max = volume;
			this.pitch.min = this.pitch.max = pitch;
		}

		public AudioClip Clip => clip;
		public AudioMixerGroup MixerGroup => mixerGroup;
		public float Volume => Random.Range(volume.min, volume.max);
		public float Pitch => Random.Range(pitch.min, pitch.max);
		public bool Loop => loop;
	}
}

#if UNITY_EDITOR
namespace UnitySimplifiedEditor.Audio
{
	[CustomPropertyDrawer(typeof(AudioSourceValueSet), true)]
	public class AudioSourceValueSetDrawer : PropertyDrawer
	{
		private const float _extraSpacing = 6;
		private SerializedProperty _clipProp;
		private SerializedProperty _groupProp;
		private SerializedProperty _volumeProp;
		private SerializedProperty _pitchProp;
		private SerializedProperty _loopProp;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			int propCount = 1;
			float height = EditorGUIUtility.singleLineHeight * propCount;
			if (property.isExpanded)
			{
				propCount += 5;
				height += EditorGUIUtility.singleLineHeight * (propCount - 1) + _extraSpacing;
			}
			return height + propCount * 2;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			_clipProp = property.FindPropertyRelative("clip");
			_groupProp = property.FindPropertyRelative("group");
			_volumeProp = property.FindPropertyRelative("volume");
			_pitchProp = property.FindPropertyRelative("pitch");
			_loopProp = property.FindPropertyRelative("loop");

			Rect previousRect = new Rect(position) { height = 0 };
			Rect foldoutRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height + 2, height = EditorGUIUtility.singleLineHeight };

			EditorGUI.PropertyField(foldoutRect, property, label, false);

			EditorGUI.BeginChangeCheck();
			if (property.isExpanded)
			{
				Rect clipRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height + 2, height = EditorGUI.GetPropertyHeight(_clipProp) };
				Rect mixerGroupRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height + 2, height = EditorGUI.GetPropertyHeight(_groupProp) };
				previousRect.y += _extraSpacing;
				Rect volumeRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height + 2, height = EditorGUI.GetPropertyHeight(_volumeProp) };
				Rect pitchRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height + 2, height = EditorGUI.GetPropertyHeight(_pitchProp) };
				Rect loopRect = previousRect = new Rect(previousRect) { y = previousRect.y + previousRect.height + 2, height = EditorGUI.GetPropertyHeight(_loopProp) };
				EditorGUI.indentLevel++;
				EditorGUI.PropertyField(clipRect, _clipProp);
				EditorGUI.PropertyField(mixerGroupRect, _groupProp);

				EditorGUI.PropertyField(volumeRect, _volumeProp);
				EditorGUI.PropertyField(pitchRect, _pitchProp);
				EditorGUI.PropertyField(loopRect, _loopProp);
				EditorGUI.indentLevel--;
			}
			if (EditorGUI.EndChangeCheck())
				property.serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
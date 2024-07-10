using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnitySimplifiedEditor
{
    internal class EditorPingData
    {
        internal struct AnimationValues
        {
            public float zoomTime;
            public float waitTime;
            public float fadeOutTime;
            public float peakScale;
            public float availableWidth;

            public AnimationValues(float zoomTime, float waitTime, float fadeOutTime, float peakScale, float availableWidth)
            {
                this.zoomTime = zoomTime;
                this.waitTime = waitTime;
                this.fadeOutTime = fadeOutTime;
                this.peakScale = peakScale;
                this.availableWidth = availableWidth;
            }
        }

        internal Action<Rect> actionOnDraw;

        private MethodInfo _unclipInfo;
        private float _timeStart;


        internal Rect Position { get; private set; }
        internal GUIContent Label { get; private set; }
        internal bool IsPinging { get; private set; }
        internal AnimationValues Values { get; set; }
        internal AnimationValues Defaults => new(0.2f, 2.5f, 1.5f, 1.75f, 100f);
        internal EditorPingData() => Values = Defaults;
        private MethodInfo UnclipInfo => _unclipInfo ??= Type.GetType("UnityEngine.GUIClip, UnityEngine")?
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == "Unclip");



        internal void Start(Rect position, GUIContent label)
        {
            IsPinging = true;
            Position = position;
            Label = label;
            _timeStart = Time.realtimeSinceStartup;
        }
        internal void Update(Rect? newPosition)
        {
            if (!IsPinging)
                throw new InvalidOperationException();

            if (newPosition != null)
                Position = newPosition.Value;

            float totalDuration = Values.zoomTime + Values.waitTime + Values.fadeOutTime;
            float elapsed = (Time.realtimeSinceStartup - _timeStart);
            if (elapsed < totalDuration)
            {
                Color c = GUI.color;
                Matrix4x4 matrix = GUI.matrix;
                if (elapsed < Values.zoomTime)
                {
                    float peakTime = Values.zoomTime / 2f;
                    float scale = (Values.peakScale - 1f) * (((Values.zoomTime - Mathf.Abs(peakTime - elapsed)) / peakTime) - 1f) + 1f;

                    Vector2 pivotPoint = Position.xMax < Values.availableWidth ? Position.center : new Vector2(Values.availableWidth, Position.center.y);
                    Vector2 point = (Vector2)UnclipInfo.Invoke(null, new object[] { pivotPoint });
                    GUI.matrix *= Matrix4x4.TRS(point, Quaternion.identity, new Vector3(scale, scale, 1)) * Matrix4x4.TRS(-point, Quaternion.identity, Vector3.one);
                }
                else if (elapsed > Values.zoomTime + Values.waitTime)
                {
                    float alpha = (totalDuration - elapsed) / Values.fadeOutTime;
                    GUI.color = new Color(c.r, c.g, c.b, c.a * alpha);
                }

                if (Event.current.type == EventType.Repaint)
                {
                    Rect backRect = Position;
                    backRect.x -= EditorGUIExtended.PingStyle.padding.left;
                    backRect.y -= EditorGUIExtended.PingStyle.padding.top;
                    EditorGUIExtended.PingStyle.Draw(backRect, GUIContent.none, false, false, false, false);
                    actionOnDraw?.Invoke(Position);
                }

                GUI.matrix = matrix;
                GUI.color = c;
            }
            else EditorGUIExtended.PingEnd();
        }

        internal void End()
        {
            IsPinging = false;
            Position = default;
            Label = null;
            _timeStart = 0;
        }
    }
}

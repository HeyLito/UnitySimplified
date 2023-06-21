#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace UnitySimplifiedEditor
{
    public class PingLabel
    {
        private class PingData
        {
            public Rect contentRect;
            public GUIStyle style;
            public Action<Rect> onContentDraw;

            private MethodInfo _unclipInfo = null;
            private bool _isPinging = false;
            private float _timeStart = -1f;
            private readonly float _zoomTime = 0.2f;
            private readonly float _waitTime = 2.5f;
            private readonly float _fadeOutTime = 1.5f;
            private readonly float _peakScale = 1.75f;
            private readonly float _availableWidth = 100f;

            public bool IsPinging
            {
                get { return _isPinging; }
                set { _isPinging = value; _timeStart = -1f; }
            }


            public MethodInfo UnclipInfo => _unclipInfo ??= Type.GetType("UnityEngine.GUIClip, UnityEngine").GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                                                                                                            .Where(x => x.Name == "Unclip")
                                                                                                            .FirstOrDefault();

            public void HandlePing()
            {
                if (!IsPinging)
                    return;

                if (_timeStart < 0)
                    _timeStart = Time.realtimeSinceStartup;
                float totalTime = _zoomTime + _waitTime + _fadeOutTime;
                float t = (Time.realtimeSinceStartup - _timeStart);

                if (t >= 0.0f && t < totalTime)
                {
                    Color c = GUI.color;
                    Matrix4x4 m = GUI.matrix;
                    if (t < _zoomTime)
                    {
                        float peakTime = _zoomTime / 2f;
                        float scale = (_peakScale - 1f) * (((_zoomTime - Mathf.Abs(peakTime - t)) / peakTime) - 1f) + 1f;
                        Matrix4x4 mat = GUI.matrix;

                        Vector2 pivotPoint = contentRect.xMax < _availableWidth ? contentRect.center : new Vector2(_availableWidth, contentRect.center.y);
                        Vector2 point = (Vector2)UnclipInfo.Invoke(null, new object[] { pivotPoint });
                        Matrix4x4 newMat = Matrix4x4.TRS(point, Quaternion.identity, new Vector3(scale, scale, 1)) * Matrix4x4.TRS(-point, Quaternion.identity, Vector3.one);
                        GUI.matrix = newMat * mat;
                    }
                    else if (t > _zoomTime + _waitTime)
                    {
                        float alpha = (totalTime - t) / _fadeOutTime;
                        GUI.color = new Color(c.r, c.g, c.b, c.a * alpha);
                    }

                    if (onContentDraw != null && Event.current.type == EventType.Repaint)
                    {
                        Rect backRect = contentRect;
                        backRect.x -= style.padding.left;
                        backRect.y -= style.padding.top;
                        style.Draw(backRect, GUIContent.none, false, false, false, false);
                        onContentDraw(contentRect);
                    }

                    GUI.matrix = m;
                    GUI.color = c;
                }
                else EndPing();
            }
        }

        private static readonly PingData _ping = new PingData();
        private static GUIStyle _pingStyle = null;
        private static EditorWindow _window = null;
        private static GUIContent _labelContent = new GUIContent();
        private static Rect _labelRect = new Rect();

        public static event Action OnPingBegan;

        public static Rect LabelRect => _labelRect;
        public static bool IsPinging => _ping.IsPinging;
        private static GUIStyle PingStyle => _pingStyle == null ? _pingStyle = new GUIStyle("OL Ping") : _pingStyle;

        public static void SetCurrentLabel(GUIContent labelContent, Rect labelRect)
        {
            if (_labelContent.text == labelContent.text)
            {
                _labelRect = labelRect;
                if (!IsPinging)
                    BeginPing();
            }
            if (IsPinging)
                HandlePing();
        }
        public static void Ping(EditorWindow window, GUIContent content, bool frame)
        {
            EndPing();

            if (window)
            {
                _window = window;
                if (frame)
                {
                    window.Show();
                    window.Focus();
                }
            }
            else return;
            _labelContent.text = content.text;
        }

        private static void BeginPing()
        {
            _ping.IsPinging = true;
            _ping.style = PingStyle;
            Vector2 pingLabelSize = _ping.style.CalcSize(_labelContent);
            _ping.contentRect.width = pingLabelSize.x;
            _ping.contentRect.height = pingLabelSize.y;
            _ping.onContentDraw = (Rect r) =>
            {
                GUIStyle label = new GUIStyle(EditorStyles.label);
                label.alignment = TextAnchor.MiddleLeft;
                label.Draw(r, _labelContent.text, false, false, false, false);
            };
            Vector2 pos = CalculatePingPosition();
            _ping.contentRect.x = pos.x;
            _ping.contentRect.y = pos.y;

            if (_window)
                _window.Repaint();

            OnPingBegan?.Invoke();
        }
        private static void EndPing()
        {   _ping.IsPinging = false; _labelContent = new GUIContent(); _labelRect = new Rect();   }

        private static void HandlePing()
        {
            if (IsPinging)
            {
                Vector2 pos = CalculatePingPosition();
                _ping.contentRect.x = pos.x;
                _ping.contentRect.y = pos.y;
            }
            _ping.HandlePing();

            if (IsPinging && _window)
                _window.Repaint();
        }

        private static Vector2 CalculatePingPosition()
        {   return new Vector2(LabelRect.x, LabelRect.y);   }
    }
}

#endif
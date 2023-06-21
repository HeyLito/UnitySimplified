#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnitySimplifiedEditor
{
    /// <summary>
    /// By default, the inspector creates an instance of <see cref="CustomPropertyDrawer"/> that is statically used across all SerializedProperties of its respective <see cref="CustomPropertyDrawer(Type)"/>.
    /// This class uses <see cref="SerializedProperty.propertyPath"/> to have unique SerializedProperty information persist throughout <see cref="PropertyDrawer"/> method calls.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    public class PropertyDrawerElementFilter<TElement>
    {
        private readonly Dictionary<string, (int, TElement)> _processedElementsByPaths = new Dictionary<string, (int, TElement)>();
        private bool _isInvoking = false;
        private bool _changed = false;
        private bool _dragged = false;
        private bool _up = false;

        public void EvaluateGUIEventChanges(Event evt)
        {
            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 0)
                        _dragged = _up = false;
                    break;

                case EventType.MouseDrag:
                    if (evt.button == 0)
                        _dragged = true;
                    break;

                case EventType.MouseUp:
                    if (evt.button == 0)
                        if (_dragged)
                            _up = true;
                        else _dragged = _up = false;
                    break;

                case EventType.ContextClick:
                    _changed = true;
                    break;


                case EventType.Repaint:
                    if (_changed)
                    {
                        _processedElementsByPaths.Clear();
                        _isInvoking = true;
                        _changed = false;
                    }
                    if (_up)
                    {
                        _processedElementsByPaths.Clear();
                        _dragged = _up = false;
                    }
                    break;
            }
        }
        public void InvokeActionIfChanged(Action action)
        {
            if (_isInvoking)
                action?.Invoke();
        }
        public void RemoveElement(SerializedProperty property)
        {   _processedElementsByPaths.Remove(PropertyPathAsIndex(property));   }
        public void SetElement(SerializedProperty property, TElement element)
        {   DoSetElement(property, element);   }
        public void SetElement(SerializedProperty property, Func<TElement> element)
        {   DoSetElement(property, element());   }
        public TElement GetFilteredElement(SerializedProperty property, TElement defaultElement)
        {   return DoGetFilteredElement(property, defaultElement, null);   }
        public TElement GetFilteredElement(SerializedProperty property, Func<TElement> defaultElement)
        {   return DoGetFilteredElement(property, default(TElement), defaultElement);   }
        public TElement GetElementWithoutFiltering(SerializedProperty property)
        {   return DoGetElementWithoutFiltering(property);   }

        private TElement DoGetElementWithoutFiltering(SerializedProperty property)
        {
            if (_processedElementsByPaths.TryGetValue(PropertyPathAsIndex(property), out var tuple))
                return tuple.Item2;
            else return default(TElement);
        }
        private TElement DoGetFilteredElement(SerializedProperty property, TElement defaultElement, Func<TElement> defaultElementFromFunc)
        {
            bool removeUnused = false;
            TElement value = default(TElement);

            if (_processedElementsByPaths.TryGetValue(PropertyPathAsIndex(property), out var tuple))
            {
                if (tuple.Item1 > 1)
                    removeUnused = true;
                _processedElementsByPaths[PropertyPathAsIndex(property)] = (tuple.Item1 + 1, tuple.Item2);
                value = tuple.Item2;
            }
            else
            {
                if (!defaultElement.Equals(default(TElement)))
                {
                    value = defaultElement;
                    SetElement(property, value);
                }
                else if (defaultElementFromFunc != null)
                {
                    value = defaultElementFromFunc();
                    SetElement(property, value);
                }
            }

            if (removeUnused)
            {
                _isInvoking = false;
                List<string> unused = new List<string>();
                List<string> used = new List<string>();
                foreach (var pair in _processedElementsByPaths)
                {
                    if (pair.Value.Item1 == 0)
                        unused.Add(pair.Key);
                    else used.Add(pair.Key);
                }

                for (int i = 0; i < unused.Count; i++)
                    _processedElementsByPaths.Remove(unused[i]);
                for (int i = 0; i < used.Count; i++)
                    _processedElementsByPaths[used[i]] = (0, _processedElementsByPaths[used[i]].Item2);
            }
            return value;
        }
        private string PropertyPathAsIndex(SerializedProperty property)
        {   return $"{property.serializedObject.targetObject.name}.{property.propertyPath}";   }
        private void DoSetElement(SerializedProperty property, TElement element)
        {   _processedElementsByPaths[PropertyPathAsIndex(property)] = (0, element);   }
    }
}

#endif
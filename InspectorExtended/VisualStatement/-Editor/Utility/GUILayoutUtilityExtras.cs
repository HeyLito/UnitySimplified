#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplifiedEditor
{
    public static class GUILayoutUtilityExtras
    {
        #region FIELDS
        private static bool _refreshIndexer = false;
        private static EventType[] _eventTypes = new EventType[0];
        private static Dictionary<EventType, int> _indexes = new Dictionary<EventType, int>();
        #endregion

        #region METHODS
        public static int GetEventIndexer()
        {
            Event evt = Event.current;

            if (evt == null)
                return -1;

            if (_indexes.Count == 0)
            {
                List<EventType> items = new List<EventType>();
                foreach (var item in Enum.GetValues(typeof(EventType)))
                    if (!_indexes.ContainsKey((EventType)item))
                    {
                        items.Add((EventType)item);
                        _indexes.Add((EventType)item, -1);
                    }
                _eventTypes = items.ToArray();
            }

            if (evt.type == EventType.Layout && _refreshIndexer)
            {
                for (int i = 0; i < _eventTypes.Length; i++)
                    _indexes[_eventTypes[i]] = -1;
                _refreshIndexer = !_refreshIndexer;
            }
            else if (evt.type != EventType.Layout && !_refreshIndexer)
                _refreshIndexer = !_refreshIndexer;

            _indexes[evt.type]++;
            return _indexes[evt.type];
        }
        #endregion
    }
}

#endif
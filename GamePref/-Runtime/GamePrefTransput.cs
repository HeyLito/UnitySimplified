using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnitySimplifiedEditor;
#endif

namespace UnitySimplified.Serialization
{
    public class GamePrefTransput : MonoBehaviour
    {
        public enum TypeOfTransput
        {
            Bool,
            Int,
            Float,
            String,
            Object,
        }
        public enum LoadOn
        {
            None,
            Awake,
            AwakeWithoutNotify,
            Start,
            StartWithoutNotify,
            Update,
            UpdateWithoutNotify,
            OnEnable,
            OnEnableWithoutNotify,
            OnDisable,
            OnDisableWithoutNotify,
        }

        [Serializable]
        public class InfoContainer
        {
            public GamePref gamePref = null;
            public string gamePrefKey = "";
            public TypeOfTransput transputType = default;
            public LoadOn loadOn;
            public UnityEvent<bool> onBoolValueChanged = new UnityEvent<bool>();
            public UnityEvent<int> onIntValueChanged = new UnityEvent<int>();
            public UnityEvent<float> onFloatValueChanged = new UnityEvent<float>();
            public UnityEvent<string> onStringValueChanged = new UnityEvent<string>();
            public UnityEvent<object> onObjectValueChanged = new UnityEvent<object>();

            private bool _initialized = false;
            private bool _gamePrefIsValid = false;

            private static bool _loaded = false;

            public void LoadValue()
            {
                if (!_loaded)
                {
                    GamePref.Load();
                    _loaded = !_loaded;
                }

                if (!_initialized)
                {
                    if (GamePref.HasGamePref(gamePref, out GamePrefData data) || GamePrefStorage.Instance.HasGamePref(gamePref, out data))
                    {
                        transputType = TypeToTransputType(data.GetPrefType());
                        _gamePrefIsValid = true;
                    }
                    _initialized = !_initialized;
                }

                object value;
                if (_gamePrefIsValid)
                    value = gamePref.GetValue();
                else value = GamePref.GetValue<object>(gamePrefKey);
                if ((int)loadOn % 2 == 1)
                    if (value != null && !value.Equals(null))
                        InvokeEvent(value);
            }
            public void SetValue(object value, bool notify)
            {
                if (!_loaded)
                {
                    GamePref.Load();
                    _loaded = !_loaded;
                }

                if (!_initialized)
                {
                    if (GamePref.HasGamePref(gamePref, out GamePrefData data) || GamePrefStorage.Instance.HasGamePref(gamePref, out data))
                    {
                        transputType = TypeToTransputType(data.GetPrefType());
                        _gamePrefIsValid = true;
                    }
                    _initialized = !_initialized;
                }

                if (_gamePrefIsValid)
                    gamePref.SetValue(value);
                else GamePref.SetValue(gamePrefKey, value);
                if (notify)
                    InvokeEvent(value);
            }

            public bool AcceptsValue(object value)
            {
                switch (transputType)
                {
                    case TypeOfTransput.Bool:
                        return value is bool;

                    case TypeOfTransput.Int:
                        return value is int;

                    case TypeOfTransput.Float:
                        return value is float;

                    case TypeOfTransput.String:
                        return value is string;

                    case TypeOfTransput.Object:
                        return true;
                }
                return false;
            }

            private void InvokeEvent(object value)
            {
                switch (transputType)
                {
                    case TypeOfTransput.Bool:
                        onBoolValueChanged?.Invoke((bool)value);
                        break;

                    case TypeOfTransput.Int:
                        onIntValueChanged?.Invoke(value is long ? Convert.ToInt32(value) : (int)value);
                        break;

                    case TypeOfTransput.Float:
                        onFloatValueChanged?.Invoke(value is double ? Convert.ToSingle(value) : (float)value);
                        break;

                    case TypeOfTransput.String:
                        onStringValueChanged?.Invoke((string)value);
                        break;

                    case TypeOfTransput.Object:
                        onObjectValueChanged?.Invoke(value);
                        break;
                }
            }
            public static TypeOfTransput TypeToTransputType(Type type)
            {
                if (type == typeof(bool))
                    return TypeOfTransput.Bool;
                if (type == typeof(int))
                    return TypeOfTransput.Int;
                if (type == typeof(float))
                    return TypeOfTransput.Float;
                if (type == typeof(string))
                    return TypeOfTransput.String;
                return TypeOfTransput.Object;
            }
        }

        [SerializeField] private int targetTransputIndex = -1;
        [SerializeField] private List<InfoContainer> transputs = new List<InfoContainer>();

        private void OnEnable()
        {
            foreach (var transput in transputs)
                if (transput.loadOn == LoadOn.OnEnable || transput.loadOn == LoadOn.OnEnableWithoutNotify)
                    transput.LoadValue();
        }
        private void OnDisable()
        {
            foreach (var transput in transputs)
                if (transput.loadOn == LoadOn.OnDisable || transput.loadOn == LoadOn.OnDisableWithoutNotify)
                    transput.LoadValue();
        }
        private void Awake()
        {
            foreach (var transput in transputs)
                if (transput.loadOn == LoadOn.Awake || transput.loadOn == LoadOn.AwakeWithoutNotify)
                    transput.LoadValue();
        }
        private void Start()
        {
            foreach (var transput in transputs)
                if (transput.loadOn == LoadOn.Start || transput.loadOn == LoadOn.StartWithoutNotify)
                    transput.LoadValue();
        }
        private void Update()
        {
            foreach (var transput in transputs)
                if (transput.loadOn == LoadOn.Update || transput.loadOn == LoadOn.UpdateWithoutNotify)
                    transput.LoadValue();
        }
        public void InvokeEvents()
        {
            List<InfoContainer> validTransputs = null;
            for (int i = 0; (targetTransputIndex == -1 || targetTransputIndex >= i) && i < transputs.Count; i++)
            {
                if ((targetTransputIndex == -1 || targetTransputIndex == i))
                {
                    if (validTransputs == null)
                        validTransputs = new List<InfoContainer>();
                    validTransputs.Add(transputs[i]);

                    if (i != -1)
                        break;
                }

            }

            for (int i = 0; i < validTransputs.Count; i++)
                validTransputs[i].LoadValue();
        }
        public void SetTargetIndex(int index)
        {   targetTransputIndex = index;   }
        public void SetValue(bool value)
        {   DoSetValue(value, true);   }
        public void SetValue(int value)
        {   DoSetValue(value, true);   }
        public void SetValue(float value)
        {   DoSetValue(value, true);   }
        public void SetValue(string value)
        {   DoSetValue(value, true);   }
        public void SetValue(object value)
        {   DoSetValue(value, true);   }
        public void SetValueWithoutNotify(bool value)
        {   DoSetValue(value, false);   }
        public void SetValueWithoutNotify(int value)
        {   DoSetValue(value, false);   }
        public void SetValueWithoutNotify(float value)
        {   DoSetValue(value, false);   }
        public void SetValueWithoutNotify(string value)
        {   DoSetValue(value, false);   }
        public void SetValueWithoutNotify(object value)
        {   DoSetValue(value, false);   }

        private void DoSetValue(object value, bool notify)
        {
            #if UNITY_EDITOR
            if (UnityObjectUtility.IsObjectInPrefabEdit(this))
                return;

            #endif
            foreach (var transput in GetTransputs(value))
                transput.SetValue(value, notify);
        }
        private List<InfoContainer> GetTransputs(object value)
        {
            List<InfoContainer> validTransputs = null;
            for (int i = 0; (targetTransputIndex == -1 || targetTransputIndex >= i) && i < transputs.Count; i++)
            {
                if ((targetTransputIndex == -1 || targetTransputIndex == i) && transputs[i].AcceptsValue(value))
                {
                    if (validTransputs == null)
                        validTransputs = new List<InfoContainer>();
                    validTransputs.Add(transputs[i]);

                    if (i != -1)
                        break;
                }

            }
            return validTransputs;
        }
    }
}
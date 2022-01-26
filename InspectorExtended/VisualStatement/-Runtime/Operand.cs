using System;
using System.Reflection;
using UnityEngine;
using UnitySimplified.Serialization;
using UnityObject = UnityEngine.Object;

namespace UnitySimplified 
{
    public sealed partial class VisualStatement
    {
        [Serializable]
        public sealed class Operand
        {
            [Serializable]
            private class DataTransfer
            {
                public object value;
                public DataTransfer()
                { }
                public DataTransfer(object value)
                { this.value = value; }
            }
            public enum ReferenceType
            {
                Value,
                Field
            }

            #region FIELDS
            [NonSerialized] private object _memberObject;
            [NonSerialized] private MemberInfo _memberInfo;
            [NonSerialized] private Type _valueType;
            [NonSerialized] private object _value;

            [SerializeField] private ReferenceType referenceType;
            [SerializeField] private string valueType;
            [SerializeField] private string valuePath;
            [SerializeField] private string valueData;
            [SerializeField] private UnityObject valueObject;
            [SerializeField] private UnityObject fieldObject;
            [SerializeField] private UnityObject fieldSubObject;
            #endregion

            #region PROPERTIES
            public Type ValueType
            {
                get
                {
                    if (_valueType != null)
                    {
                        if (string.IsNullOrEmpty(valueType))
                            return _valueType = null;
                        if (!string.IsNullOrEmpty(valueType) && _valueType.AssemblyQualifiedName != valueType)
                            return _valueType = Type.GetType(valueType);
                        else return _valueType;
                    }
                    else if (!string.IsNullOrEmpty(valueType))
                        return _valueType = Type.GetType(valueType);
                    return null;
                }
            }
            #endregion

            #region CONTRUCTORS
            public Operand(ReferenceType referenceType)
            {   this.referenceType = referenceType;   }
            #endregion

            #region METHODS
            internal bool DoIsValid(out int code, out string message)
            {
                code = -1;
                message = "";

                var valueType = ValueType;
                if (valueType == null)
                {
                    code = 0;
                    message = $"The {nameof(valueType)} of {this} returned as NULL";
                    return false;
                }
                if (referenceType == ReferenceType.Field)
                {
                    if (string.IsNullOrEmpty(valuePath))
                    {
                        code = 1;
                        message = $"Operand as {ReferenceType.Field} is invalid as the value path is NULL or empty";
                        return false;
                    }
                    if (fieldObject == null || fieldObject.Equals(null))
                    {
                        code = 2;
                        message = $"Operand as {ReferenceType.Field} is invalid as the field target object is missing";
                        return false;
                    }
                    else if (VisualStatementUtility.GetReturnTypeFromObjectPath(fieldObject, valuePath) == null)
                    {
                        code = 3;
                        message = $"Operand as {ReferenceType.Field} is invalid as the value given from value path is NULL";
                        return false;
                    }
                }
                return true;
            }
            internal object DoGetResult()
            {
                switch (referenceType)
                {
                    case ReferenceType.Field:
                        return GetFieldObject();
                    case ReferenceType.Value:
                        return GetValueObject();
                    default:
                        return null;
                }
            }

            public void Intialize(object value, Type valueType)
            {
                _memberObject = null;
                _memberInfo = null;
                _value = null;
                _valueType = null;
                valueObject = null;
                valueData = "";
                this.valueType = "";

                switch (referenceType)
                {
                    case ReferenceType.Field:
                        if (!string.IsNullOrEmpty(valuePath) && value == null && valueType != null)
                        {
                            _value = value;
                            _valueType = valueType;
                            this.valueType = valueType.AssemblyQualifiedName;
                        }
                        break;

                    case ReferenceType.Value:
                        var newValue = valueType != null && value == null ? valueType.IsValueType && DataManagerUtility.IsSerializable(valueType) ? Activator.CreateInstance(valueType) : valueType == typeof(string) ? "" : value : value;
                        if (newValue != null && valueType != null && (newValue.GetType() == valueType || newValue.GetType().IsSubclassOf(valueType)))
                        {
                            _value = newValue;
                            _valueType = valueType;
                            this.valueType = valueType.AssemblyQualifiedName;

                            if (DataManagerUtility.IsSerializable(valueType))
                                valueData = DataManager.SaveFileAsString(new DataTransfer(newValue), FileFormat.Binary);
                        }

                        if (valueType != null && valueType.IsSubclassOf(typeof(UnityObject)))
                        {
                            if (value != null && !value.GetType().IsSubclassOf(typeof(UnityObject)))
                                return;
                            _value = newValue;
                            _valueType = valueType;
                            valueObject = value as UnityObject;
                            this.valueType = valueType.AssemblyQualifiedName;
                        }
                        break;
                }
            }
            public bool IsValid()
            {   return DoIsValid(out _, out _);   }
            public object GetFieldObject()
            {
                if (referenceType != ReferenceType.Field)
                    return null;

                if (fieldObject == null || string.IsNullOrEmpty(valuePath))
                    return _value = null;

                if (_memberInfo != null && _memberObject != null)
                {
                    var memberObject = _memberObject;
                    var memberInfo = _memberInfo;
                    var tuple = VisualStatementUtility.GetResultFromMember(_memberObject, _memberInfo);
                    _value = tuple.Item1;
                    _valueType = tuple.Item2;
                    _memberObject = memberObject;
                    _memberInfo = memberInfo;
                }
                else
                {
                    var tuple = VisualStatementUtility.GetValueFromObjectPath(fieldObject, valuePath);
                    _value = tuple.Item2;
                    _valueType = tuple.Item3;
                    _memberObject = tuple.Item1.Item1;
                    _memberInfo = tuple.Item1.Item2;
                }
                return _value;
            }
            public object GetValueObject()
            {
                if (referenceType != ReferenceType.Value)
                    return null;

                var type = ValueType;
                if (type == null)
                    return _value = null;

                if (_value == null)
                {
                    if (DataManagerUtility.IsSerializable(type))
                    {
                        if (!string.IsNullOrEmpty(valueData))
                        {
                            var dataTransfer = new DataTransfer();
                            DataManager.LoadFileFromString(dataTransfer, valueData, FileFormat.Binary);
                            return _value = dataTransfer.value;
                        }
                    }
                    else if (type.IsSubclassOf(typeof(UnityObject)))
                        return _value = valueObject;
                }
                else if (_value.GetType() == type)
                    return _value;

                return _value = null;
            }

            public object GetResult()
            {
                if (!DoIsValid(out int code, out string message))
                    throw new Exception($"Error {code}: {message}");

                return DoGetResult();
            }

            public override string ToString()
            {   return base.ToString() + $"({ValueType})";   }
            #endregion
        }
    }
}
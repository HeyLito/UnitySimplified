using System;
using UnityEngine;

[Serializable]
public struct Trigger : IConvertible, IComparable, IComparable<Trigger>, IEquatable<Trigger>, IComparable<bool>, IEquatable<bool>
{
    [SerializeField]
    private bool _value;

    public Trigger(bool value) => _value = value;

    public readonly int CompareTo(object obj)
    {
        return obj switch
        {
            Trigger triggerValue => _value.CompareTo(triggerValue._value),
            bool boolValue => _value.CompareTo(boolValue),
            _ => _value.CompareTo(obj)
        };
    }
    public readonly bool Equals(Trigger other) => _value.Equals(other._value);
    public readonly bool Equals(bool other) => _value.Equals(other);
    public readonly int CompareTo(bool other) => (this as IComparable).CompareTo(other);
    public readonly int CompareTo(Trigger other) => (this as IComparable).CompareTo(other);
    readonly byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)_value).ToByte(provider);
    readonly char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)_value).ToChar(provider);
    readonly bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)_value).ToBoolean(provider);
    readonly decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)_value).ToDecimal(provider);
    readonly double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)_value).ToDouble(provider);
    readonly short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)_value).ToInt16(provider);
    readonly int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)_value).ToInt32(provider);
    readonly long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)_value).ToInt64(provider);
    readonly sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)_value).ToSByte(provider);
    readonly float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)_value).ToSingle(provider);
    readonly ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)_value).ToUInt16(provider);
    readonly uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)_value).ToUInt32(provider);
    readonly ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)_value).ToUInt64(provider);
    readonly DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)_value).ToDateTime(provider);
    readonly TypeCode IConvertible.GetTypeCode() => _value.GetTypeCode();
    readonly object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)_value).ToType(conversionType, provider);
    public readonly string ToString(IFormatProvider provider) => _value.ToString(provider);

    public static implicit operator Trigger(bool value) => new(value);

    public bool GetValue()
    {
        if (_value)
        {
            _value = false;
            return true;
        }
        else return false;
    }
    public bool SetValue() => _value = true;
    public override string ToString() => $"Trigger({_value})";
}
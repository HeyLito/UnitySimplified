using System;
using UnityEngine;

[Serializable]
public struct Trigger : IConvertible, IComparable, IComparable<Trigger>, IEquatable<Trigger>, IComparable<bool>, IEquatable<bool>
{
    [SerializeField]
    private bool value;

    public Trigger(bool value) => this.value = value;

    public readonly int CompareTo(object obj)
    {
        if (obj is Trigger triggerValue)
            return value.CompareTo(triggerValue.value);
        else if (obj is bool boolValue)
            return value.CompareTo(boolValue);
        return value.CompareTo(obj);
    }
    public readonly bool Equals(Trigger other) => value.Equals(other.value);
    public readonly bool Equals(bool other) => value.Equals(other);
    public readonly int CompareTo(bool other) => (this as IComparable).CompareTo(other);
    public readonly int CompareTo(Trigger other) => (this as IComparable).CompareTo(other);
    readonly byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)value).ToByte(provider);
    readonly char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)value).ToChar(provider);
    readonly bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)value).ToBoolean(provider);
    readonly decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)value).ToDecimal(provider);
    readonly double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)value).ToDouble(provider);
    readonly short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)value).ToInt16(provider);
    readonly int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)value).ToInt32(provider);
    readonly long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)value).ToInt64(provider);
    readonly sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)value).ToSByte(provider);
    readonly float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)value).ToSingle(provider);
    readonly ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)value).ToUInt16(provider);
    readonly uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)value).ToUInt32(provider);
    readonly ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)value).ToUInt64(provider);
    readonly DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)value).ToDateTime(provider);
    readonly TypeCode IConvertible.GetTypeCode() => value.GetTypeCode();
    readonly object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)value).ToType(conversionType, provider);
    public readonly string ToString(IFormatProvider provider) => value.ToString(provider);

    public static implicit operator Trigger(bool value) => new(value);

    public bool GetValue()
    {
        if (value)
        {
            value = false;
            return true;
        }
        else return false;
    }
    public bool SetValue() => value = true;
    public override string ToString() => $"Trigger({value})";
}
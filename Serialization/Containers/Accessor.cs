using System;

namespace UnitySimplified.Serialization.Containers
{
#if ENABLE_UNITYSIMPLIFIED_NEWTONSOFT
    [Newtonsoft.Json.JsonConverter(typeof(AccessorJsonConverter))]
#endif
    [Serializable]
    public abstract class Accessor
    {
        public abstract bool CanAccess(Type valueType);
        public abstract void Set(object value);
        public abstract void Get(out object value);
    }

    [Serializable]
    public abstract class Accessor<T> : Accessor
    {
        public abstract void Set(T value);
        public abstract void Get(out T value);

        public sealed override bool CanAccess(Type valueType) => valueType == typeof(T);
        public sealed override void Set(object value)
        {
            Set((T)value);
        }
        public sealed override void Get(out object value)
        {
            Get(out T genericValue);
            value = genericValue;
        }
    }
}
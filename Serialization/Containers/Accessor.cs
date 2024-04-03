using System;

namespace UnitySimplified.Serialization.Containers
{
#if ENABLE_UNITYSIMPLIFIED_NEWTONSOFT
    [Newtonsoft.Json.JsonConverter(typeof(AccessorJsonConverter))]
#endif
    [Serializable]
    public abstract class Accessor
    {
        public virtual void Set(object value) { }
        public virtual void Get(out object value) => value = null;
    }

    [Serializable]
    public abstract class Accessor<T> : Accessor
    {
        public abstract void Set(T value);
        public abstract void Get(out T value);

        public override void Set(object value)
        {
            Set((T)value);
        }
        public override void Get(out object value)
        {
            Get(out T genericValue);
            value = genericValue;
        }
    }
}
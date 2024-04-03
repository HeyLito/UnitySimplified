using System;
using System.Collections;

namespace UnitySimplified.Serialization.Containers
{
    /// <summary>
    /// A data structure for containing an object field data.
    /// </summary>
    [Serializable]
    public class ObjectData
    {
        /// <summary>
        /// The name and assembly given from <see cref="Type"/>.
        /// </summary>
        public readonly string targetType = "";

        /// <summary>
        /// If used, a string identifier is utilized to reinstate references to runtime script variables.
        /// </summary>
        public readonly ReferenceData reference;

        /// <summary>
        /// The field data of an object compressed into <see cref="IDictionary"/> structure. Each entry consists of the field's name as a key and the field's object value.
        /// </summary>
        public readonly AccessorDictionary accessors = new();



        internal ObjectData() { }
        internal ObjectData(string targetType, ReferenceData reference, AccessorDictionary accessors)
        {
            this.targetType = targetType;
            this.reference = reference;
            this.accessors = accessors;
        }

        /// <summary>
        /// Creates a new data structure instance.
        /// </summary>
        /// 
        /// <param name="type">
        /// The <see cref="Type"/> that this data container is representing.
        /// </param>
        public ObjectData(Type type) => targetType = $"{type.FullName}, {type.Assembly.GetName().Name}";

        /// <summary>
        /// Creates a new data structure instance.
        /// </summary>
        /// 
        /// <param name="type">
        /// The <see cref="Type"/> that this data container is representing.
        /// </param>
        /// 
        /// <param name="reference">
        /// Sets <see cref="reference"/>.
        /// </param>
        public ObjectData(Type type, ReferenceData reference) : this(type) => this.reference = reference;

        /// <summary>
        /// Creates a new data structure instance.
        /// </summary>
        /// 
        /// <param name="type">
        /// The <see cref="Type"/> that this data container is representing.
        /// </param>
        /// 
        /// <param name="reference">
        /// Used to create a new <see cref="ReferenceData(object)"/>.
        /// </param>
        public ObjectData(Type type, object reference) : this(type, new ReferenceData(reference)) { }



        /// <summary>
        /// Retrieves the type given from <see cref="typeName"/>.
        /// </summary>
        /// <returns></returns>
        public Type GetDataType() => Type.GetType(targetType);
        public override string ToString() => $"{GetType().Name}({GetType().FullName})";
    }
}
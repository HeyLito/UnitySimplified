using System;
using System.Collections;

namespace UnitySimplified.Serialization
{
    /// <summary>
    /// A data structure for containing an object field data.
    /// </summary>
    [Serializable]
    public class ObjectData
    {
        /// <summary>
        /// The full name given from <see cref="Type.GetType()"/>.
        /// </summary>
        public readonly string name = "";

        /// <summary>
        /// The assembly qualified name given from <see cref="Type.GetType()"/>.
        /// </summary>
        public readonly string assemblyQualifiedName = "";

        /// <summary>
        /// The field data of an object compressed into <see cref="IDictionary"/> structure. Each entry consists of the field's name as a key and the field's object value.
        /// </summary>
        public readonly FieldData fieldData = new FieldData();

        /// <summary>
        /// Creates a new data structure instance.
        /// </summary>
        /// <param name="name">
        /// The full name given from <see cref="Type.GetType()"/>.
        /// </param>
        /// <param name="assemblyQualifiedName">
        /// The assembly qualified name given from <see cref="Type.GetType()"/>.
        /// </param>
        public ObjectData(string name, string assemblyQualifiedName)
        {
            this.name = name;
            this.assemblyQualifiedName = assemblyQualifiedName;
        }

        /// <summary>
        /// Retrieves the type given from <see cref="assemblyQualifiedName"/>.
        /// </summary>
        /// <returns></returns>
        public Type GetObjectType()
        { return Type.GetType(assemblyQualifiedName); }

        public override string ToString()
        { return $"{GetType().Name}({name})"; }
    }
}
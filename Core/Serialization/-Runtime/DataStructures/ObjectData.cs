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
        /// The name given from <see cref="Type.GetType()"/>.FullName.
        /// </summary>
        public readonly string name = "";

        /// <summary>
        /// The name given from <see cref="Type.GetType()"/>.AssemblyQualifiedName.
        /// </summary>
        public readonly string assemblyQualifiedName = "";

        /// <summary>
        /// The field data of an object compressed into <see cref="IDictionary"/> structure. Each entry consists of the field's name as a key and the field's object value.
        /// </summary>
        public readonly FieldData fieldData = new FieldData();
        
        /// <summary>
        /// If enabled, a persistent identifier is created from a new <see cref="ObjectData"/> instance.
        /// </summary>
        public readonly SerializableReference reference = null;

        /// <summary>
        /// Creates a new data structure instance.
        /// </summary>
        /// 
        /// <param name="type">
        /// The <see cref="Type"/> that this data container is frameworked after.
        /// </param>
        public ObjectData(Type type)
        {
            name = type.FullName;
            assemblyQualifiedName = type.AssemblyQualifiedName;
        }

        /// <summary>
        /// Creates a new data structure instance.
        /// </summary>
        /// 
        /// <param name="type">
        /// The <see cref="Type"/> that this data container is frameworked after.
        /// </param>
        /// 
        /// <param name="reference">
        /// Sets <see cref="reference"/>.
        /// </param>
        public ObjectData(Type type, SerializableReference reference)
        {
            name = type.FullName;
            assemblyQualifiedName = type.AssemblyQualifiedName;
            this.reference = reference;
        }

        /// <summary>
        /// Creates a new data structure instance.
        /// </summary>
        /// 
        /// <param name="type">
        /// The <see cref="Type"/> that this data container is frameworked after.
        /// </param>
        /// 
        /// <param name="referenceObject">
        /// Used to create a new <see cref="SerializableReference(object, Type)"/>.
        /// </param>
        /// 
        /// <param name="typeIndexer">
        /// Used to create a new <see cref="SerializableReference(object, Type)"/>.
        /// </param>
        public ObjectData(Type type, object referenceObject, Type typeIndexer)
        {
            name = type.FullName;
            assemblyQualifiedName = type.AssemblyQualifiedName;
            reference = new SerializableReference(referenceObject, typeIndexer);
        }

        /// <summary>
        /// Creates a new data structure instance.
        /// </summary>
        /// 
        /// <param name="name">
        /// Sets <see cref="name"/>.
        /// </param>
        /// 
        /// <param name="assemblyQualifiedName">
        /// Sets <see cref="assemblyQualifiedName"/>.
        /// </param>
        public ObjectData(string name, string assemblyQualifiedName)
        {
            this.name = name;
            this.assemblyQualifiedName = assemblyQualifiedName;
        }

        /// <summary>
        /// Creates a new data structure instance.
        /// </summary>
        /// 
        /// <param name="name">
        /// Sets <see cref="name"/>.
        /// </param>
        /// 
        /// <param name="assemblyQualifiedName">
        /// Sets <see cref="assemblyQualifiedName"/>.
        /// </param>
        /// 
        /// <param name="reference">
        /// Sets <see cref="reference"/>.
        /// </param>
        public ObjectData(string name, string assemblyQualifiedName, SerializableReference reference)
        {
            this.name = name;
            this.assemblyQualifiedName = assemblyQualifiedName;
            this.reference = reference;
        }

        /// <summary>
        /// Creates a new data structure instance.
        /// </summary>
        /// 
        /// <param name="name">
        /// Sets <see cref="name"/>.
        /// </param>
        /// 
        /// <param name="assemblyQualifiedName">
        /// Sets <see cref="assemblyQualifiedName"/>.
        /// </param>
        /// 
        /// <param name="referenceObject">
        /// Used to create a new <see cref="SerializableReference(object, Type)"/>.
        /// </param>
        /// 
        /// <param name="typeIndexer">
        /// Used to create a new <see cref="SerializableReference(object, Type)"/>.
        /// </param>
        public ObjectData(string name, string assemblyQualifiedName, object referenceObject, Type typeIndexer)
        {
            this.name = name;
            this.assemblyQualifiedName = assemblyQualifiedName;
            reference = new SerializableReference(referenceObject, typeIndexer);
        }

        /// <summary>
        /// Retrieves the type given from <see cref="assemblyQualifiedName"/>.
        /// </summary>
        /// <returns></returns>
        public Type GetDataType()
        {   return Type.GetType(assemblyQualifiedName);   }

        public override string ToString()
        {   return $"{GetType().Name}({name})";   }
    }
}
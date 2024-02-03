using System;

namespace UnitySimplified.Serialization 
{
    /// <summary>
    ///     An attribute tag for classes that allows custom serialization when using <see cref="DataSerializer"/>.
    ///     <em>
    ///     <br/>
    ///     Requires serializer class to inherit from <see cref="IDataSerializable"/>
    ///     </em>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomSerializer : Attribute
    {
        /// <summary>
        ///     The target class that this serializer will read and write from.
        /// </summary>
        public readonly Type inspectedType = null;

        /// <summary>
        ///     Determines whether attribute will also work on types that inherit from <see cref="inspectedType"/>.
        ///     <br/>
        ///     If <b><em>False</em></b>, this serializer will only apply to class types that are equivalent to <see cref="inspectedType"/>.
        /// </summary>
        public readonly bool useForDerivedClasses = false;

        /// <summary>
        ///     A value that determines which serializer to select. Built-in serializers have priority range of <b>-10</b> to <b>-1</b>.
        /// </summary>
        public readonly int overridePriority = 0;



        /// <summary><inheritdoc cref="CustomSerializer" path="/summary"/></summary>
        /// <param name="inspectedType"><inheritdoc cref="inspectedType" path="/summary"/></param>
        public CustomSerializer(Type inspectedType) => this.inspectedType = inspectedType;

        /// <summary><inheritdoc cref="CustomSerializer" path="/summary"/></summary>
        /// <param name="inspectedType"><inheritdoc cref="inspectedType" path="/summary"/></param>
        /// <param name="useForDerivedClasses"><inheritdoc cref="useForDerivedClasses" path="/summary"/></param>
        public CustomSerializer(Type inspectedType, bool useForDerivedClasses) : this(inspectedType) => this.useForDerivedClasses = useForDerivedClasses;

        /// <summary><inheritdoc cref="CustomSerializer" path="/summary"/></summary>
        /// <param name="inspectedType"><inheritdoc cref="inspectedType" path="/summary"/></param>
        /// <param name="overridePriority"><inheritdoc cref="overridePriority" path="/summary"/></param>
        public CustomSerializer(Type inspectedType, int overridePriority = 0) : this(inspectedType) => this.overridePriority = overridePriority;

        /// <summary><inheritdoc cref="CustomSerializer" path="/summary"/></summary>
        /// <param name="inspectedType"><inheritdoc cref="inspectedType" path="/summary"/></param>
        /// <param name="useForDerivedClasses"><inheritdoc cref="useForDerivedClasses" path="/summary"/></param>
        /// <param name="overridePriority"><inheritdoc cref="overridePriority" path="/summary"/></param>
        public CustomSerializer(Type inspectedType, bool useForDerivedClasses, int overridePriority = 0) : this(inspectedType, useForDerivedClasses) => this.overridePriority = overridePriority;
    }
}
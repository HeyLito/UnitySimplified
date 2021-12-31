using System;

namespace UnitySimplified.Serialization 
{
    /// <summary>
    ///     A class attribute tag that allows custom serialization support when using <see cref="DataSerializer"/>.
    ///     <em>
    ///     <br/>
    ///         Requires class to inherit from <see cref="IConvertibleData"/>
    ///     </em>
    /// </summary>
    public class CustomSerializer : Attribute
    {
        /// <summary>
        ///     The type in which this serializer will target.
        /// </summary>
        public readonly Type inspectedType = null;

        /// <summary>
        ///     Determines whether this serialization process applys to data types that are not equalivent to <strong><em>inspectedType</em></strong>, but can be assigned from it. 
        ///     When false, The process only applys to data types that are equalivent to <strong><em>inspectedType</em></strong>.
        /// </summary>
        public readonly bool allowsInheritance = false;

        /// <summary>
        ///     Sorts serializers by their importance. Built-in serializers have priority range of <strong>-1</strong> to <strong>-10</strong>.
        /// </summary>
        public readonly int priority = 0;

        /// <summary>
        ///     A class attribute tag that allows custom serialization support when using <see cref="DataSerializer"/>.
        ///     <em>
        ///     <br/>
        ///         Requires class to inherit from <see cref="IConvertibleData"/>
        ///     </em>
        /// </summary>
        /// <param name="inspectedType">
        ///     The type in which this serializer will target.
        /// </param>
        public CustomSerializer(Type inspectedType)
        {   this.inspectedType = inspectedType;   }

        /// <summary>
        ///     A class attribute tag that allows custom serialization support when using <see cref="DataSerializer"/>.
        ///     <em>
        ///     <br/> 
        ///         Requires class to inherit from <see cref="IConvertibleData"/>
        ///     </em>
        /// </summary>
        /// <param name="inspectedType">
        ///     The type in which this serializer will target.
        /// </param>
        /// <param name="allowsInheritance">
        ///     Determines whether this serialization process applys to data types that are not equalivent to <strong><em>inspectedType</em></strong>, but can be assigned from it. 
        ///     When false, The process only applys to data types that are equalivent to <strong><em>inspectedType</em></strong>.
        /// </param>
        public CustomSerializer(Type inspectedType, bool allowsInheritance)
        {
            this.inspectedType = inspectedType;
            this.allowsInheritance = allowsInheritance;
        }

        /// <summary>
        ///     A class attribute tag that allows custom serialization support when using <see cref="DataSerializer"/>.
        ///     <em>
        ///     <br/> 
        ///         Requires class to inherit from <see cref="IConvertibleData"/>
        ///     </em>
        /// </summary>
        /// <param name="inspectedType">
        ///     The type in which this serializer will target.
        /// </param>
        /// <param name="priority">
        ///     Sorts serializers by their importance. Built-in serializers have priority range of <strong>-1</strong> to <strong>-10</strong>.
        /// </param>
        public CustomSerializer(Type inspectedType, int priority = 0)
        {
            this.inspectedType = inspectedType;
            this.priority = priority;
        }

        /// <summary>
        ///     A class attribute tag that allows custom serialization support when using <see cref="DataSerializer"/>.
        ///     <em>
        ///     <br/> 
        ///         Requires class to inherit from <see cref="IConvertibleData"/>
        ///     </em>
        /// </summary>
        /// <param name="inspectedType">
        ///     The type in which this serializer will target.
        /// </param>
        /// <param name="allowsInheritance">
        ///     Determines whether this serialization process applys to data types that are not equalivent to <strong><em>inspectedType</em></strong>, but can be assigned from it. 
        ///     When false, The process only applys to data types that are equalivent to <strong><em>inspectedType</em></strong>.
        /// </param>
        /// <param name="priority">
        ///     Sorts serializers by their importance. Built-in serializers have priority range of <strong>-1</strong> to <strong>-10</strong>.
        /// </param>
        public CustomSerializer(Type inspectedType, bool allowsInheritance, int priority = 0)
        {
            this.inspectedType = inspectedType;
            this.allowsInheritance = allowsInheritance;
            this.priority = priority;
        }
    }
}
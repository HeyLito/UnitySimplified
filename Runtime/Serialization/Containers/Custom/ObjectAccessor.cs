using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    public class ObjectAccessor : Accessor
    {
        public override int SortPriority => -100;

        public override bool CanConvert(Type valueType) => true;

        public override void Set(object value)
        {
            //throw new NotImplementedException();
        }

        public override void Get(out object value)
        {
            value = default;
        }
    }

}
using System.Collections.Generic;
using System;
using UnityEngine;

namespace UnitySimplified.RuntimeDatabases
{
    public interface IRuntimeValueDatabase<T>
    {
        [Serializable]
        public struct Entry
        {
            [SerializeField]
            private string identifier;
            [SerializeField]
            private T value;

            public Entry(string identifier, T value)
            {
                this.identifier = identifier;
                this.value = value;
            }

            public string Identifier => identifier;
            public T Value => value;

            public static implicit operator KeyValuePair<string, T>(Entry self) => new(self.identifier, self.value);
            public static implicit operator KeyValuePair<string, object>(Entry self) => new(self.identifier, self.value);
        }

        public List<Entry> Entries { get; }
    }
}
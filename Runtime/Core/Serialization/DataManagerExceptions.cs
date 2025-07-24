using System;

namespace UnitySimplified.Serialization
{
    public class DataManagerException : Exception
    {
        public DataManagerException(string message) : base(message) { }
    }

    public class DataManagerInvalidPathException : DataManagerException
    {
        public DataManagerInvalidPathException(string message) : base(message) { }
    }

    public class DataManagerFileDatabaseException : DataManagerException
    {
        public DataManagerFileDatabaseException(string message) : base(message) { }
    }
}
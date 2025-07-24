using System;

namespace UnitySimplified.Serialization
{
    public class FileDatabaseException : Exception
    {
        public FileDatabaseException(string message) : base(message) { }
    }

    public class FileDatabaseInvalidPathException : FileDatabaseException
    {
        public FileDatabaseInvalidPathException(string message) : base(message) { }
    }
    public class FileDatabaseKeyDuplicateException : FileDatabaseException
    {
        public FileDatabaseKeyDuplicateException(string message) : base(message) { }
    }
    public class FileDatabaseKeyMissingException : FileDatabaseException
    {
        public FileDatabaseKeyMissingException(string message) : base(message) { }
    }
}
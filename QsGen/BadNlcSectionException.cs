using System;
using System.Runtime.Serialization;

namespace QsGen
{
    [Serializable]
    internal class BadNlcSectionException : Exception
    {
        public BadNlcSectionException()
        {
        }

        public BadNlcSectionException(string message) : base(message)
        {
        }

        public BadNlcSectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BadNlcSectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
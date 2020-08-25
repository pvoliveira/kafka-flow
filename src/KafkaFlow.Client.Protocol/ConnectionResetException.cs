using System;
using System.Runtime.Serialization;

namespace KafkaFlow.Client.Protocol
{
    [Serializable]
    internal class ConnectionResetException : Exception
    {
        public ConnectionResetException()
        {
        }

        public ConnectionResetException(string? message) : base(message)
        {
        }

        public ConnectionResetException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ConnectionResetException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
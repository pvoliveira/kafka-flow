namespace KafkaFlow.Client.Protocol
{
    using System.Threading;

    public static class Protocol
    {
        public static ProtocolWriter CreateWriter(this SocketConnection connection)
            => new ProtocolWriter(connection.Transport.Output);

        public static ProtocolWriter CreateWriter(this SocketConnection connection, SemaphoreSlim semaphore)
            => new ProtocolWriter(connection.Transport.Output, semaphore);

        public static ProtocolReader CreateReader(this SocketConnection connection)
            => new ProtocolReader(connection.Transport.Input);
    }
}

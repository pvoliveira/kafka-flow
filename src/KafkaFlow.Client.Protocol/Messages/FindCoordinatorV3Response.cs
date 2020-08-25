namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Buffers;
    using System.IO;

    public class FindCoordinatorV3Response : IResponseV2
    {
        public int ThrottleTimeMs { get; private set; }

        public ErrorCode Error { get; private set; }

        public string ErrorMessage { get; private set; }

        public int NodeId { get; private set; }

        public string Host { get; private set; }

        public int Port { get; private set; }

        public TaggedField[] TaggedFields { get; private set; }

        public void Read(ref SequenceReader<byte> source)
        {
            this.ThrottleTimeMs = BufferExtensions.ReadInt32(ref source);
            this.Error = BufferExtensions.ReadErrorCode(ref source);
            this.ErrorMessage = BufferExtensions.ReadCompactString(ref source);
            this.NodeId = BufferExtensions.ReadInt32(ref source);
            this.Host = BufferExtensions.ReadCompactString(ref source);
            this.Port = BufferExtensions.ReadInt32(ref source);
            this.TaggedFields = BufferExtensions.ReadTaggedFields(ref source);
        }
    }
}
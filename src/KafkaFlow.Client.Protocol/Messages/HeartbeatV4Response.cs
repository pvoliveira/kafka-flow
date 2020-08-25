namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Buffers;
    using System.IO;

    public class HeartbeatV4Response : IResponseV2
    {
        public int ThrottleTimeMs { get; private set; }

        public ErrorCode Error { get; private set; }

        public TaggedField[] TaggedFields { get; private set; }

        public void Read(ref SequenceReader<byte> source)
        {
            this.ThrottleTimeMs = BufferExtensions.ReadInt32(ref source);
            this.Error = BufferExtensions.ReadErrorCode(ref source);
            this.TaggedFields = BufferExtensions.ReadTaggedFields(ref source);
        }
    }
}

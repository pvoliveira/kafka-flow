namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Buffers;
    using System.IO;

    public class SyncGroupV5Response : IResponseV2
    {
        public int ThrottleTimeMs { get; private set; }

        public ErrorCode Error { get; private set; }

        public string? ProtocolType { get; private set; }

        public string? ProtocolName { get; private set; }

        public byte[] AssignmentMetadata { get; private set; }

        public TaggedField[] TaggedFields { get; private set; }

        public void Read(ref SequenceReader<byte> source)
        {
            this.ThrottleTimeMs = BufferExtensions.ReadInt32(ref source);
            this.Error = BufferExtensions.ReadErrorCode(ref source);
            this.ProtocolType = BufferExtensions.ReadCompactNullableString(ref source);
            this.ProtocolName = BufferExtensions.ReadCompactNullableString(ref source);
            this.AssignmentMetadata = BufferExtensions.ReadCompactByteArray(ref source);
            this.TaggedFields = BufferExtensions.ReadTaggedFields(ref source);
        }
    }
}

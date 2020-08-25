namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Buffers;
    using System.IO;

    public class JoinGroupV7Response : IResponseV2
    {
        public int ThrottleTimeMs { get; private set; }

        public ErrorCode Error { get; private set; }

        public int GenerationId { get; private set; }

        public string? ProtocolType { get; private set; }

        public string? ProtocolName { get; private set; }

        public string LeaderId { get; private set; }

        public string MemberId { get; private set; }

        public Member[] Members { get; private set; }

        public TaggedField[] TaggedFields { get; private set; }

        public void Read(ref SequenceReader<byte> source)
        {
            this.ThrottleTimeMs = BufferExtensions.ReadInt32(ref source);
            this.Error = (ErrorCode) BufferExtensions.ReadInt16(ref source);
            this.GenerationId = BufferExtensions.ReadInt32(ref source);
            this.ProtocolType = BufferExtensions.ReadCompactNullableString(ref source);
            this.ProtocolName = BufferExtensions.ReadCompactNullableString(ref source);
            this.LeaderId = BufferExtensions.ReadCompactString(ref source);
            this.MemberId = BufferExtensions.ReadCompactString(ref source);
            this.Members = BufferExtensions.ReadCompactArray<Member>(ref source);
            this.TaggedFields = BufferExtensions.ReadTaggedFields(ref source);
        }

        public class Member : IResponseV2
        {
            public string MemberId { get; private set; }

            public string? GroupInstanceId { get; private set; }

            public byte[] Metadata { get; private set; }

            public TaggedField[] TaggedFields { get; private set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.MemberId = BufferExtensions.ReadCompactString(ref source);
                this.GroupInstanceId = BufferExtensions.ReadCompactNullableString(ref source);
                this.Metadata = BufferExtensions.ReadCompactByteArray(ref source);
                this.TaggedFields = BufferExtensions.ReadTaggedFields(ref source);
            }
        }
    }
}

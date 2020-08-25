namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Buffers;

    public class OffsetFetchV5Response : IResponse
    {
        public int ThrottleTimeMs { get; set; }

        public Topic[] Topics { get; set; }

        public ErrorCode Error { get; set; }

        public void Read(ref SequenceReader<byte> source)
        {
            this.ThrottleTimeMs = BufferExtensions.ReadInt32(ref source);
            this.Topics = BufferExtensions.ReadArray<Topic>(ref source);
            this.Error = BufferExtensions.ReadErrorCode(ref source);
        }

        public class Topic : IResponse
        {
            public string Name { get; set; }

            public Partition[] Partitions { get; set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.Name = BufferExtensions.ReadString(ref source);
                this.Partitions = BufferExtensions.ReadArray<Partition>(ref source);
            }
        }

        public class Partition : IResponse
        {
            public int Id { get; set; }

            public long CommittedOffset { get; set; }

            public int CommittedLeaderEpoch { get; set; }

            public string Metadata { get; set; }

            public short ErrorCode { get; set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.Id = BufferExtensions.ReadInt32(ref source);
                this.CommittedOffset = BufferExtensions.ReadInt64(ref source);
                this.CommittedLeaderEpoch = BufferExtensions.ReadInt32(ref source);
                this.Metadata = BufferExtensions.ReadString(ref source);
                this.ErrorCode = BufferExtensions.ReadInt16(ref source);
            }
        }
    }
}

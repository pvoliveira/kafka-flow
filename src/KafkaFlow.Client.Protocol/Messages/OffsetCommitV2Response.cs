namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Buffers;

    public class OffsetCommitV2Response : IResponse
    {
        public Topic[] Topics { get; private set; }

        public void Read(ref SequenceReader<byte> source)
        {
            this.Topics = BufferExtensions.ReadArray<Topic>(ref source);
        }

        public class Topic : IResponse
        {
            public string Name { get; private set; }

            public Partition[] Partitions { get; private set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.Name = BufferExtensions.ReadString(ref source);
                this.Partitions = BufferExtensions.ReadArray<Partition>(ref source);
            }
        }

        public class Partition : IResponse
        {
            public int Id { get; private set; }

            public ErrorCode Error { get; private set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.Id = BufferExtensions.ReadInt32(ref source);
                this.Error = BufferExtensions.ReadErrorCode(ref source);
            }
        }
    }
}

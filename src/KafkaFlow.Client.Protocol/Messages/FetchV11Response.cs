namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Buffers;
    using System.IO;

    public class FetchV11Response : IResponse
    {
        public int ThrottleTimeMs { get; set; }

        public ErrorCode Error { get; set; }

        public int SessionId { get; set; }

        public Topic[] Topics { get; set; }

        public void Read(ref SequenceReader<byte> source)
        {
            this.ThrottleTimeMs = BufferExtensions.ReadInt32(ref source);
            this.Error = BufferExtensions.ReadErrorCode(ref source);
            this.SessionId = BufferExtensions.ReadInt32(ref source);
            this.Topics = BufferExtensions.ReadArray<Topic>(ref source);
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
            public PartitionHeader Header { get; set; }

            public RecordBatch RecordBatch { get; set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.Header = BufferExtensions.ReadMessage<PartitionHeader>(ref source);
                this.RecordBatch = BufferExtensions.ReadMessage<RecordBatch>(ref source);
            }
        }

        public class PartitionHeader : IResponse
        {
            public int Id { get; set; }

            public ErrorCode Error { get; set; }

            public long HighWatermark { get; set; }

            public long LastStableOffset { get; set; }

            public long LogStartOffset { get; set; }

            public AbortedTransaction[] AbortedTransactions { get; set; }

            public int PreferredReadReplica { get; set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.Id = BufferExtensions.ReadInt32(ref source);
                this.Error = BufferExtensions.ReadErrorCode(ref source);
                this.HighWatermark = BufferExtensions.ReadInt64(ref source);
                this.LastStableOffset = BufferExtensions.ReadInt64(ref source);
                this.LogStartOffset = BufferExtensions.ReadInt64(ref source);
                this.AbortedTransactions = BufferExtensions.ReadArray<AbortedTransaction>(ref source);
                this.PreferredReadReplica = BufferExtensions.ReadInt32(ref source);
            }
        }

        public class AbortedTransaction : IResponse
        {
            public long ProducerId { get; set; }

            public long FirstOffset { get; set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.ProducerId = BufferExtensions.ReadInt64(ref source);
                this.FirstOffset = BufferExtensions.ReadInt64(ref source);
            }
        }
    }
}

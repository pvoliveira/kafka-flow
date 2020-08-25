namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Buffers;

    public class ProduceV8Response : IResponse
    {
        public Topic[] Topics { get; set; }

        public int ThrottleTimeMs { get; set; }

        public void Read(ref SequenceReader<byte> source)
        {
            this.Topics = BufferExtensions.ReadArray<Topic>(ref source);
            this.ThrottleTimeMs = BufferExtensions.ReadInt32(ref source);
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

            public ErrorCode Error { get; set; }

            public long Offset { get; set; }

            public long LogAppendTime { get; set; }

            public long LogStartOffset { get; set; }

            public RecordError[] Errors { get; set; }

            public string? ErrorMessage { get; set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.Id = BufferExtensions.ReadInt32(ref source);
                this.Error = BufferExtensions.ReadErrorCode(ref source);
                this.Offset = BufferExtensions.ReadInt64(ref source);
                this.LogAppendTime = BufferExtensions.ReadInt64(ref source);
                this.LogStartOffset = BufferExtensions.ReadInt64(ref source);
                this.Errors = BufferExtensions.ReadArray<RecordError>(ref source);
                this.ErrorMessage = BufferExtensions.ReadNullableString(ref source);
            }
        }

        public class RecordError : IResponse
        {
            public int BatchIndex { get; set; }

            public string? Message { get; set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.BatchIndex = BufferExtensions.ReadInt32(ref source);
                this.Message = BufferExtensions.ReadNullableString(ref source);
            }
        }
    }
}

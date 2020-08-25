namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Buffers;
    using System.IO;

    public class ApiVersionV2Response : IResponse
    {
        public ErrorCode Error { get; private set; }

        public ApiVersion[] ApiVersions { get; private set; }

        public int ThrottleTime { get; private set; }

        public void Read(ref SequenceReader<byte> source)
        {
            this.Error = BufferExtensions.ReadErrorCode(ref source);
            this.ApiVersions = BufferExtensions.ReadArray<ApiVersion>(ref source);
            this.ThrottleTime = BufferExtensions.ReadInt32(ref source);
        }

        public class ApiVersion : IResponse
        {
            public short ApiKey { get; private set; }

            public short MinVersion { get; private set; }

            public short MaxVersion { get; private set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.ApiKey = BufferExtensions.ReadInt16(ref source);
                this.MinVersion = BufferExtensions.ReadInt16(ref source);
                this.MaxVersion = BufferExtensions.ReadInt16(ref source);
            }
        }
    }
}

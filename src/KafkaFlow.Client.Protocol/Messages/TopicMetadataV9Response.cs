namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Buffers;
    using System.IO;

    public class TopicMetadataV9Response : IResponseV2
    {
        public int ThrottleTime { get; private set; }
        public Broker[] Brokers { get; private set; }
        public string? ClusterId { get; private set; }
        public int ControllerId { get; private set; }
        public Topic[] Topics { get; private set; }
        public int ClusterAuthorizedOperations { get; private set; }
        public TaggedField[] TaggedFields { get; private set; }

        public void Read(ref SequenceReader<byte> source)
        {
            this.ThrottleTime = BufferExtensions.ReadInt32(ref source);
            this.Brokers = BufferExtensions.ReadCompactArray<Broker>(ref source);
            this.ClusterId = BufferExtensions.ReadCompactNullableString(ref source);
            this.ControllerId = BufferExtensions.ReadInt32(ref source);
            this.Topics = BufferExtensions.ReadCompactArray<Topic>(ref source);
            this.ClusterAuthorizedOperations = BufferExtensions.ReadInt32(ref source);
            this.TaggedFields = BufferExtensions.ReadTaggedFields(ref source);
        }

        public class Topic : IResponseV2
        {
            public ErrorCode Error { get; private set; }
            public string Name { get; private set; }
            public bool IsInternal { get; private set; }
            public Partition[] Partitions { get; private set; }
            public int TopicAuthorizedOperations { get; set; }
            public TaggedField[] TaggedFields { get; private set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.Error = BufferExtensions.ReadErrorCode(ref source);
                this.Name = BufferExtensions.ReadCompactString(ref source);
                this.IsInternal = BufferExtensions.ReadBoolean(ref source);
                this.Partitions = BufferExtensions.ReadCompactArray<Partition>(ref source);
                this.TopicAuthorizedOperations = BufferExtensions.ReadInt32(ref source);
                this.TaggedFields = BufferExtensions.ReadTaggedFields(ref source);
            }
        }

        public class Partition : IResponseV2
        {
            public ErrorCode Error { get; set; }
            public int Id { get; private set; }
            public int LeaderId { get; private set; }
            public int LeaderEpoch { get; private set; }
            public int[] ReplicaNodes { get; private set; }
            public int[] IsrNodes { get; private set; }
            public int[] OfflineReplicas { get; private set; }
            public TaggedField[] TaggedFields { get; private set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.Error = BufferExtensions.ReadErrorCode(ref source);
                this.Id = BufferExtensions.ReadInt32(ref source);
                this.LeaderId = BufferExtensions.ReadInt32(ref source);
                this.LeaderEpoch = BufferExtensions.ReadInt32(ref source);
                this.ReplicaNodes = BufferExtensions.ReadCompactInt32Array(ref source);
                this.IsrNodes = BufferExtensions.ReadCompactInt32Array(ref source);
                this.OfflineReplicas = BufferExtensions.ReadCompactInt32Array(ref source);
                this.TaggedFields = BufferExtensions.ReadTaggedFields(ref source);
            }
        }

        public class Broker : IResponseV2
        {
            public int NodeId { get; private set; }
            public string Host { get; private set; }
            public int Port { get; private set; }
            public string? Rack { get; private set; }
            public TaggedField[] TaggedFields { get; private set; }

            public void Read(ref SequenceReader<byte> source)
            {
                this.NodeId = BufferExtensions.ReadInt32(ref source);
                this.Host = BufferExtensions.ReadCompactString(ref source);
                this.Port = BufferExtensions.ReadInt32(ref source);
                this.Rack = BufferExtensions.ReadCompactNullableString(ref source);
                this.TaggedFields = BufferExtensions.ReadTaggedFields(ref source);
            }
        }
    }
}

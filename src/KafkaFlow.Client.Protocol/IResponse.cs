namespace KafkaFlow.Client.Protocol
{
    using System.Buffers;
    using System.IO;
    using KafkaFlow.Client.Protocol.Messages;

    public interface IResponse
    {
        void Read(ref SequenceReader<byte> source);
    }

    public interface IResponseV2 : IResponse
    {
        TaggedField[] TaggedFields { get; }
    }
}

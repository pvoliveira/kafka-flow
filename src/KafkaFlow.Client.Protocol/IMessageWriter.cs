using System.Buffers;

namespace KafkaFlow.Client.Protocol
{
    public interface IMessageWriter<TMessage>
    {
        void WriteMessage(TMessage message, IBufferWriter<byte> output);
    }
}

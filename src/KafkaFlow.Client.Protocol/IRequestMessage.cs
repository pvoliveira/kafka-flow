namespace KafkaFlow.Client.Protocol
{
    using KafkaFlow.Client.Protocol.Messages;

    public interface IRequestMessage<TResponse> : IRequestMessage where TResponse : IResponse
    {
    }

    public interface IRequestMessageV2<TResponse> : IRequestMessage<TResponse>, IRequestV2 where TResponse : IResponseV2
    {
    }

    public interface IRequestMessage : IRequest
    {
        ApiKey ApiKey { get; }

        short ApiVersion { get; }
    }
}

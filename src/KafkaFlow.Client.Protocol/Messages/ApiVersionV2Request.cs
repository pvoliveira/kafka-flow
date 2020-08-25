namespace KafkaFlow.Client.Protocol.Messages
{
    using System.IO;

    public class ApiVersionV2Request : IRequestMessage<ApiVersionV2Response>
    {
        public ApiKey ApiKey => ApiKey.ApiVersions;

        public short ApiVersion => 2;

        public void Write(Stream destination)
        {
            destination.WriteInt16((short)ApiKey);
            destination.WriteInt16(ApiVersion);
        }
    }
}

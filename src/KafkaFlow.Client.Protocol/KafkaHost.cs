namespace KafkaFlow.Client.Protocol
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public class KafkaHost : IAsyncDisposable
    {
        protected SocketConnection Socket;
        protected ProtocolReader MessageReader;
        protected ProtocolWriter MessageWriter;

        private readonly ConcurrentDictionary<int, PendingRequest> pendingRequests = new ConcurrentDictionary<int, PendingRequest>();

        private int lastCorrelationId;

        private readonly CancellationTokenSource stopTokenSource = new CancellationTokenSource();
        protected Task listenerTask;

        private readonly string clientId;

        protected KafkaHost(string host, int port, string clientId)
        {
            this.clientId = clientId;

            this.Socket = new SocketConnection(new DnsEndPoint(host, port));
        }

        public static async Task<KafkaHost> MakeHostAsync(string host, int port, string clientId)
        {
            var kafkaHost = new KafkaHost(host, port, clientId);

            await kafkaHost.Socket.StartAsync();

            kafkaHost.MessageReader = kafkaHost.Socket.CreateReader();
            kafkaHost.MessageWriter = kafkaHost.Socket.CreateWriter();

            kafkaHost.listenerTask = Task.Run(kafkaHost.ListenStream);

            return kafkaHost;
        }

        protected async Task ListenStream()
        {
            while (!this.stopTokenSource.IsCancellationRequested)
            {
                try
                {
                    await this.MessageReader.ReadAsync(this.pendingRequests);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                finally
                {
                    this.MessageReader.Advance();
                }
            }
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequestMessage<TResponse> request, TimeSpan timeout)
            where TResponse : IResponse, new()
        {
            var pendingRequest = new PendingRequest(
                timeout,
                typeof(TResponse));

            this.pendingRequests.TryAdd(++this.lastCorrelationId, pendingRequest);

            await this.MessageWriter.WriteAsync(
                    new Request(
                        this.lastCorrelationId,
                        this.clientId,
                        request));

            return await pendingRequest.GetTask<TResponse>();
        }

        public async ValueTask DisposeAsync()
        {
            this.stopTokenSource.Cancel();
            await this.listenerTask;
            this.listenerTask.Dispose();
            await this.Socket.DisposeAsync();
        }

        public class PendingRequest
        {
            public TimeSpan Timeout { get; }

            public Type ResponseType { get; }

            public readonly TaskCompletionSource<IResponse> CompletionSource =
                new TaskCompletionSource<IResponse>();

            public PendingRequest(TimeSpan timeout, Type responseType)
            {
                this.Timeout = timeout;
                this.ResponseType = responseType;
            }

            public Task<TResponse> GetTask<TResponse>() where TResponse : IResponse
            {
                _ = Task.Delay(Timeout)
                    .ContinueWith(_ => this.CompletionSource.TrySetException(new TimeoutException()));
                return this.CompletionSource.Task.ContinueWith((x) => !x.IsFaulted ? (TResponse)x.Result : default);
            }
        }
    }
}

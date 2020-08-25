namespace KafkaFlow.Client.Protocol
{
    using System;
    using System.Buffers;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipelines;
    using System.Threading;
    using System.Threading.Tasks;

    public class ProtocolWriter : IAsyncDisposable
    {
        private readonly PipeWriter writer;
        private readonly SemaphoreSlim semaphore;
        private bool disposed;

        public ProtocolWriter(Stream stream) :
            this(PipeWriter.Create(stream))
        {

        }

        public ProtocolWriter(PipeWriter writer)
            : this(writer, new SemaphoreSlim(1))
        {
        }

        public ProtocolWriter(PipeWriter writer, SemaphoreSlim semaphore)
        {
            this.writer = writer;
            this.semaphore = semaphore;
        }

        public async ValueTask WriteAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (disposed)
                {
                    return;
                }

                request.Write(this.writer.AsStream());

                var result = await this.writer.FlushAsync(cancellationToken).ConfigureAwait(false);

                if (result.IsCanceled)
                {
                    throw new OperationCanceledException();
                }

                if (result.IsCompleted)
                {
                    disposed = true;
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}

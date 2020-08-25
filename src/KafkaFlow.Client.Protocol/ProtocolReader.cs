namespace KafkaFlow.Client.Protocol
{
    using System;
    using System.Buffers;
    using System.Collections.Concurrent;
    using System.IO;
    using System.IO.Pipelines;
    using System.Threading;
    using System.Threading.Tasks;
    using static KafkaFlow.Client.Protocol.KafkaHost;

    public class ProtocolReader : IAsyncDisposable
    {
        private readonly PipeReader reader;
        private SequencePosition examined;
        private SequencePosition consumed;
        private ReadOnlySequence<byte> buffer;
        private bool isCanceled;
        private bool isCompleted;
        private bool hasMessage;
        private bool disposed;

        public ProtocolReader(Stream stream) :
            this(PipeReader.Create(stream))
        {

        }

        public ProtocolReader(PipeReader reader)
        {
            this.reader = reader;
        }

        public ValueTask ReadAsync(ConcurrentDictionary<int, PendingRequest> pendingRequests, CancellationToken cancellationToken = default)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (hasMessage)
            {
                throw new InvalidOperationException($"{nameof(Advance)} must be called before calling {nameof(ReadAsync)}");
            }

            // If this is the very first read, then make it go async since we have no data
            if (consumed.GetObject() == null)
            {
                return DoAsyncRead(pendingRequests, cancellationToken);
            }

            // We have a buffer, test to see if there's any message left in the buffer
            if (TryParseMessage(pendingRequests, buffer))
            {
                hasMessage = true;
                return new ValueTask(Task.CompletedTask);
            }
            else
            {
                // We couldn't parse the message so advance the input so we can read
                this.reader.AdvanceTo(consumed, examined);
            }

            if (isCompleted)
            {
                consumed = default;
                examined = default;

                // If we're complete then short-circuit
                if (!buffer.IsEmpty)
                {
                    throw new InvalidDataException("Connection terminated while reading a message.");
                }

                return new ValueTask(Task.CompletedTask);
            }

            return DoAsyncRead(pendingRequests, cancellationToken);
        }

        private async ValueTask DoAsyncRead(ConcurrentDictionary<int, PendingRequest> pendingRequests, CancellationToken cancellationToken)
        {
            while (true)
            {
                var result = await this.reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                buffer = result.Buffer;
                isCanceled = result.IsCanceled;
                isCompleted = result.IsCompleted;
                consumed = buffer.Start;
                examined = buffer.End;

                if (isCanceled)
                {
                    break;
                }

                if (TryParseMessage(pendingRequests, buffer))
                {
                    hasMessage = true;
                    return;
                }
                else
                {
                    this.reader.AdvanceTo(consumed, examined);
                }

                if (isCompleted)
                {
                    consumed = default;
                    examined = default;

                    if (!buffer.IsEmpty)
                    {
                        throw new InvalidDataException("Connection terminated while reading a message.");
                    }

                    break;
                }
            };
        }

        private bool TryParseMessage(ConcurrentDictionary<int, PendingRequest>  pendingRequests, in ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);
            if (!reader.TryReadBigEndian(out int length)
                || buffer.Length < length)
            {
                return false;
            }

            reader.TryReadBigEndian(out int correlationId);

            if (!pendingRequests.TryRemove(correlationId, out var request))
            {
                return false;
            }

            var message = (IResponse)Activator.CreateInstance(request.ResponseType)!;

            if (message is IResponseV2)
                _ = BufferExtensions.ReadTaggedFields(ref reader);

            message.Read(ref reader);

            request.CompletionSource.TrySetResult(message);

            consumed = reader.Position;
            examined = consumed;
            return true;
        }


        public void Advance()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            isCanceled = false;

            if (!hasMessage)
            {
                return;
            }

            buffer = buffer.Slice(consumed);

            hasMessage = false;
        }

        public ValueTask DisposeAsync()
        {
            disposed = true;
            return default;
        }
    }
}

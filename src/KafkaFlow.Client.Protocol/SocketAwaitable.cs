namespace KafkaFlow.Client.Protocol
{
    using System;
    using System.Diagnostics;
    using System.IO.Pipelines;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal class SocketAwaitable : ICriticalNotifyCompletion
    {
        private static readonly Action callbackCompleted = () => { };

        private readonly PipeScheduler ioScheduler;

        private Action callback;
        private int bytesTransferred;
        private SocketError _error;

        public SocketAwaitable(PipeScheduler ioScheduler)
        {
            this.ioScheduler = ioScheduler;
        }

        public SocketAwaitable GetAwaiter() => this;
        public bool IsCompleted => ReferenceEquals(callback, callbackCompleted);

        public int GetResult()
        {
            Debug.Assert(ReferenceEquals(callback, callbackCompleted));

            callback = null;

            if (_error != SocketError.Success)
            {
                throw new SocketException((int)_error);
            }

            return bytesTransferred;
        }

        public void OnCompleted(Action continuation)
        {
            if (ReferenceEquals(callback, callbackCompleted) ||
                ReferenceEquals(Interlocked.CompareExchange(ref callback, continuation, null), callbackCompleted))
            {
                Task.Run(continuation);
            }
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }

        public void Complete(int bytesTransferred, SocketError socketError)
        {
            _error = socketError;
            this.bytesTransferred = bytesTransferred;
            var continuation = Interlocked.Exchange(ref callback, callbackCompleted);

            if (continuation != null)
            {
                ioScheduler.Schedule(state => ((Action)state)(), continuation);
            }
        }
    }
}

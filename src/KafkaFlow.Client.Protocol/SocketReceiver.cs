namespace KafkaFlow.Client.Protocol
{
    using System;
    using System.IO.Pipelines;
    using System.Net.Sockets;

    internal class SocketReceiver
    {
        private readonly Socket socket;
        private readonly SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
        private readonly SocketAwaitable awaitable;

        public SocketReceiver(Socket socket, PipeScheduler scheduler)
        {
            this.socket = socket;
            awaitable = new SocketAwaitable(scheduler);
            eventArgs.UserToken = awaitable;
            eventArgs.Completed += (_, e) => ((SocketAwaitable)e.UserToken).Complete(e.BytesTransferred, e.SocketError);
        }

        public SocketAwaitable ReceiveAsync(Memory<byte> buffer)
        {
            eventArgs.SetBuffer(buffer);

            if (!socket.ReceiveAsync(eventArgs))
            {
                awaitable.Complete(eventArgs.BytesTransferred, eventArgs.SocketError);
            }

            return awaitable;
        }
    }
}

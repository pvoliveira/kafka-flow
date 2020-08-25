namespace KafkaFlow.Client.Protocol
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Pipelines;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class SocketSender
    {
        private readonly Socket socket;
        private readonly SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
        private readonly SocketAwaitable awaitable;

        private List<ArraySegment<byte>> _bufferList;

        public SocketSender(Socket socket, PipeScheduler scheduler)
        {
            this.socket = socket;
            awaitable = new SocketAwaitable(scheduler);
            eventArgs.UserToken = awaitable;
            eventArgs.Completed += (_, e) => ((SocketAwaitable)e.UserToken).Complete(e.BytesTransferred, e.SocketError);
        }

        public SocketAwaitable SendAsync(in ReadOnlySequence<byte> buffers)
        {
            if (buffers.IsSingleSegment)
            {
                return SendAsync(buffers.First);
            }

            if (eventArgs.MemoryBuffer.Equals(Memory<byte>.Empty))
            {
                eventArgs.SetBuffer(null, 0, 0);
            }

            eventArgs.BufferList = GetBufferList(buffers);

            if (!socket.SendAsync(eventArgs))
            {
                awaitable.Complete(eventArgs.BytesTransferred, eventArgs.SocketError);
            }

            return awaitable;
        }

        private SocketAwaitable SendAsync(ReadOnlyMemory<byte> memory)
        {
            // The BufferList getter is much less expensive then the setter.
            if (eventArgs.BufferList != null)
            {
                eventArgs.BufferList = null;
            }


            eventArgs.SetBuffer(MemoryMarshal.AsMemory(memory));

            if (!socket.SendAsync(eventArgs))
            {
                awaitable.Complete(eventArgs.BytesTransferred, eventArgs.SocketError);
            }

            return awaitable;
        }

        private List<ArraySegment<byte>> GetBufferList(in ReadOnlySequence<byte> buffer)
        {
            Debug.Assert(!buffer.IsEmpty);
            Debug.Assert(!buffer.IsSingleSegment);

            if (_bufferList == null)
            {
                _bufferList = new List<ArraySegment<byte>>();
            }
            else
            {
                // Buffers are pooled, so it's OK to root them until the next multi-buffer write.
                _bufferList.Clear();
            }

            foreach (var b in buffer)
            {
                _bufferList.Add(b.GetArray());
            }

            return _bufferList;
        }
    }
}

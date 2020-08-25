namespace KafkaFlow.Client.Protocol.Messages
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class TaggedField : IRequest, IResponse
    {
        public int Tag { get; private set; }

        public byte[] Data { get; private set; } = Array.Empty<byte>();

        public void Write(Stream destination)
        {
            destination.WriteUVarint((ulong) this.Tag);
            destination.WriteUVarint((ulong) this.Data.Length);
            destination.Write(this.Data);
        }

        public void Read(ref SequenceReader<byte> source)
        {
            this.Tag = BufferExtensions.ReadUVarint(ref source);
            this.Data = BufferExtensions.ReadBytes(ref source, BufferExtensions.ReadUVarint(ref source));
        }
    }
}

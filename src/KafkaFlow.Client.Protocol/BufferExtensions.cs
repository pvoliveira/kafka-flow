namespace KafkaFlow.Client.Protocol
{
    using System;
    using System.Buffers;
    using System.Buffers.Binary;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using KafkaFlow.Client.Protocol.Messages;

    public static class BufferExtensions
    {
        public static ArraySegment<byte> GetArray(this Memory<byte> memory)
        {
            return ((ReadOnlyMemory<byte>)memory).GetArray();
        }

        public static ArraySegment<byte> GetArray(this ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteMessage(this IBufferWriter<byte> destination, IRequest message) =>
            throw new NotImplementedException();  // message.Write(destination.);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteByte(this IBufferWriter<byte> destination, byte value)
        {
            MemoryMarshal.Write(destination.GetSpan(1), ref value);
            destination.Advance(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this IBufferWriter<byte> destination, byte[] value)
        {
            var tmp = destination.GetSpan(value.Length);
            value.CopyTo(tmp);
            destination.Advance(value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16(this IBufferWriter<byte> destination, short value)
        {
            var tmp = destination.GetSpan(2);
            BinaryPrimitives.WriteInt16BigEndian(tmp, value);
            destination.Advance(2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32(this IBufferWriter<byte> destination, int value)
        {
            var tmp = destination.GetSpan(4); // stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(tmp, value);
            destination.Advance(4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64(this IBufferWriter<byte> destination, long value)
        {
            var tmp = destination.GetSpan(8);  //stackalloc byte[8];
            BinaryPrimitives.WriteInt64BigEndian(tmp, value);
            destination.Advance(8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32Array(this IBufferWriter<byte> destination, ReadOnlySpan<int> values)
        {
            destination.WriteInt32(values.Length);
            InternalWriteInt32Array(destination, values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCompactInt32Array(this IBufferWriter<byte> destination, ReadOnlySpan<int> values)
        {
            destination.WriteUVarint((uint)values.Length);
            InternalWriteInt32Array(destination, values);
        }

        private static void InternalWriteInt32Array(IBufferWriter<byte> destination, ReadOnlySpan<int> values)
        {
            for (var i = 0; i < values.Length; ++i)
            {
                destination.WriteInt32(values[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteString(this IBufferWriter<byte> destination, ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
            {
                destination.WriteInt16(-1);
                return;
            }

            destination.WriteInt16(Convert.ToInt16(value.Length));

            var tmp = destination.GetSpan(value.Length);
            Encoding.UTF8.GetBytes(value, tmp);
            destination.Write(tmp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCompactString(this IBufferWriter<byte> destination, string value)
        {
            destination.WriteUVarint((uint)value.Length + 1u);
            destination.Write(Encoding.UTF8.GetBytes(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCompactNullableString(this IBufferWriter<byte> destination, string? value)
        {
            if (value is null)
            {
                destination.WriteUVarint(0);
                return;
            }

            destination.WriteCompactString(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCompactNullableByteArray(this IBufferWriter<byte> destination, byte[]? data)
        {
            if (data is null)
            {
                destination.WriteUVarint(0);
                return;
            }

            destination.WriteCompactByteArray(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCompactByteArray(this IBufferWriter<byte> destination, byte[] data)
        {
            destination.WriteUVarint((uint)data.Length + 1u);
            destination.Write(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBoolean(this IBufferWriter<byte> destination, bool value)
        {
            destination.WriteByte((byte)(value ? 1 : 0));
        }

        public static void WriteArray<TMessage>(this IBufferWriter<byte> destination, TMessage[] items)
            where TMessage : IRequest
        {
            destination.WriteInt32(items.Length);

            for (var i = 0; i < items.Length; ++i)
            {
                destination.WriteMessage(items[i]);
            }
        }

        public static void WriteTaggedFields(this IBufferWriter<byte> destination, TaggedField[] items)
        {
            destination.WriteUVarint((uint)items.Length);

            for (var i = 0; i < items.Length; ++i)
            {
                destination.WriteMessage(items[i]);
            }
        }

        public static void WriteCompactArray<TMessage>(this IBufferWriter<byte> destination, TMessage[] items)
            where TMessage : IRequest
        {
            destination.WriteUVarint((uint)items.Length + 1);

            for (var i = 0; i < items.Length; ++i)
            {
                destination.WriteMessage(items[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadBoolean(ref SequenceReader<byte> source) => ReadByte(ref source) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ErrorCode ReadErrorCode(ref SequenceReader<byte> source) => (ErrorCode)ReadInt16(ref source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16(ref SequenceReader<byte> source)
        {
            _ = source.TryReadBigEndian(out short value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32(ref SequenceReader<byte> source)
        {
            _ = source.TryReadBigEndian(out int value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64(ref SequenceReader<byte> source)
        {
            _ = source.TryReadBigEndian(out long value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(ref SequenceReader<byte> source)
        {
            _ = source.TryRead(out byte value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ReadBytes(ref SequenceReader<byte> source, int count)
        {
            var value = source.CurrentSpan.Slice(source.CurrentSpanIndex, count).ToArray();
            source.Advance(count);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? ReadString(ref SequenceReader<byte> source)
        {
            return ReadString(ref source, ReadInt16(ref source));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? ReadNullableString(ref SequenceReader<byte> source)
        {
            return ReadNullableString(ref source, ReadInt16(ref source));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? ReadNullableString(ref SequenceReader<byte> source, int size)
        {
            return size < 0 ? null : ReadString(ref source, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(ref SequenceReader<byte> source, int size)
        {
            var value = Encoding.UTF8.GetString(source.CurrentSpan.Slice(source.CurrentSpanIndex, size));
            source.Advance(size);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? ReadCompactNullableString(ref SequenceReader<byte> source)
        {
            var size = ReadUVarint(ref source);

            return size <= 0 ?
                null :
                ReadString(ref source, size - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadCompactString(ref SequenceReader<byte> source)
        {
            return ReadString(ref source, ReadUVarint(ref source) - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ReadCompactByteArray(ref SequenceReader<byte> source)
        {
            var size = ReadUVarint(ref source);

            if (size <= 0)
                return null;

            return ReadBytes(ref source, size - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TMessage ReadMessage<TMessage>(ref SequenceReader<byte> source)
            where TMessage : class, IResponse, new()
        {
            var message = new TMessage();
            message.Read(ref source);
            return message;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TMessage[] ReadArray<TMessage>(ref SequenceReader<byte> source) where TMessage : class, IResponse, new() =>
            ReadArray<TMessage>(ref source, ReadInt32(ref source));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TMessage[] ReadCompactArray<TMessage>(ref SequenceReader<byte> source) where TMessage : class, IResponse, new() =>
            ReadArray<TMessage>(ref source, ReadUVarint(ref source) - 1);

        public static TMessage[] ReadArray<TMessage>(ref SequenceReader<byte> source, int count) where TMessage : class, IResponse, new()
        {
            if (count < 0)
                return null;

            if (count == 0)
                return Array.Empty<TMessage>();

            var result = new TMessage[count];

            for (var i = 0; i < count; i++)
            {
                result[i] = new TMessage();
                result[i].Read(ref source);
            }

            return result;
        }

        public static TaggedField[] ReadTaggedFields(ref SequenceReader<byte> source)
        {
            var count = ReadUVarint(ref source);

            if (count == 0)
                return Array.Empty<TaggedField>();

            var result = new TaggedField[count];

            for (var i = 0; i < count; i++)
            {
                result[i] = new TaggedField();
                result[i].Read(ref source);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] ReadInt32Array(ref SequenceReader<byte> source) => ReadInt32Array(ref source, ReadInt32(ref source));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] ReadCompactInt32Array(ref SequenceReader<byte> source) => ReadInt32Array(ref source, ReadUVarint(ref source) - 1);

        public static int[] ReadInt32Array(ref SequenceReader<byte> source, int count)
        {
            if (count < 0)
                return null;

            if (count == 0)
                return Array.Empty<int>();

            var result = new int[count];

            for (var i = 0; i < count; ++i)
            {
                result[i] = ReadInt32(ref source);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadVarint(ref SequenceReader<byte> source)
        {
            var num = ReadUVarint(ref source);

            return (num >> 1) ^ -(num & 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadUVarint(ref SequenceReader<byte> source) => ReadUVarint(ref source, out _);

        public static int ReadUVarint(ref SequenceReader<byte> source, out int bytesRead)
        {
            const int endMask = 0b1000_0000;
            const int valueMask = 0b0111_1111;

            bytesRead = 0;

            var num = 0;
            var shift = 0;
            int current;

            do
            {
                source.TryRead(out byte value);
                current = value;

                if (++bytesRead > 4)
                    throw new InvalidOperationException("The value is not a valid VARINT");

                num |= (current & valueMask) << shift;
                shift += 7;
            } while ((current & endMask) != 0);

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteVarint(this IBufferWriter<byte> destination, long num) =>
            destination.WriteUVarint(((ulong)num << 1) ^ ((ulong)num >> 63));

        public static void WriteUVarint(this IBufferWriter<byte> destination, ulong num)
        {
            const ulong endMask = 0b1000_0000;
            const ulong valueMask = 0b0111_1111;

            do
            {
                byte value = (byte)((num & valueMask) | (num > valueMask ? endMask : 0));
                MemoryMarshal.Write(destination.GetSpan(1), ref value); // CreateReadOnlySpan(ref value, 1);
                destination.Advance(1);
                num >>= 7;
            } while (num != 0);
        }
    }
}

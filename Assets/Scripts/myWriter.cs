using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public class myWriter
{
    private byte[] _buffer;
    private int _position;
    private bool _isDisposed;
    private int _capacity;

    // Sử dụng ArrayPool để quản lý bộ nhớ hiệu quả
    private static readonly ArrayPool<byte> s_arrayPool = ArrayPool<byte>.Shared;

    public myWriter(int initialCapacity = 2048)
    {
        _buffer = s_arrayPool.Rent(initialCapacity);
        _capacity = initialCapacity;
        _position = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeSByte(sbyte value)
    {
        EnsureCapacity(1);
        _buffer[_position++] = (byte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeByte(sbyte value) => writeSByte(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeByte(int value) => writeSByte((sbyte)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeChar(char value)
    {
        EnsureCapacity(2);
        _buffer[_position++] = 0;
        _buffer[_position++] = (byte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeUnsignedByte(byte value) => writeSByte((sbyte)value);

    public void writeUnsignedByte(byte[] value)
    {
        if (value == null) return;

        EnsureCapacity(value.Length);
        Buffer.BlockCopy(value, 0, _buffer, _position, value.Length);
        _position += value.Length;
    }

    public void writeSByte(sbyte[] value)
    {
        if (value == null) return;

        EnsureCapacity(value.Length);
        // Sử dụng BufferBlockCopy để sao chép hiệu quả
        Buffer.BlockCopy(value, 0, _buffer, _position, value.Length);
        _position += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeShort(short value)
    {
        EnsureCapacity(2);
        // Sử dụng BinaryPrimitives để ghi trực tiếp vào bộ nhớ
        BinaryPrimitives.WriteInt16BigEndian(_buffer.AsSpan(_position), value);
        _position += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeShort(int value) => writeShort((short)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeUnsignedShort(ushort value)
    {
        EnsureCapacity(2);
        BinaryPrimitives.WriteUInt16BigEndian(_buffer.AsSpan(_position), value);
        _position += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeInt(int value)
    {
        EnsureCapacity(4);
        BinaryPrimitives.WriteInt32BigEndian(_buffer.AsSpan(_position), value);
        _position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeLong(long value)
    {
        EnsureCapacity(8);
        BinaryPrimitives.WriteInt64BigEndian(_buffer.AsSpan(_position), value);
        _position += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeFloat(float value)
    {
        EnsureCapacity(4);
        // Convert float to int bits and write as big endian
        int intBits = BitConverter.SingleToInt32Bits(value);
        BinaryPrimitives.WriteInt32BigEndian(_buffer.AsSpan(_position), intBits);
        _position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeBoolean(bool value) => writeSByte((sbyte)(value ? 1 : 0));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void writeBool(bool value) => writeBoolean(value);

    public void writeString(string value)
    {
        if (value == null)
        {
            writeShort(0);
            return;
        }

        int charCount = value.Length;
        writeShort((short)charCount);

        if (charCount == 0) return;

        EnsureCapacity(charCount);

        // Tối ưu: ghi trực tiếp từng ký tự
        for (int i = 0; i < charCount; i++)
        {
            _buffer[_position++] = (byte)value[i];
        }
    }

    public void writeUTF(string value)
    {
        if (value == null)
        {
            writeShort(0);
            return;
        }

        // Đếm số byte cần thiết cho chuỗi UTF-8
        int maxByteCount = Encoding.UTF8.GetMaxByteCount(value.Length);

        // Sử dụng bộ nhớ tạm thời từ ArrayPool để tránh GC
        byte[] rentedArray = s_arrayPool.Rent(maxByteCount);

        try
        {
            int actualByteCount = Encoding.UTF8.GetBytes(value, 0, value.Length, rentedArray, 0);
            writeShort((short)actualByteCount);
            EnsureCapacity(actualByteCount);
            Buffer.BlockCopy(rentedArray, 0, _buffer, _position, actualByteCount);
            _position += actualByteCount;
        }
        finally
        {
            s_arrayPool.Return(rentedArray);
        }
    }

    public void write(ref sbyte[] data, int offset, int count)
    {
        if (data == null || count <= 0) return;

        EnsureCapacity(count);

        // Sao chép dữ liệu hiệu quả sử dụng Buffer.BlockCopy
        Buffer.BlockCopy(data, offset, _buffer, _position, count);
        _position += count;
    }

    public void write(ReadOnlySpan<sbyte> data, int offset, int count)
    {
        if (data.IsEmpty || count <= 0) return;

        EnsureCapacity(count);
        // Sử dụng Span bình thường, tránh dùng con trỏ unsafe
        ReadOnlySpan<sbyte> source = data.Slice(offset, count);
        // Chuyển đổi thành byte[] rồi sao chép
        for (int i = 0; i < count; i++)
        {
            _buffer[_position + i] = (byte)source[i];
        }
        _position += count;
    }

    public void write(sbyte[] value)
    {
        if (value == null) return;
        writeSByte(value);
    }

    public void write(byte[] value)
    {
        if (value == null) return;
        EnsureCapacity(value.Length);
        Buffer.BlockCopy(value, 0, _buffer, _position, value.Length);
        _position += value.Length;
    }

    public byte[] getData()
    {
        if (_position <= 0) return Array.Empty<byte>();

        byte[] result = new byte[_position];
        Buffer.BlockCopy(_buffer, 0, result, 0, _position);
        return result;
    }

    // Trả về dữ liệu dưới dạng sbyte[] để tương thích với code cũ
    public sbyte[] getSByteData()
    {
        if (_position <= 0) return Array.Empty<sbyte>();

        sbyte[] result = new sbyte[_position];
        Buffer.BlockCopy(_buffer, 0, result, 0, _position);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int additionalCapacity)
    {
        int requiredCapacity = _position + additionalCapacity;
        if (requiredCapacity > _capacity)
        {
            // Tính toán kích thước mới (gấp đôi hoặc đủ cho dữ liệu mới)
            int newSize = Math.Max(_capacity * 2, requiredCapacity);

            // Thuê một buffer mới từ pool
            byte[] newBuffer = s_arrayPool.Rent(newSize);

            // Sao chép dữ liệu từ buffer cũ
            Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _position);

            // Trả lại buffer cũ cho pool
            byte[] oldBuffer = _buffer;
            _buffer = newBuffer;
            _capacity = newSize;
            s_arrayPool.Return(oldBuffer);
        }
    }

    public void Dispose()
    {
        if (!_isDisposed && _buffer != null)
        {
            s_arrayPool.Return(_buffer);
            _buffer = null;
            _isDisposed = true;
            _capacity = 0;
        }
    }

    public void Close() => Dispose();

    public void WriteInts(int[] values)
    {
        if (values == null)
        {
            writeInt(-1); // Đánh dấu null
            return;
        }

        // Ghi độ dài mảng
        int length = values.Length;
        writeInt(length);

        // Tối ưu: Nếu đủ dung lượng, ghi toàn bộ mảng một lần
        if (length > 0)
        {
            EnsureCapacity(length * 4);
            var span = _buffer.AsSpan(_position);

            for (int i = 0; i < length; i++)
            {
                BinaryPrimitives.WriteInt32BigEndian(span.Slice(i * 4), values[i]);
            }

            _position += length * 4;
        }
    }

    /// <summary>
    /// Ghi một mảng long vào luồng đầu ra.
    /// </summary>
    /// <param name="values">Mảng long cần ghi</param>
    public void WriteLongs(long[] values)
    {
        if (values == null)
        {
            writeInt(-1); // Đánh dấu null
            return;
        }

        // Ghi độ dài mảng
        int length = values.Length;
        writeInt(length);

        // Tối ưu: Nếu đủ dung lượng, ghi toàn bộ mảng một lần
        if (length > 0)
        {
            EnsureCapacity(length * 8);
            var span = _buffer.AsSpan(_position);

            for (int i = 0; i < length; i++)
            {
                BinaryPrimitives.WriteInt64BigEndian(span.Slice(i * 8), values[i]);
            }

            _position += length * 8;
        }
    }

    /// <summary>
    /// Ghi một mảng chuỗi vào luồng đầu ra dưới dạng UTF-8.
    /// </summary>
    /// <param name="values">Mảng chuỗi cần ghi</param>
    public void WriteUTFs(string[] values)
    {
        if (values == null)
        {
            writeInt(-1); // Đánh dấu null
            return;
        }

        // Ghi độ dài mảng
        int length = values.Length;
        writeInt(length);

        // Ghi từng chuỗi
        for (int i = 0; i < length; i++)
        {
            string str = values[i];
            if (str == null)
            {
                writeShort(-1); // Đánh dấu chuỗi null
            }
            else
            {
                writeUTF(str);
            }
        }
    }

    /// <summary>
    /// Ghi một mảng boolean vào luồng đầu ra.
    /// </summary>
    /// <param name="values">Mảng boolean cần ghi</param>
    public void WriteBooleans(bool[] values)
    {
        if (values == null)
        {
            writeInt(-1); // Đánh dấu null
            return;
        }

        // Ghi độ dài mảng
        int length = values.Length;
        writeInt(length);

        // Nếu mảng nhỏ, ghi theo cách đơn giản
        if (length <= 32)
        {
            for (int i = 0; i < length; i++)
            {
                writeBool(values[i]);
            }
            return;
        }

        // Đối với mảng lớn, đóng gói nhiều giá trị boolean vào một byte
        int byteCount = (length + 7) / 8; // Số byte cần thiết
        EnsureCapacity(byteCount);

        // Tối ưu: sử dụng mảng tạm để đóng gói bit
        byte[] bitPacked = new byte[byteCount];
        for (int i = 0; i < length; i++)
        {
            if (values[i])
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                bitPacked[byteIndex] |= (byte)(1 << bitIndex);
            }
        }

        // Sao chép tất cả các byte đã đóng gói vào buffer
        Buffer.BlockCopy(bitPacked, 0, _buffer, _position, byteCount);
        _position += byteCount;
    }
}
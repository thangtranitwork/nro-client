using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class myReader
{
    public sbyte[] buffer;
    private int posRead;
    private int posMark;
    private static string fileName;
    private static int status;

    public myReader()
    {
    }

    public myReader(sbyte[] data)
    {
        buffer = data;
    }

    public myReader(string filename)
    {
        TextAsset textAsset = (TextAsset)Resources.Load(filename, typeof(TextAsset));
        buffer = mSystem.convertToSbyte(textAsset.bytes);
    }

    // Helper method để có được Span khi cần
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<byte> GetByteSpan()
    {
        return MemoryMarshal.Cast<sbyte, byte>(buffer.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte readSByte()
    {
        if (posRead < buffer.Length)
        {
            return buffer[posRead++];
        }
        posRead = buffer.Length;
        throw new Exception(" loi doc sbyte eof ");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte readsbyte()
    {
        return readSByte();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte readByte()
    {
        return readSByte();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void mark(int readlimit)
    {
        posMark = posRead;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void reset()
    {
        posRead = posMark;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte readUnsignedByte()
    {
        return convertSbyteToByte(readSByte());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short readShort()
    {
        // Kiểm tra xem có đủ dữ liệu không
        if (posRead + 2 > buffer.Length)
            throw new Exception("Không đủ dữ liệu để đọc short");

        // Sử dụng BinaryPrimitives cho hiệu suất cao
        short result = BinaryPrimitives.ReadInt16BigEndian(GetByteSpan().Slice(posRead));
        posRead += 2;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort readUnsignedShort()
    {
        // Kiểm tra xem có đủ dữ liệu không
        if (posRead + 2 > buffer.Length)
            throw new Exception("Không đủ dữ liệu để đọc ushort");

        // Sử dụng BinaryPrimitives cho hiệu suất cao
        ushort result = BinaryPrimitives.ReadUInt16BigEndian(GetByteSpan().Slice(posRead));
        posRead += 2;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int readInt()
    {
        // Kiểm tra xem có đủ dữ liệu không
        if (posRead + 4 > buffer.Length)
            throw new Exception("Không đủ dữ liệu để đọc int");

        // Sử dụng BinaryPrimitives cho hiệu suất cao
        int result = BinaryPrimitives.ReadInt32BigEndian(GetByteSpan().Slice(posRead));
        posRead += 4;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long readLong()
    {
        // Kiểm tra xem có đủ dữ liệu không
        if (posRead + 8 > buffer.Length)
            throw new Exception("Không đủ dữ liệu để đọc long");

        // Sử dụng BinaryPrimitives cho hiệu suất cao
        long result = BinaryPrimitives.ReadInt64BigEndian(GetByteSpan().Slice(posRead));
        posRead += 8;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float readFloat()
    {
        // Kiểm tra xem có đủ dữ liệu không
        if (posRead + 4 > buffer.Length)
            throw new Exception("Không đủ dữ liệu để đọc float");

        // Read as int and convert to float
        int intBits = BinaryPrimitives.ReadInt32BigEndian(GetByteSpan().Slice(posRead));
        posRead += 4;
        return BitConverter.Int32BitsToSingle(intBits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool readBool()
    {
        return readSByte() > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool readBoolean()
    {
        return readSByte() > 0;
    }

    public string readString()
    {
        short length = readShort();

        // Kiểm tra độ dài hợp lệ
        if (length < 0)
            throw new Exception("Độ dài chuỗi không hợp lệ");

        // Kiểm tra có đủ dữ liệu không
        if (posRead + length > buffer.Length)
            throw new Exception("Không đủ dữ liệu để đọc chuỗi");

        // Sử dụng Span để tránh cấp phát thêm bộ nhớ
        ReadOnlySpan<byte> stringBytes = GetByteSpan().Slice(posRead, length);
        posRead += length;

        // Sử dụng Encoding.UTF8.GetString với Span trực tiếp
        return Encoding.UTF8.GetString(stringBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string readStringUTF()
    {
        return readString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string readUTF()
    {
        return readStringUTF();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int read()
    {
        if (posRead < buffer.Length)
        {
            return readSByte();
        }
        return -1;
    }

    public int read(ref sbyte[] data)
    {
        if (data == null)
        {
            return 0;
        }

        // Tính toán số byte tối đa có thể đọc
        int bytesToRead = Math.Min(data.Length, buffer.Length - posRead);
        if (bytesToRead <= 0)
            return -1;

        // Sử dụng CopyTo hiệu suất cao với Span
        ReadOnlySpan<sbyte> source = buffer.AsSpan(posRead, bytesToRead);
        Span<sbyte> destination = data.AsSpan(0, bytesToRead);
        source.CopyTo(destination);

        posRead += bytesToRead;
        return bytesToRead;
    }

    public void readFully(ref sbyte[] data)
    {
        if (data == null)
            return;

        if (posRead + data.Length > buffer.Length)
            throw new Exception("Không đủ dữ liệu để đọc");

        // Sử dụng CopyTo hiệu suất cao với Span
        ReadOnlySpan<sbyte> source = buffer.AsSpan(posRead, data.Length);
        Span<sbyte> destination = data.AsSpan();
        source.CopyTo(destination);

        posRead += data.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int available()
    {
        return buffer.Length - posRead;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte convertSbyteToByte(sbyte var)
    {
        if (var > 0)
        {
            return (byte)var;
        }
        return (byte)(var + 256);
    }

    public static byte[] convertSbyteToByte(sbyte[] var)
    {
        if (var == null)
            return null;

        byte[] array = new byte[var.Length];

        // Sử dụng MemoryMarshal để tránh cấp phát bộ nhớ mới khi chuyển đổi
        ReadOnlySpan<sbyte> sbyteSpan = var.AsSpan();
        Span<byte> byteResult = array.AsSpan();

        // Chuyển đổi trực tiếp từng phần tử - không thể dùng MemoryMarshal.Copy vì phép chuyển đổi sbyte -> byte
        for (int i = 0; i < var.Length; i++)
        {
            if (var[i] > 0)
            {
                array[i] = (byte)var[i];
            }
            else
            {
                array[i] = (byte)(var[i] + 256);
            }
        }
        return array;
    }

    public void Close()
    {
        buffer = null;
    }

    public void close()
    {
        Close();
    }

    public void read(ref sbyte[] data, int offset, int length)
    {
        if (data == null)
            return;

        // Tính toán số byte tối đa có thể đọc
        int bytesToRead = Math.Min(length, buffer.Length - posRead);
        if (bytesToRead <= 0)
            return;

        // Sử dụng CopyTo hiệu suất cao với Span
        ReadOnlySpan<sbyte> source = buffer.AsSpan(posRead, bytesToRead);
        Span<sbyte> destination = data.AsSpan(offset, bytesToRead);
        source.CopyTo(destination);

        posRead += bytesToRead;
    }


    public int[] readInts()
    {
        // Đọc độ dài mảng
        int length = readInt();

        // Kiểm tra mảng null
        if (length == -1)
        {
            return null;
        }

        //// Kiểm tra độ dài hợp lệ
        //if (length < 0 || length > 1000000) // Giới hạn hợp lý
        //{
        //    throw new Exception("Invalid array length: " + length);
        //}

        int[] result = new int[length];

        // Tối ưu cho trường hợp đặc biệt khi dữ liệu đủ và liền mạch trong bộ đệm
        if (posRead + length * 4 <= buffer.Length)
        {
            ReadOnlySpan<byte> byteSpan = GetByteSpan().Slice(posRead);
            for (int i = 0; i < length; i++)
            {
                result[i] = BinaryPrimitives.ReadInt32BigEndian(byteSpan.Slice(i * 4));
            }
            posRead += length * 4;
        }
        else
        {
            // Fallback vào cách đọc từng phần tử
            for (int i = 0; i < length; i++)
            {
                result[i] = readInt();
            }
        }

        return result;
    }

    /// <summary>
    /// Đọc một mảng long từ luồng đầu vào.
    /// </summary>
    /// <returns>Mảng long đã đọc, hoặc null nếu mảng là null</returns>
    public long[] readLongs()
    {
        // Đọc độ dài mảng
        int length = readInt();

        // Kiểm tra mảng null
        if (length == -1)
        {
            return null;
        }

        //// Kiểm tra độ dài hợp lệ
        //if (length < 0 || length > 1000000) // Giới hạn hợp lý
        //{
        //    throw new Exception("Invalid array length: " + length);
        //}

        long[] result = new long[length];

        // Tối ưu cho trường hợp đặc biệt khi dữ liệu đủ và liền mạch trong bộ đệm
        if (posRead + length * 8 <= buffer.Length)
        {
            ReadOnlySpan<byte> byteSpan = GetByteSpan().Slice(posRead);
            for (int i = 0; i < length; i++)
            {
                result[i] = BinaryPrimitives.ReadInt64BigEndian(byteSpan.Slice(i * 8));
            }
            posRead += length * 8;
        }
        else
        {
            // Fallback vào cách đọc từng phần tử
            for (int i = 0; i < length; i++)
            {
                result[i] = readLong();
            }
        }

        return result;
    }

    /// <summary>
    /// Đọc một mảng chuỗi từ luồng đầu vào.
    /// </summary>
    /// <returns>Mảng chuỗi đã đọc, hoặc null nếu mảng là null</returns>
    public string[] readUTFs()
    {
        // Đọc độ dài mảng
        int length = readInt();

        // Kiểm tra mảng null
        if (length == -1)
        {
            return null;
        }

        //// Kiểm tra độ dài hợp lệ
        //if (length < 0 || length > 1000000) // Giới hạn hợp lý
        //{
        //    throw new Exception("Invalid array length: " + length);
        //}

        string[] result = new string[length];

        // Đọc từng chuỗi
        for (int i = 0; i < length; i++)
        {
            short strLength = readShort();
            if (strLength == -1)
            {
                result[i] = null; // Chuỗi null
            }
            else
            {
                // Quay lại một bước để đọc chính xác chuỗi
                // vì readUTF() sẽ đọc độ dài chuỗi
                posRead -= 2;
                result[i] = readUTF();
            }
        }

        return result;
    }

    /// <summary>
    /// Đọc một mảng boolean từ luồng đầu vào.
    /// </summary>
    /// <returns>Mảng boolean đã đọc, hoặc null nếu mảng là null</returns>
    public bool[] readBooleans()
    {
        // Đọc độ dài mảng
        int length = readInt();

        // Kiểm tra mảng null
        if (length == -1)
        {
            return null;
        }

        // Kiểm tra độ dài hợp lệ
        //if (length < 0 || length > 1000000) // Giới hạn hợp lý
        //{
        //    throw new Exception("Invalid array length: " + length);
        //}

        bool[] result = new bool[length];

        // Với mảng nhỏ, đọc theo cách đơn giản
        if (length <= 32)
        {
            for (int i = 0; i < length; i++)
            {
                result[i] = readBool();
            }
            return result;
        }

        // Với mảng lớn, giải nén các bit
        int byteCount = (length + 7) / 8;
        int bitPosition = 0;
        sbyte currentByte = 0;

        for (int i = 0; i < length; i++)
        {
            if (bitPosition == 0)
            {
                currentByte = readSByte();
            }

            result[i] = ((currentByte >> bitPosition) & 1) == 1;

            bitPosition = (bitPosition + 1) % 8;
        }

        return result;
    }
}
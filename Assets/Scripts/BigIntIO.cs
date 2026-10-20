using System;
using System.Numerics;

public static class BigIntIO
{
    public static BigInteger ReadBigInt(myReader r)
    {
        int len = r.readInt();
        if (len <= 0)
            return BigInteger.Zero;
        sbyte[] sb = new sbyte[len];
        r.read(ref sb);
        byte[] be = new byte[len + 1];
        for (int i = 0; i < len; i++)
        {
            be[i] = myReader.convertSbyteToByte(sb[len - 1 - i]);
        }
        // BigInteger expects little endian representation
        return new BigInteger(be);
    }

    public static void WriteBigInt(myWriter w, BigInteger value)
    {
        // Java's BigInteger.toByteArray returns big-endian two's complement.
        // C# BigInteger uses little-endian, so reverse without trimming.
        byte[] little = value.ToByteArray();
        byte[] be = new byte[little.Length];
        for (int i = 0; i < little.Length; i++)
        {
            be[i] = little[little.Length - 1 - i];
        }
        w.writeInt(be.Length);
        sbyte[] sb = new sbyte[be.Length];
        for (int i = 0; i < be.Length; i++)
        {
            sb[i] = (sbyte)be[i];
        }
        w.write(sb);
    }
}


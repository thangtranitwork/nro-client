using System.IO;
using System.IO.Compression;

public static class CompressionUtils
{
    public static sbyte[] Decompress(sbyte[] data)
    {
        if (data == null || data.Length == 0)
        {
            return data;
        }
        using (MemoryStream input = new MemoryStream(ArrayCast.cast(data)))
        using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
        using (MemoryStream output = new MemoryStream())
        {
            gzip.CopyTo(output);
            return ArrayCast.cast(output.ToArray());
        }
    }
}

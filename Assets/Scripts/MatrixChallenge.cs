using System;
using System.IO;
using UnityEngine;
using Random = System.Random;

public static class MatrixChallenge
{
    public const int SIZE = 5;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    public const ulong MOD = 1247892793UL;
#else
    public const ulong MOD = 4294967291UL;
#endif
    private static readonly ulong[][] DEFAULT_SECRET = GenerateDefaultSecret();
    private static string logFilePath = "matrix_debug.txt";

    public static void LogToFile(string message)
    {
        //try
        //{
        //    File.AppendAllText(logFilePath, message + "\n");
        //    Debug.LogError(message);
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError($"Failed to write to log file: {e.Message}");
        //}
    }

    // Custom Random implementation matching Java's algorithm exactly
    private class JavaRandom
    {
        private long seed;

        public JavaRandom(long seed)
        {
            this.seed = seed & ((1L << 48) - 1);
            LogToFile($"JavaRandom initialized with seed: {seed:X}, internal seed: {this.seed:X}");
        }

        public int NextInt()
        {
            long oldSeed = seed;
            seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
            int result = (int)((seed >> 16) & 0xFFFFFFFFL);
            LogToFile($"NextInt: {oldSeed:X} -> {seed:X} -> {result}");
            return result;
        }
    }

    private static ulong[][] GenerateDefaultSecret()
    {
        LogToFile("=== Generating C# DEFAULT_SECRET ===");
        var rnd = new JavaRandom(0x5eedfaceL);
        var secret = new ulong[SIZE][];
        for (int i = 0; i < SIZE; i++)
        {
            secret[i] = new ulong[SIZE];
            for (int j = 0; j < SIZE; j++)
            {
                int hiInt = rnd.NextInt();
                int loInt = rnd.NextInt();
                ulong hi = (ulong)((uint)hiInt);
                ulong lo = (ulong)((uint)loInt);
                ulong val = (hi << 32) | lo;
                secret[i][j] = val % MOD;
                LogToFile($"SECRET[{i}][{j}]: hiInt={hiInt}, loInt={loInt}, hi={hi}, lo={lo}, val={val}, final={secret[i][j]}");
            }
        }
        LogToFile("=== C# DEFAULT_SECRET Generation Complete ===");
        return secret;
    }

    public static ulong[][] DefaultSecret()
    {
        var copy = new ulong[SIZE][];
        for (int i = 0; i < SIZE; i++)
        {
            copy[i] = new ulong[SIZE];
            Array.Copy(DEFAULT_SECRET[i], copy[i], SIZE);
        }
        return copy;
    }

    public static ulong[][] RandomMatrix()
    {
        var rnd = new Random();
        var m = new ulong[SIZE][];
        for (int i = 0; i < SIZE; i++)
        {
            m[i] = new ulong[SIZE];
            for (int j = 0; j < SIZE; j++)
            {
                byte[] bytes = new byte[4];
                rnd.NextBytes(bytes);
                ulong val = BitConverter.ToUInt32(bytes, 0);
                m[i][j] = val % MOD;
            }
        }
        return m;
    }

    public static ulong[][] Multiply(ulong[][] a, ulong[][] b)
    {
        LogToFile("=== C# Matrix Multiplication Start ===");
        var r = new ulong[SIZE][];
        for (int i = 0; i < SIZE; i++)
        {
            r[i] = new ulong[SIZE];
            for (int k = 0; k < SIZE; k++)
            {
                ulong aik = a[i][k];
                for (int j = 0; j < SIZE; j++)
                {
                    System.Numerics.BigInteger temp =
                        (System.Numerics.BigInteger)r[i][j] +
                        ((System.Numerics.BigInteger)aik * (System.Numerics.BigInteger)b[k][j]) % MOD;
                    ulong newVal = (ulong)(temp % MOD);

                    if (i < 2 && k < 2 && j < 2)
                    {
                        LogToFile($"r[{i}][{j}] += a[{i}][{k}] * b[{k}][{j}] = {r[i][j]} + ({aik} * {b[k][j]} % {MOD}) = {newVal}");
                    }

                    r[i][j] = newVal;
                }
            }
        }
        LogToFile("=== C# Matrix Multiplication End ===");
        return r;
    }

    public static ulong[][] ComputeResponse(ulong[][] secret, ulong[][] challenge)
    {
        LogToFile("=== C# Computing Response: S * C * S ===");

        LogToFile("Complete C# Secret matrix:");
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                LogToFile($"S[{i}][{j}] = {secret[i][j]}");
            }
        }

        LogToFile("Complete C# Challenge matrix:");
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                LogToFile($"C[{i}][{j}] = {challenge[i][j]}");
            }
        }

        var sc = Multiply(secret, challenge);
        LogToFile("C# S * C completed - Full result:");
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                LogToFile($"SC[{i}][{j}] = {sc[i][j]}");
            }
        }

        var result = Multiply(sc, secret);
        LogToFile("C# (S * C) * S completed - Full result:");
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                LogToFile($"Final[{i}][{j}] = {result[i][j]}");
            }
        }

        return result;
    }

    public static void ClearLogFile()
    {
        try
        {
            File.WriteAllText(logFilePath, "=== Matrix Challenge C# Debug Log ===\n");
            LogToFile($"Log file created at: {logFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create log file: {e.Message}");
        }
    }

    public static void PrintDefaultSecret()
    {
        LogToFile("=== C# DEFAULT_SECRET ===");
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                LogToFile($"[{i}][{j}] = {DEFAULT_SECRET[i][j]}");
            }
        }
        LogToFile("=== End C# DEFAULT_SECRET ===");
    }

    public static ulong[][] ProcessChallenge(ulong[][] challenge)
    {
        LogToFile("=== C# Processing Challenge ===");

        LogToFile("Received Challenge from Server:");
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                LogToFile($"ReceivedChallenge[{i}][{j}] = {challenge[i][j]}");
            }
        }

        ulong[][] secret = DefaultSecret();
        ulong[][] response = ComputeResponse(secret, challenge);

        LogToFile("Response to Send:");
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                LogToFile($"ResponseToSend[{i}][{j}] = {response[i][j]}");
            }
        }

        LogToFile("=== C# Challenge Processing Complete ===");
        return response;
    }
}
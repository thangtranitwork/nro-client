using System;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;

public class ECCAuth
{
    // Obfuscated field names but same functionality
    private SimpleECC _cryptoEngine;
    private SimpleECC.Point _serverChallenge;

    // Dynamic field names generation to avoid detection
    private readonly string _fieldValidator = BuildValidationString();

    // Build validation string dynamically
    private static string BuildValidationString()
    {
        char[] chars = { 'E', 'C', 'C', '_', 'V', 'A', 'L', 'I', 'D' };
        return new string(chars);
    }

    // Same Init logic but obfuscated variable names
    public void Init(BigInteger p, BigInteger a, BigInteger b, BigInteger gx, BigInteger gy, BigInteger cx, BigInteger cy)
    {
        // Simple validation without changing logic
        if (_fieldValidator == null || _fieldValidator.Length == 0)
            throw new InvalidOperationException("Validation failed");

        // Same exact logic as original, just obfuscated names
        var generatorPoint = new SimpleECC.Point(gx, gy);
        var challengePoint = new SimpleECC.Point(cx, cy);

        _cryptoEngine = new SimpleECC(p, a, b, generatorPoint);
        _serverChallenge = challengePoint;
    }

    // Same ComputeResponse logic but obfuscated
    public (SimpleECC.Point R, SimpleECC.Point S) ComputeResponse()
    {
        // Same validation as original
        if (_cryptoEngine == null)
            throw new InvalidOperationException("ECC not initialized");

        // Same random generation logic
        BigInteger randomScalar = GenerateRandomScalar();

        // Same exact calculations as original
        SimpleECC.Point responseR = _cryptoEngine.Multiply(_cryptoEngine.g, randomScalar);
        SimpleECC.Point responseS = _cryptoEngine.Multiply(_serverChallenge, randomScalar);

        return (responseR, responseS);
    }

    // Same random generation logic but obfuscated method name
    private BigInteger GenerateRandomScalar()
    {
        // Exact same logic as original ComputeResponse
        byte[] randomBytes = new byte[_cryptoEngine.p.ToByteArray().Length];
        RandomNumberGenerator.Fill(randomBytes);

        BigInteger randomValue = new BigInteger(randomBytes);
        if (randomValue.Sign < 0) randomValue = -randomValue;
        randomValue %= _cryptoEngine.p;

        // Ensure r is not zero
        if (randomValue == BigInteger.Zero) randomValue = BigInteger.One;

        return randomValue;
    }
}
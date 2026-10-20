using System;
using System.Numerics;
using System.Collections.Generic;

/// <summary>
/// Challenge từ server
/// </summary>
[Serializable]
public class MultiLayerChallenge
{
    public string challengeId;
    public long timestamp;
    public VRFChallenge vrfChallenge;
    public CommitmentChallenge commitmentChallenge;
    public ECCChallenge eccChallenge;
    public PairingChallenge pairingChallenge;
    public ZKChallenge zkChallenge;
    
    // Response fields (sẽ được điền sau khi xử lý)
    public VRFResponse vrfResponse;
    public CommitmentResponse commitmentResponse;
    public ECCResponse eccResponse;
    public PairingResponse pairingResponse;
    public ZKResponse zkResponse;
}

/// <summary>
/// Response cuối cùng gửi về server
/// </summary>
[Serializable]
public class MultiLayerResponse
{
    public string challengeId;
    public VRFResponse vrfResponse;
    public CommitmentResponse commitmentResponse;
    public ECCResponse eccResponse;
    public PairingResponse pairingResponse;
    public ZKResponse zkResponse;
    public long totalProcessingTime;
    public Dictionary<string, long> layerTimings;
}

// ==================== CHALLENGE CLASSES ====================

[Serializable]
public class VRFChallenge
{
    public string p;  // Prime modulus
    public string q;  // (p-1)/2
    public string g;  // Generator
    public string y;  // g^x mod p
    public string x;  // Secret challenge
    
    public BigInteger P => BigInteger.Parse(p);
    public BigInteger Q => BigInteger.Parse(q);
    public BigInteger G => BigInteger.Parse(g);
    public BigInteger Y => BigInteger.Parse(y);
    public BigInteger X => BigInteger.Parse(x);
}

[Serializable]
public class CommitmentChallenge
{
    public string p;  // Prime modulus
    public string q;  // (p-1)/2
    public string g;  // Generator
    public string[] challenges;  // Array of challenges
    public string[] commitments; // Array of commitments
    
    public BigInteger P => BigInteger.Parse(p);
    public BigInteger Q => BigInteger.Parse(q);
    public BigInteger G => BigInteger.Parse(g);
    public BigInteger[] Challenges => Array.ConvertAll(challenges, BigInteger.Parse);
    public BigInteger[] Commitments => Array.ConvertAll(commitments, BigInteger.Parse);
}

[Serializable]
public class ECCChallenge
{
    public string p;  // Prime modulus
    public string a;  // Curve parameter a
    public string b;  // Curve parameter b
    public string gx; // Generator point x
    public string gy; // Generator point y
    public string[] scalars; // Array of scalars
    public ECCPoint[] points;    // Array of points
    
    public BigInteger P => BigInteger.Parse(p);
    public BigInteger A => BigInteger.Parse(a);
    public BigInteger B => BigInteger.Parse(b);
    public BigInteger GX => BigInteger.Parse(gx);
    public BigInteger GY => BigInteger.Parse(gy);
    public BigInteger[] Scalars => Array.ConvertAll(scalars, BigInteger.Parse);
}

[Serializable]
public class PairingChallenge
{
    public string[] group1Elements; // Elements from group G1
    public string[] group2Elements; // Elements from group G2
    
    public BigInteger[] Group1Elements => Array.ConvertAll(group1Elements, BigInteger.Parse);
    public BigInteger[] Group2Elements => Array.ConvertAll(group2Elements, BigInteger.Parse);
}

[Serializable]
public class ZKChallenge
{
    public string p;  // Prime modulus
    public string q;  // (p-1)/2
    public string g;  // Generator 1
    public string h;  // Generator 2
    public string commitment; // g^secret mod p
    public string secret;     // Secret value
    
    public BigInteger P => BigInteger.Parse(p);
    public BigInteger Q => BigInteger.Parse(q);
    public BigInteger G => BigInteger.Parse(g);
    public BigInteger H => BigInteger.Parse(h);
    public BigInteger Commitment => BigInteger.Parse(commitment);
    public BigInteger Secret => BigInteger.Parse(secret);
}

// ==================== RESPONSE CLASSES ====================

[Serializable]
public class VRFResponse
{
    public string proof; // Proof value
    
    public BigInteger Proof => BigInteger.Parse(proof);
}

[Serializable]
public class CommitmentResponse
{
    public string[] proofs; // Array of proof values
    
    public BigInteger[] Proofs => Array.ConvertAll(proofs, BigInteger.Parse);
}

[Serializable]
public class ECCResponse
{
    public ECCPoint[] results; // Array of ECC operation results
}

[Serializable]
public class PairingResponse
{
    public string pairingResult; // Result of pairing computation
    
    public BigInteger PairingResult => BigInteger.Parse(pairingResult);
}

[Serializable]
public class ZKResponse
{
    public string proof;    // Proof value
    public string challenge; // Challenge value
    
    public BigInteger Proof => BigInteger.Parse(proof);
    public BigInteger Challenge => BigInteger.Parse(challenge);
}

// ==================== UTILITY CLASSES ====================

[Serializable]
public class ECCPoint
{
    public string x;
    public string y;
    
    public BigInteger X => BigInteger.Parse(x);
    public BigInteger Y => BigInteger.Parse(y);
    
    public ECCPoint() { }
    
    public ECCPoint(BigInteger x, BigInteger y)
    {
        this.x = x.ToString();
        this.y = y.ToString();
    }
}

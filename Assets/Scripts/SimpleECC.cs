using System;
using System.Numerics;
using System.Security.Cryptography;
using System.IO;
using UnityEngine;

public static class FileLogger
{
    // Obfuscated static fields
    private static string _logPath;
    private static bool _initState = false;
    private static readonly string _logPrefix = GenerateLogPrefix();

    // Dynamic log prefix generation
    private static string GenerateLogPrefix()
    {
        char[] prefix = { 'E', 'C', 'C', '_', 'D', 'e', 'b', 'u', 'g' };
        return new string(prefix);
    }

    public static void Initialize()
    {
        if (!_initState)
        {
            // Same logic, obfuscated variable names
            string fileIdentifier = BuildFileName();
            _logPath = Path.Combine(Application.persistentDataPath, fileIdentifier);

            // Same file operations
            File.WriteAllText(_logPath, $"{_logPrefix} Log - {DateTime.Now}\n");
            File.AppendAllText(_logPath, "=".PadRight(50, '=') + "\n\n");

            Debug.Log($"{_logPrefix} log file: {_logPath}");
            _initState = true;
        }
    }

    // Obfuscated filename generation
    private static string BuildFileName()
    {
        return $"ecc_debug_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
    }

    public static void Log(string message)
    {
        if (!_initState) Initialize();

        // Same exact logging logic
        string timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n";
        File.AppendAllText(_logPath, timestampedMessage);

        // Same console logging condition
        if (message.Contains("==="))
        {
            Debug.Log(message);
        }
    }

    public static void LogError(string message)
    {
        Log($"ERROR: {message}");
        Debug.LogError(message);
    }

    public static string GetLogPath()
    {
        return _logPath;
    }
}

public class SimpleECC
{
    public class Point
    {
        // Same fields, same logic
        public readonly BigInteger x;
        public readonly BigInteger y;
        public readonly bool infinity;

        public Point()
        {
            this.infinity = true;
            this.x = BigInteger.Zero;
            this.y = BigInteger.Zero;
        }

        public Point(BigInteger x, BigInteger y)
        {
            this.x = x;
            this.y = y;
            this.infinity = false;
        }
    }

    // Obfuscated field names but same functionality
    public readonly BigInteger p;
    public readonly BigInteger a;
    public readonly BigInteger b;
    public readonly Point g;

    // Dynamic validation constants
    private readonly BigInteger _zeroValue = GetZeroValue();
    private readonly BigInteger _oneValue = GetOneValue();
    private readonly BigInteger _twoValue = GetTwoValue();

    // Dynamic constant generation (but return same values)
    private static BigInteger GetZeroValue()
    {
        return BigInteger.Zero ^ BigInteger.Zero; // Still zero
    }

    private static BigInteger GetOneValue()
    {
        return BigInteger.One * BigInteger.One; // Still one
    }

    private static BigInteger GetTwoValue()
    {
        return BigInteger.One + BigInteger.One; // Still two
    }

    public SimpleECC(BigInteger p, BigInteger a, BigInteger b, Point g)
    {
        // Exact same initialization
        this.p = p;
        this.a = a;
        this.b = b;
        this.g = g;
    }

    public bool IsOnCurve(Point P)
    {
        // Same exact logic, just obfuscated variable names
        if (P.infinity) return true;

        BigInteger leftSide = (P.y * P.y) % p;
        BigInteger rightSide = (P.x * P.x * P.x + a * P.x + b) % p;

        if (rightSide.Sign < 0) rightSide += p;

        return leftSide.Equals((rightSide + p) % p);
    }

    private BigInteger ModInverse(BigInteger x)
    {
        BigInteger normalizedInput = NormalizeInput(x);
        return ProcessModularInverse(normalizedInput);
    }

    private BigInteger NormalizeInput(BigInteger input)
    {
        BigInteger step1 = ComputeModuloP(input);
        BigInteger step2 = AddPToResult(step1);
        return ComputeFinalModuloP(step2);
    }

    private BigInteger ComputeModuloP(BigInteger value)
    {
        return value % p;
    }

    private BigInteger AddPToResult(BigInteger value)
    {
        return value + p;
    }

    private BigInteger ComputeFinalModuloP(BigInteger value)
    {
        return value % p;
    }

    private BigInteger ProcessModularInverse(BigInteger normalizedValue)
    {
        if (CheckZeroCondition(normalizedValue))
        {
            return GetZeroResult();
        }
        return CalculateFermatInverse(normalizedValue);
    }

    private bool CheckZeroCondition(BigInteger value)
    {
        return CompareWithZero(value);
    }

    private bool CompareWithZero(BigInteger value)
    {
        return value == _zeroValue;
    }

    private BigInteger GetZeroResult()
    {
        return _zeroValue;
    }

    private BigInteger CalculateFermatInverse(BigInteger value)
    {
        BigInteger exponent = ComputeFermatExponent();
        return ExecuteModularPow(value, exponent);
    }

    private BigInteger ComputeFermatExponent()
    {
        return SubstractFromP(_twoValue);
    }

    private BigInteger SubstractFromP(BigInteger value)
    {
        return p - value;
    }

    private BigInteger ExecuteModularPow(BigInteger baseValue, BigInteger exponent)
    {
        return BigInteger.ModPow(baseValue, exponent, p);
    }

    public Point Add(Point P, Point Q)
    {
        if (HandleInfinityPoints(P, Q, out Point result))
        {
            return result;
        }

        return ProcessPointAddition(P, Q);
    }

    private bool HandleInfinityPoints(Point P, Point Q, out Point result)
    {
        if (CheckFirstPointInfinity(P))
        {
            result = ReturnSecondPoint(Q);
            return true;
        }
        if (CheckSecondPointInfinity(Q))
        {
            result = ReturnFirstPoint(P);
            return true;
        }
        result = GetNullPoint();
        return false;
    }

    private bool CheckFirstPointInfinity(Point P)
    {
        return P.infinity;
    }

    private bool CheckSecondPointInfinity(Point Q)
    {
        return Q.infinity;
    }

    private Point ReturnFirstPoint(Point P)
    {
        return P;
    }

    private Point ReturnSecondPoint(Point Q)
    {
        return Q;
    }

    private Point GetNullPoint()
    {
        return null;
    }

    private Point ProcessPointAddition(Point P, Point Q)
    {
        if (CheckSameXCoordinate(P, Q))
        {
            return HandleSameXCoordinate(P, Q);
        }
        else
        {
            return HandleDifferentXCoordinate(P, Q);
        }
    }

    private bool CheckSameXCoordinate(Point P, Point Q)
    {
        return CompareXCoordinates(P.x, Q.x);
    }

    private bool CompareXCoordinates(BigInteger x1, BigInteger x2)
    {
        return x1.Equals(x2);
    }

    private Point HandleSameXCoordinate(Point P, Point Q)
    {
        if (CheckOppositeYCoordinates(P, Q))
        {
            return CreateInfinityPoint();
        }
        return PerformPointDoubling(P);
    }

    private bool CheckOppositeYCoordinates(Point P, Point Q)
    {
        BigInteger sum = AddYCoordinates(P.y, Q.y);
        BigInteger modResult = ComputeModuloP(sum);
        return CompareWithZero(modResult);
    }

    private BigInteger AddYCoordinates(BigInteger y1, BigInteger y2)
    {
        return y1 + y2;
    }

    private Point CreateInfinityPoint()
    {
        return new Point();
    }

    private Point PerformPointDoubling(Point P)
    {
        BigInteger slope = CalculateDoublingSlope(P);
        return ComputeResultFromSlope(P, P, slope);
    }

    private BigInteger CalculateDoublingSlope(Point P)
    {
        BigInteger numerator = ComputeDoublingNumerator(P);
        BigInteger denominator = ComputeDoublingDenominator(P);
        return MultiplyWithInverse(numerator, denominator);
    }

    private BigInteger ComputeDoublingNumerator(Point P)
    {
        BigInteger xSquared = MultiplyValues(P.x, P.x);
        BigInteger threeXSquared = MultiplyByThree(xSquared);
        BigInteger result = AddConstantA(threeXSquared);
        return ComputeModuloP(result);
    }

    private BigInteger MultiplyValues(BigInteger a, BigInteger b)
    {
        return a * b;
    }

    private BigInteger MultiplyByThree(BigInteger value)
    {
        return 3 * value;
    }

    private BigInteger AddConstantA(BigInteger value)
    {
        return value + a;
    }

    private BigInteger ComputeDoublingDenominator(Point P)
    {
        BigInteger doubledY = MultiplyByTwo(P.y);
        return ComputeModuloP(doubledY);
    }

    private BigInteger MultiplyByTwo(BigInteger value)
    {
        return _twoValue * value;
    }

    private BigInteger MultiplyWithInverse(BigInteger numerator, BigInteger denominator)
    {
        BigInteger inverse = ModInverse(denominator);
        BigInteger product = MultiplyValues(numerator, inverse);
        return ComputeModuloP(product);
    }

    private Point HandleDifferentXCoordinate(Point P, Point Q)
    {
        BigInteger slope = CalculateAdditionSlope(P, Q);
        return ComputeResultFromSlope(P, Q, slope);
    }

    private BigInteger CalculateAdditionSlope(Point P, Point Q)
    {
        BigInteger numerator = ComputeAdditionNumerator(P, Q);
        BigInteger denominator = ComputeAdditionDenominator(P, Q);
        return MultiplyWithInverse(numerator, denominator);
    }

    private BigInteger ComputeAdditionNumerator(Point P, Point Q)
    {
        return SubtractValues(Q.y, P.y);
    }

    private BigInteger ComputeAdditionDenominator(Point P, Point Q)
    {
        return SubtractValues(Q.x, P.x);
    }

    private BigInteger SubtractValues(BigInteger a, BigInteger b)
    {
        return a - b;
    }

    private Point ComputeResultFromSlope(Point P, Point Q, BigInteger slope)
    {
        BigInteger resultX = CalculateResultX(P, Q, slope);
        BigInteger resultY = CalculateResultY(P, slope, resultX);
        return CreateNormalizedPoint(resultX, resultY);
    }

    private BigInteger CalculateResultX(Point P, Point Q, BigInteger slope)
    {
        if (CheckPointsEqual(P, Q))
        {
            return ComputeDoublingResultX(P, slope);
        }
        else
        {
            return ComputeAdditionResultX(P, Q, slope);
        }
    }

    private bool CheckPointsEqual(Point P, Point Q)
    {
        return CompareXCoordinates(P.x, Q.x);
    }

    private BigInteger ComputeDoublingResultX(Point P, BigInteger slope)
    {
        BigInteger slopeSquared = MultiplyValues(slope, slope);
        BigInteger doubledX = MultiplyByTwo(P.x);
        BigInteger result = SubtractValues(slopeSquared, doubledX);
        return ComputeModuloP(result);
    }

    private BigInteger ComputeAdditionResultX(Point P, Point Q, BigInteger slope)
    {
        BigInteger slopeSquared = MultiplyValues(slope, slope);
        BigInteger temp = SubtractValues(slopeSquared, P.x);
        BigInteger result = SubtractValues(temp, Q.x);
        return ComputeModuloP(result);
    }

    private BigInteger CalculateResultY(Point P, BigInteger slope, BigInteger resultX)
    {
        BigInteger xDifference = SubtractValues(P.x, resultX);
        BigInteger product = MultiplyValues(slope, xDifference);
        BigInteger result = SubtractValues(product, P.y);
        return ComputeModuloP(result);
    }

    private Point CreateNormalizedPoint(BigInteger x, BigInteger y)
    {
        BigInteger normalizedX = NormalizeCoordinate(x);
        BigInteger normalizedY = NormalizeCoordinate(y);
        return ConstructPoint(normalizedX, normalizedY);
    }

    private BigInteger NormalizeCoordinate(BigInteger coordinate)
    {
        BigInteger temp = AddPToResult(coordinate);
        return ComputeModuloP(temp);
    }

    private Point ConstructPoint(BigInteger x, BigInteger y)
    {
        return new Point(x, y);
    }

    public Point Multiply(Point P, BigInteger k)
    {
        Point accumulator = InitializeAccumulator();
        Point currentPoint = InitializeCurrentPoint(P);
        BigInteger scalar = InitializeScalar(k);

        return ProcessScalarMultiplication(accumulator, currentPoint, scalar);
    }

    private Point InitializeAccumulator()
    {
        return CreateInfinityPoint();
    }

    private Point InitializeCurrentPoint(Point P)
    {
        return CopyPoint(P);
    }

    private Point CopyPoint(Point P)
    {
        return P;
    }

    private BigInteger InitializeScalar(BigInteger k)
    {
        return CopyScalar(k);
    }

    private BigInteger CopyScalar(BigInteger k)
    {
        return k;
    }

    private Point ProcessScalarMultiplication(Point accumulator, Point currentPoint, BigInteger scalar)
    {
        while (CheckScalarCondition(scalar))
        {
            accumulator = ProcessSingleIteration(accumulator, currentPoint, scalar);
            currentPoint = DoubleCurrentPoint(currentPoint);
            scalar = ShiftScalarRight(scalar);
        }
        return GetFinalResult(accumulator);
    }

    private bool CheckScalarCondition(BigInteger scalar)
    {
        return CompareScalarWithZero(scalar);
    }

    private bool CompareScalarWithZero(BigInteger scalar)
    {
        return scalar > _zeroValue;
    }

    private Point ProcessSingleIteration(Point accumulator, Point currentPoint, BigInteger scalar)
    {
        if (ShouldAddCurrentPoint(scalar))
        {
            return AddToAccumulator(accumulator, currentPoint);
        }
        return ReturnUnchangedAccumulator(accumulator);
    }

    private bool ShouldAddCurrentPoint(BigInteger scalar)
    {
        return IsScalarBitSet(scalar);
    }

    private bool IsScalarBitSet(BigInteger scalar)
    {
        return CheckOddScalar(scalar);
    }

    private bool CheckOddScalar(BigInteger scalar)
    {
        return !scalar.IsEven;
    }

    private Point AddToAccumulator(Point accumulator, Point currentPoint)
    {
        return Add(accumulator, currentPoint);
    }

    private Point ReturnUnchangedAccumulator(Point accumulator)
    {
        return accumulator;
    }

    private Point DoubleCurrentPoint(Point currentPoint)
    {
        return PerformPointDoubling(currentPoint);
    }

    private BigInteger ShiftScalarRight(BigInteger scalar)
    {
        return PerformRightShift(scalar);
    }

    private BigInteger PerformRightShift(BigInteger value)
    {
        return value >> 1;
    }

    private Point GetFinalResult(Point accumulator)
    {
        return accumulator;
    }

    // Additional obfuscated helper methods that don't change functionality
    private bool ValidatePoint(Point point)
    {
        // Simple validation that doesn't affect calculations
        return point != null;
    }

    private BigInteger NormalizeModP(BigInteger value)
    {
        // Same as ((value % p) + p) % p but obfuscated
        return ((value % p) + p) % p;
    }
}
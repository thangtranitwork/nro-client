using System;

public class Math
{
	public const double PI = System.Math.PI;
    public static int abs(int i)
    {
        return (i <= 0) ? (-i) : i;
    }

    public static int Abs(int i)
	{
		return (i <= 0) ? (-i) : i;
	}

	public static int Min(int x, int y)
	{
		return (x >= y) ? y : x;
	}
	public static float Min(float x, float y)
	{
		return (x >= y) ? y : x;
	}
	public static double Sqrt(double d) => System.Math.Sqrt(d);
	public static int Max(int x, int y)
	{
		return (x <= y) ? y : x;
	}
	public static float Max(float x, float y)
	{
		return (x <= y) ? y : x;
	}
	public static long Max(long x, long y)
	{
		return (x <= y) ? y : x;
	}


	public static int Pow(int data, int x)
	{
		int num = 1;
		for (int i = 0; i < x; i++)
		{
			num *= data;
		}
		return num;
	}
}

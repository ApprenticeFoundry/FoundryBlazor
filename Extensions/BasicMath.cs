namespace FoundryBlazor.Extensions
{
    public static class BasicMath
{
	public static double toDouble(this string Value)
	{
		if (Value == null)
		{
			return 0;
		}
		else
		{
			double.TryParse(Value, out double OutVal);

			if (double.IsNaN(OutVal) || double.IsInfinity(OutVal))
			{
				return 0;
			}
			return OutVal;
		}
	}

	public static int toInteger(this string Value)
	{
		if (Value == null)
		{
			return 0;
		}
		else
		{
			if (int.TryParse(Value, out int OutVal))
			{
				return OutVal;
			}
			return 0;
		}
	}


	public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
	{
		return items.GroupBy(property).Select(x => x.First());
	}
	public static bool IsNullOrEmpty(this string str1)
	{
		var result = str1 == null || string.IsNullOrWhiteSpace(str1);
		return result;
	}
	public static bool Matches(this string str1, string str2)
    {
		if (str1.IsNullOrEmpty() && str2.IsNullOrEmpty()) return true;
        var result = str1?.ToLower() == str2?.ToLower();
        return result;
    }

}

static public class IoBTMath
{
	public static double toDouble(string Value)
	{
		return Value.toDouble();
	}

	public static int toInteger(string Value)
	{
		return Value.toInteger();
	}

	public static double toRad(this double ang)
	{
		return ang * Math.PI / 180;
	}

	public static double toDeg(this double ang)
	{
		return ang * 180 / Math.PI;
	}


    public static double DoubleBetween(this Random rand, double min, double max)
    {
        double value = min + (max - min) * rand.NextDouble();
        return value;
    }
}

}
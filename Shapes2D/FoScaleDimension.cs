using FoundryRulesAndUnits.Extensions;
using System;
using System.Collections.Generic;

namespace FoundryRulesAndUnits.Units;


// this is implied to be in units of length with meters as base unit
// but we don't enforce that here - we leave it to the user to be consistent
// we just provide the conversion and arithmetic support
// for example, you can create a FoScaleDimension in "cm" or "in"
// but you should not create one in "kg" or "s" - that would be nonsensical
// we also provide some legacy compatibility methods for common operations
// but we recommend using the As() method from MeasuredValue for conversions
public class FoScaleDimension : MeasuredValue
{
	public FoScaleDimension() 
	{
		Init(1, "m"); // Base class handles everything!
	}
	public FoScaleDimension(double value, string units)
	{
		Init(value, units); // Base class handles everything!
	}

	public FoScaleDimension Assign(double value, string? units)
	{
		Init(value, units); // Base class handles everything!
		return this;
	}

	public FoScaleDimension Assign(FoScaleDimension source)
	{
		Init(source.Value(), source.U); // Base class handles everything!
		return this;
	}

	public FoScaleDimension Copy()
	{
		var copy = new FoScaleDimension();
		copy.Init(Value(), Internal());
		return copy;
	}

	// Static factory methods removed - use UnitSystem.CreateUnit<T>() instead
	// Example: factory.CreateLength(1000, "m") for kilometers

	// As() method inherited from MeasuredValue - no override needed!

	// Legacy compatibility methods
	public int AsPixels()
	{
		// Handle pixel conversion with manual fallback when UnitGroup injection unavailable
		try
		{
			return (int)Math.Round(As("px"));
		}
		catch (InvalidOperationException)
		{
			// Manual conversion fallback for common units to pixels (96 DPI standard)
			// This handles cases where UnitGroup injection is not available
			var valueInMeters = I switch
			{
				"m" => V,
				"cm" => V * 0.01,
				"mm" => V * 0.001,
				"in" => V * 0.0254,
				"ft" => V * 0.3048,
				"px" => V / (96.0 / 0.0254), // Convert px back to meters first, then to px (identity)
				_ => throw new InvalidOperationException($"Cannot convert {I} to pixels without UnitGroup injection")
			};

			// Convert meters to pixels (96 DPI: 96 pixels per inch, 0.0254 meters per inch)
			var pixelsPerMeter = 96.0 / 0.0254;
			return (int)Math.Round(valueInMeters * pixelsPerMeter);
		}
	}

	public static bool operator <(FoScaleDimension left, FoScaleDimension right) => left.Value() < right.Value();
	public static bool operator >(FoScaleDimension left, FoScaleDimension right) => left.Value() > right.Value();
	public static bool operator ==(FoScaleDimension left, FoScaleDimension right) => Math.Abs(left.Value() - right.Value()) < 1e-10;
	public static bool operator !=(FoScaleDimension left, FoScaleDimension right) => !(left == right);

	// Override Equals and GetHashCode to be consistent with == operator
	public override bool Equals(object? obj) => obj is FoScaleDimension other && this == other;
	public override int GetHashCode() => Value().GetHashCode();

	public static FoScaleDimension operator +(FoScaleDimension left, FoScaleDimension right)
	{
		var result = new FoScaleDimension(left._unitGroup);
		result.Init(left.Value() + right.Value(), left.Internal());
		return result;
	}

	public static FoScaleDimension operator -(FoScaleDimension left, FoScaleDimension right)
	{
		var result = new FoScaleDimension(left._unitGroup);
		result.Init(left.Value() - right.Value(), left.Internal());
		return result;
	}

	public static FoScaleDimension operator *(double left, FoScaleDimension right)
	{
		var result = new FoScaleDimension(right._unitGroup);
		result.Init(left * right.Value(), right.Internal());
		return result;
	}

	public static FoScaleDimension operator *(FoScaleDimension left, double right)
	{
		var result = new FoScaleDimension(left._unitGroup);
		result.Init(left.Value() * right, left.Internal());
		return result;
	}

	public static FoScaleDimension operator /(FoScaleDimension left, double right)
	{
		var result = new FoScaleDimension(left._unitGroup);
		result.Init(left.Value() / right, left.Internal());
		return result;
	}

	public static double operator /(FoScaleDimension left, FoScaleDimension right) => left.Value() / right.Value();


}





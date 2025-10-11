using FoundryRulesAndUnits.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoundryRulesAndUnits.Units;


// this is implied to be in units of length with meters as base unit
// but we don't enforce that here - we leave it to the user to be consistent
// we just provide the conversion and arithmetic support
// for example, you can create a FoScaleDimension in "cm" or "in"
// but you should not create one in "kg" or "s" - that would be nonsensical
// we also provide some legacy compatibility methods for common operations
// but we recommend using the As() method from MeasuredValue for conversions
public class FoScaleDimension : IMeasuredValue 
{

	public double V = 0.0;
	public string I = "";  //internal storage units
	public string U = "";  //reporting  input and output units

	public IReadOnlyList<UnitDefinition> Definitions { get; } = new List<UnitDefinition>
	{
		// Length units (meters as base) - All-scale measurements
		UnitDefinition.BaseUnit("m", "meters", UnitFamilyName.Length),
		UnitDefinition.LinearUnit("km", "kilometers", UnitFamilyName.Length, 1000.0),        // 1 km = 1000 m
        UnitDefinition.LinearUnit("dm", "decimeters", UnitFamilyName.Length, 0.1),           // 1 dm = 0.1 m  
        UnitDefinition.LinearUnit("cm", "centimeters", UnitFamilyName.Length, 0.01),         // 1 cm = 0.01 m
        UnitDefinition.LinearUnit("mm", "millimeters", UnitFamilyName.Length, 0.001),        // 1 mm = 0.001 m
        UnitDefinition.LinearUnit("in", "inches", UnitFamilyName.Length, 0.0254),            // 1 in = 0.0254 m
        UnitDefinition.LinearUnit("ft", "feet", UnitFamilyName.Length, 0.3048),              // 1 ft = 0.3048 m
        UnitDefinition.LinearUnit("yd", "yards", UnitFamilyName.Length, 0.9144),              // 1 yd = 0.9144 m
        UnitDefinition.LinearUnit("mi", "miles", UnitFamilyName.Length, 1609.34),            // 1 mi = 1609.34 m
        UnitDefinition.LinearUnit("px", "pixels", UnitFamilyName.Length, 1.0 / 96.0 * 0.0254), // 96 DPI
		UnitDefinition.LinearUnit("pc", "picas", UnitFamilyName.Length, 0.0254 * 12.0 / 72.0), // 1 pica = 12 points, 1 point = 1/72 inch
		UnitDefinition.LinearUnit("pt", "points", UnitFamilyName.Length, 0.0254 / 72.0),      // 1 point = 1/72 inch
	};
	
	public FoScaleDimension() 
	{
		Init(1, "m"); // Base class handles everything!
	}
	public FoScaleDimension(double value, string units)
	{
		Init(value, units); // Base class handles everything!
	}
	public double Init(double value, string? units = null)
	{
		U = units ?? "m";
		I = "m";

		// Convert to base units for internal storage using UnitGroup
		if (I != U)
		{
			var found = Definitions.FirstOrDefault(u => u.Symbol == U);
			if (found == null)
			{
				throw new ArgumentException($"Unknown unit: {U}");
			}
			V = found.ConvertToBase(value);
		}
		else
		{
			V = value;
		}
		return V;
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

	// As() method for unit conversion
	public virtual double As(string units)
	{
		if (units == I) return V;
		
		var found = Definitions.FirstOrDefault(u => u.Symbol == units);
		if (found == null)
		{
			throw new ArgumentException($"Unknown unit: {units}");
		}
		
		return found.ConvertFromBase(V);
	}

	// Legacy compatibility methods
	public int AsPixels()
	{
		// Convert meters to pixels (96 DPI: 96 pixels per inch, 0.0254 meters per inch)
		var pixelsPerMeter = 96.0 / 0.0254;
		return (int)Math.Round(V * pixelsPerMeter);
	}

	public double Value() => V;
	public string Internal() => I;
	public string Units() => U;

	// IMeasuredValue interface implementation
	public void SetValue(double value)
	{
		V = value;
	}

	public void SetDisplayUnits(string units)
	{
		U = units;
	}

	public string Debug()
	{
		return $"{Value()}({Internal()}) {Units()}";
	}

	public string AsString(string units)
	{
		return $"{As(units)} {units}";
	}

	public string Format(string format)
	{
		var value = As(Units());
		return $"{value.ToString(format, System.Globalization.CultureInfo.CurrentCulture)} {Units()}";
	}

	public override string ToString()
	{
		return AsString(Units());
	}

	public double BaseValue() => V;
	public string BaseUnits() => I;
	public string DisplayUnits() => U;

	public string InternalRepresentation()
	{
		return $"V={V:G}, I='{I}', U='{U}'";
	}

	public string BaseUnitInfo()
	{
		return $"{V:G} {I}";
	}

	public string DisplayInfo()
	{
		return $"{As(U):G} {U}";
	}

	public string ConversionInfo()
	{
		if (U == I)
			return $"{As(U):G} {U} (no conversion needed)";

		return $"{As(U):G} {U} = {V:G} {I} (base units)";
	}

	public string DetailedDebug()
	{
		return $"Display: {As(U):G} {U} | Base: {V:G} {I} | Family: Length | V/I/U: {V:G}/'{I}'/'{U}'";
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
		var result = new FoScaleDimension();
		result.Init(left.Value() + right.Value(), left.Internal());
		return result;
	}

	public static FoScaleDimension operator -(FoScaleDimension left, FoScaleDimension right)
	{
		var result = new FoScaleDimension();
		result.Init(left.Value() - right.Value(), left.Internal());
		return result;
	}

	public static FoScaleDimension operator *(double left, FoScaleDimension right)
	{
		var result = new FoScaleDimension();
		result.Init(left * right.Value(), right.Internal());
		return result;
	}

	public static FoScaleDimension operator *(FoScaleDimension left, double right)
	{
		var result = new FoScaleDimension();
		result.Init(left.Value() * right, left.Internal());
		return result;
	}

	public static FoScaleDimension operator /(FoScaleDimension left, double right)
	{
		var result = new FoScaleDimension();
		result.Init(left.Value() / right, left.Internal());
		return result;
	}

	public static double operator /(FoScaleDimension left, FoScaleDimension right) => left.Value() / right.Value();


}





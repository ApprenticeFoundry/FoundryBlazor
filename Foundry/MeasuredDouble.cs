using System;
namespace FoundryBlazor;

public enum Direction
{
    N = 0,
    S = 180,
    E = 90,
    W = 270
}

public class UnitFamily
{
    private readonly string name;
    protected Units? DefaultUnits { get; set; }

    public UnitFamily(string name)
    {
        this.name = name;
    }

    public string Name { get { return name; } }
}

public class DistanceUnitFamily: UnitFamily
{
    public DistanceUnitFamily(): this("Distance")
    {
    }

    public DistanceUnitFamily(string name):base(name)
    {
    }
}


public class Units
{
    private readonly string name;
    private readonly UnitFamily? parentFamily;
    public Units(string name, UnitFamily? parent=null)
    {
        this.name = name;
        parentFamily = parent;
    }

    public string Name { get { return name; } }
    public UnitFamily? Family { get { return parentFamily; } }
}

public class MeasuredValue<T>
{
    protected T? Value { get; set; }
    protected Units? BaseUnits { get; set; }
    protected UnitFamily? UnitCategory { get; set; }
}

public enum DistanceUnits
{
    miles,
    meters,
    feet,
    inches
}

public class Distance: MeasuredValue<double>
{
    public Distance()
    {
        BaseUnits = new Units("m"); //meters
        UnitCategory = new UnitFamily(this.GetType().Name);
    }

    
    public Distance Feet(double value){
        this.Value = value / 3.28084;
        return this;
    }

    public Distance Miles(double value){
        this.Value = value * 1609.34;
        return this;
    }
    
    public Distance MilliMeters(double value){
        this.Value = value / 1000.0;
        return this;
    }

    public Distance KiloMeters(double value){
        this.Value = value * 1000.0;
        return this;
    }
}

public enum TimeUnits
{
    ms,
    sec,
    min,
    hrs,
    days
}

public class Time: MeasuredValue<double>
{
    public Time()
    {
        BaseUnits = new Units("sec"); //seconds
        UnitCategory = new UnitFamily(this.GetType().Name);
    }



    public Time SetValue(double value, TimeUnits units){
    
        if ( units == TimeUnits.days)
            return Days(value);

        return this;
    }
    public Time Days(double value){
        this.Value = value * 86400.0;
        return this;
    }

    public Time Hours(double value){
        this.Value = value / 3600.0;
        return this;
    }

    
    public Time Minutes(double value){
        this.Value = value / 60.0;
        return this;
    }
        public Time MicroSeconds(double value){
        this.Value = 1000.0 * value;
        return this;
    }
}

public class Angle: MeasuredValue<double>
{
    public Angle()
    {
        BaseUnits = new Units("deg"); //seconds
        UnitCategory = new UnitFamily(this.GetType().Name);
    }
    public Angle Degrees(double value){
        this.Value = value;
        return this;
    }
    public Angle Radians(double value){
        return this.Degrees(value * 180.0 / Math.PI);
    }

    public Angle IncrementDegrees(double value) {
        this.Value += value;
        return this;
    }
    public Angle IncrementRadians(double value){
        return this.IncrementDegrees(value * 180.0 / Math.PI);
    }

    public double Degrees(){
        return this.Value;
    }
    public double Radians(){
        return this.Value * Math.PI / 180.0;
    }
}

public class Speed: MeasuredValue<double>
{

    public Speed()
    {
        BaseUnits = new Units("m/s"); //meters per second
        UnitCategory = new UnitFamily(this.GetType().Name);
    }

    public Speed MetersPerSecond(double value){
        this.Value = value;
        return this;
    }

    public Speed MilesPerHour(double value){
        this.MetersPerSecond(value / 3600.0 * 1609.34);
        return this;
    }

    public Speed KilometersPerHour(double value){
        this.MetersPerSecond(value / 3600.0 * 1000);
        return this;
    }

    public double MetersPerSecond(){
        return this.Value;
    }

    public double KiloMetersPerSecond(){
        return this.Value / 1000.0;
    }

    public double MilesPerSecond(){
        return this.Value / 1609.34;
    }
}
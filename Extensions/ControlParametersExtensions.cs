using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using System.Reflection;

namespace FoundryBlazor.Extensions;


public static class ControlParametersExtension
{

    public static ControlParameters EstablishMetaData(this FoBase source, string field, string value)
    {
        source.AddMetaData(field, value);
        var result = source.MetaData();

        return result;
    }

    public static double GetDouble(this ControlParameters cn, string field)
    {
        var data = cn.Find(field);
        var value = data == null ? 0.0 : double.Parse(data.ToString()!);
        return value;
    }

    public static double SetDouble(this ControlParameters cn, string field, double value)
    {
        cn.Establish(field, value);
        return value;
    }

    public static T SetObject<T>(this ControlParameters cn, string field, T value) where T: class
    {
        var result = CodingExtensions.Dehydrate<T>(value, true);
        cn.Establish(field, result);
        return value;
    }

    public static T? GetObject<T>(this ControlParameters cn, string field) where T: class
    {
        var value = cn.Find(field);
        var result = value == null ? default : CodingExtensions.Hydrate<T>(value.ToString()!, true);
        return result;
    }

    public static string SetValue(this ControlParameters cn, string field, string value)
    {
        cn.Establish(field, value);
        return value;
    }

    public static ControlParameters Write<T>(this ControlParameters cn, T source)
    {
        var spec = BindingFlags.Instance | BindingFlags.Public;
        var properties = source!.GetType().GetProperties(spec);
        foreach (var property in properties)
        {
            var field = property.Name;
            var value = property.GetValue(source);
            if (value != null)
                cn.Establish(field, value.ToString()!);
        };
        return cn;
    }

    public static T Read<T>(this ControlParameters cn)
    {
        var spec = BindingFlags.Instance | BindingFlags.Public;
        var source = Activator.CreateInstance<T>();
        var properties = source!.GetType().GetProperties(spec);
        foreach (var property in properties)
        {
            var field = property.Name;
            var value = cn.Find(field);
            property.SetValue(source, value);
        };
        return source;
    }

}

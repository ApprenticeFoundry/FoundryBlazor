using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;


namespace FoundryBlazor.Extensions
{

	public static class CodingExtensions
	{

		public static String RemoveExtension(this String str)
		{
			return Path.GetFileNameWithoutExtension(str);
		}
		public static String RemoveVersion(this String str)
		{
			if (!str.Contains('_'))
			{
				return str;
			}
			var parts = str.Split('_');
			var allButLast = parts.SkipLast(1).ToList();
			var retVal = String.Join("_", allButLast);
			return retVal;
		}

		public static IEnumerable<T> DistinctUsing<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
		{
			return items.GroupBy(property).Select(x => x.First());
		}


		public static String UrlEncode(this String str)
		{
			return HttpUtility.UrlEncode(str);
		}

		public static String UrlEncode(this String str, Encoding e)
		{
			return HttpUtility.UrlEncode(str, e);
		}

		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (T element in source)
				action(element);
		}

		public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
		{
			foreach (var value in list)
			{
				await func(value);
			}
		}

		public static string Dehydrate<T>(T target, bool includeFields) where T : class
		{
			var options = new JsonSerializerOptions()
			{
				IncludeFields = includeFields,
				IgnoreReadOnlyFields = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
			};

			var result = JsonSerializer.Serialize(target, typeof(T), options);
			return result;
		}

		public static string Dehydrate(object target, Type type, bool includeFields) 
		{	
			var options = new JsonSerializerOptions()
			{
				IncludeFields = includeFields,
				IgnoreReadOnlyFields = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
			};
			var result = JsonSerializer.Serialize(target, type, options);
			return result;
		}

		public static string EncodeFieldNamesAsCSV(this object source, char d = '\u002C')
		{
			var list = new List<string>();
			var flist = from field in source.GetType().GetFields() where field.IsPublic select field;

			foreach (FieldInfo field in flist)
			{
				list.Add(field.Name);
			}
			return string.Join(d,list);
		}

		public static string EncodeFieldDataAsCSV(this object source, char d = '\u002C')
		{
			var list = new List<string>();
			var flist = from field in source.GetType().GetFields() where field.IsPublic select field;

			foreach (FieldInfo field in flist)
			{
				var value = field.GetValue(source);
				list.Add(value?.ToString() ?? "");
			}
			return string.Join(d,list);
		}

		public static int DecodeFieldDataAsCSV(this object source, string[] data)
		{

			int i = 0;
			var flist = from field in source.GetType().GetFields() where field.IsPublic select field;

			foreach (FieldInfo field in flist)
			{
				var value = data[i++];

				if (field.FieldType == typeof(double))
				{
					field.SetValue(source, double.Parse(value));
				}
				else if (field.FieldType == typeof(int))
				{
					field.SetValue(source, int.Parse(value));
				}
				else if (field.FieldType == typeof(bool))
				{
					field.SetValue(source, bool.Parse(value));
				}
				else if (field.FieldType == typeof(string))
				{
					field.SetValue(source, value);
				} else 
				{
					throw new ArgumentException($"Cannot DecodeFieldDataAsCSV for {field.Name}");
				} 
			}
			return flist.Count();
		}

		public static void CopyNonNullProperties<T>(this T source, T dest)
		{
			var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

			foreach (PropertyInfo prop in plist)
			{
				var value = prop.GetValue(source, null);
				if (value != null && !string.IsNullOrEmpty(value.ToString()))
					prop.SetValue(dest, value, null);
			}
		}

		public static void CopyNonNullFields<T>(this T source, T dest)
		{
			var plist = typeof(T).GetFields();

			foreach (FieldInfo prop in plist)
			{
				var value = prop.GetValue(source);
				if (value != null && !string.IsNullOrEmpty(value.ToString()))
					prop.SetValue(dest, value);
			}
		}

		public static void CopyFields<T>(this T source, T dest)
		{
			var plist = typeof(T).GetFields();

			foreach (FieldInfo prop in plist)
			{
				var value = prop.GetValue(source);
				prop.SetValue(dest, value);
			}
		}

		public static void CopyFieldsTo<T,U>(this T source, U dest)
		{
			var flist1 = typeof(T).GetFields();
			var flist2 = typeof(T).GetFields();

			foreach (FieldInfo fld1 in flist1)
			{
				var value = fld1.GetValue(source);
				var fld2 = flist2.Where(x => x.Name.Matches(fld1.Name)).FirstOrDefault();
				fld2?.SetValue(dest, value);
			}
		}

		public static void CopyProperties<T>(this T source, T dest)
		{
			var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

			foreach (PropertyInfo prop in plist)
			{
				var value = prop.GetValue(source, null);
				prop.SetValue(dest, value, null);
			}
		}

		public static void CopyPropertiesTo<T, U>(this T source, U dest)
		{
			var plistsource = from prop1 in typeof(T).GetProperties() where prop1.CanRead select prop1;
			var plistdest = from prop2 in typeof(U).GetProperties() where prop2.CanWrite select prop2;

			foreach (PropertyInfo destprop in plistdest)
			{
				var sourceprops = plistsource.Where((p) => p.Name == destprop.Name &&
				  destprop.PropertyType.IsAssignableFrom(p.GetType()));
				foreach (PropertyInfo sourceprop in sourceprops)
				{ // should only be one
					var value = sourceprop.GetValue(source, null);
					destprop.SetValue(dest, value, null);
				}
			}
		}

		public static U CreateUDTOfromSPEC<S, U>(this S source)
		{
			var result = Activator.CreateInstance<U>();

			var plistsource = from prop1 in typeof(S).GetProperties() where prop1.CanRead select prop1;
			var flistdest = from field1 in typeof(U).GetFields() where field1.IsPublic select field1;

			foreach (FieldInfo destField in flistdest)
			{
				var sourceProp = plistsource.Where((p) => p.Name == destField.Name).FirstOrDefault();
				if (sourceProp != null)
				{
					var value = sourceProp.GetValue(source, null);
					destField.SetValue(result, value);
				}
			}
			return result;
		}


		public static S CreateSPECfromUDTO<U, S>(this U source)
		{
			var result = Activator.CreateInstance<S>();

			var flistsource = from field1 in typeof(U).GetFields() where field1.IsPublic select field1;
			var plistdest = from prop1 in typeof(S).GetProperties() where prop1.CanRead select prop1;

			foreach (PropertyInfo destProp in plistdest)
			{
				var sourceField = flistsource.Where((f) => f.Name == destProp.Name).FirstOrDefault();
				if (sourceField != null)
				{
					var value = sourceField.GetValue(source);
					destProp.SetValue(result, value);
				}
			}
			return result;
		}

	}
}

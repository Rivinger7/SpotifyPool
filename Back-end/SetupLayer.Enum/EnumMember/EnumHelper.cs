using System.Reflection;

namespace SetupLayer.Enum.EnumMember
{
	public static class EnumHelper
	{
		public static string Name<T>(this T srcValue) => GetCustomName(typeof(T).GetField(srcValue?.ToString() ?? string.Empty));
		private static string GetCustomName(FieldInfo? fi)
		{
			Type type = typeof(CustomName);

			Attribute? attr = null;
			if (fi is not null)
			{
				attr = Attribute.GetCustomAttribute(fi, type);
			}

			return (attr as CustomName)?.Name ?? string.Empty;
		}

		public static List<string> GetValuesAsString<T>() where T : global::System.Enum
		{
			return global::System.Enum.GetValues(typeof(T))
				.Cast<T>()
				.Select(arg => GetCustomName(arg))
			.ToList();
		}

		private static string GetCustomName<T>(T enumValue) where T : global::System.Enum
		{
			var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
			var attributes = fieldInfo.GetCustomAttributes(typeof(CustomNameAttribute), false);
			return attributes.Length > 0 ? ((CustomNameAttribute)attributes[0]).Name : enumValue.ToString();
		}


		[AttributeUsage(AttributeTargets.Field)]
		public class CustomNameAttribute : Attribute
		{
			public string Name { get; }
			public CustomNameAttribute(string name)
			{
				Name = name;
			}
		}


	}
}

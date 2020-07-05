#if !NEW_REFLECTION
namespace System
{
	internal static class ShimReflectionExtensions
	{
		public static Type GetTypeInfo(this Type type) => type;
	}
}
#endif

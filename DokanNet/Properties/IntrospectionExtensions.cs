#if NET10 || NET11 || NET20 || NET30 || NET35 || NET40
#define NET40_OR_LESS
#endif

#if NET40_OR_LESS

namespace System.Reflection
{
    /// <summary>Contains methods for converting <see cref="T:System.Type" /> objects.</summary>
    /// <remarks>This extension is missing in .NET framework 4.0 and below.</remarks>
    public static class IntrospectionExtensions
    {
        /// <summary>Returns the <see cref="T:System.Reflection.TypeInfo" /> representation of the specified type.</summary>
        /// <returns>The converted object.</returns>
        /// <param name="type">The type to convert.</param>
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }
    }
}
#endif
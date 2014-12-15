using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace System
{
    /// <summary>
    /// Provides extension methods for enumerations.
    /// </summary>
    public static class EnumExtensions
    {
        private static Dictionary<string, Attribute> enumAttributeCache = new Dictionary<string, Attribute>();
        private static readonly object enumAttributeCacheLock = new object();

        /// <summary>
        /// Gets the description for the enumeration value.
        /// If a enumeration member is decorated with the <see cref="DescriptionAttribute"/>
        /// that description is returned; otherwise only the enumeration value name is returned.
        /// </summary>
        /// <param name="value">The value to get the description for.</param>
        /// <returns>The description for the value.</returns>
        public static string GetDescription(this Enum value)
        {
            DescriptionAttribute attr = value.GetAttribute<DescriptionAttribute>();
            if (attr != null)
                return attr.Description;

            return value.ToString();
        }

        /// <summary>
        /// Gets the description for the enumeration value.
        /// If a enumeration member is decorated with the <see cref="DescriptionAttribute"/>
        /// that description is returned; otherwise only the enumeration value name is returned.
        /// </summary>
        /// <param name="value">The value to get the description for.</param>
        /// <returns>The description for the value.</returns>
        public static T GetAttribute<T>(this Enum value)
            where T: Attribute
        {
            Type enumType = value.GetType();
            string cacheKey = string.Format("{0}:{1}:{2}", enumType.FullName, value.ToString(), typeof(T).FullName);

            // double locking pattern in case of multi-threading
            if (!enumAttributeCache.ContainsKey(cacheKey))
            {
                lock (enumAttributeCacheLock)
                {
                    // still not in cache? get the value
                    if (!enumAttributeCache.ContainsKey(cacheKey))
                    {
                        Attribute attr = null;
                        MemberInfo[] memberInfo = enumType.GetMember(value.ToString());

                        // 'GetMember' always returns an array
                        if (memberInfo.Length > 0)
                        {
                            object[] attributes =
                                memberInfo[0].GetCustomAttributes(typeof(T), false);

                            // has 'Description' attribute?
                            // 'GetCustomAttributes' always returns an array
                            if (attributes.Length > 0)
                                attr = (Attribute)attributes[0];
                        }

                        // add to cache
                        enumAttributeCache[cacheKey] = attr;
                    }
                }
            }

            // cache contains the description now
            return enumAttributeCache[cacheKey] as T;
        }

        /// <summary>
        /// Determines whether the enumeration value is in the specified collection of values.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="values">The values to compare with.</param>
        /// <returns>
        /// <c>true</c> if the specified value is in; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIn(this Enum value, params Enum[] values)
        {
            if (!values.IsNullOrEmpty())
                return values.Contains(value);

            return false;
        }
    }
}

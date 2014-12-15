using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace System
{
    /// <summary>
    /// Provides extension methods for throwing exception when checking values.
    /// </summary>
    public static class ThrowExtensions
    {
        /// <summary>
        /// Throws an exception if the object is a null reference.
        /// </summary>
        /// <param name="value">The object to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>value</c> is a null reference.
        /// </exception>
        public static void ThrowIfNull(this object value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
        }

        /// <summary>
        /// Throws an exception if the string is a null reference or is empty.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>value</c> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <c>value</c> is an empty string.
        /// </exception>
        public static void ThrowIfNullOrEmpty(this string value, string paramName)
        {
            value.ThrowIfNull(paramName);

            if (value.Length == 0)
                throw new ArgumentException(
                    string.Format("String '{0}' cannot be empty.", paramName));
        }

        /// <summary>
        /// Throws an exception if the string is a null reference, is empty or 
        /// consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>value</c> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <c>value</c> is an empty string or consists only of white-space characters..
        /// </exception>
        public static void ThrowIfNullOrWhiteSpace(this string value, string paramName)
        {
            value.ThrowIfNullOrEmpty(paramName);

            // has any non-white-space character?
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return;
            }

            // if we get here, we've only found white-space characters
            throw new ArgumentException(
                string.Format("String '{0}' consists only of white-space characters.", paramName));
        }

        /// <summary>
        /// Throws an exception if the collection is a null reference or is empty.
        /// </summary>
        /// <param name="value">The collection to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>value</c> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <c>value</c> is an empty collection.
        /// </exception>
        public static void ThrowIfNullOrEmpty(this ICollection value, string paramName)
        {
            value.ThrowIfNull(paramName);

            if (value.Count < 1)
                throw new ArgumentException(
                    string.Format("Collection '{0}' cannot be empty.", paramName));
        }

        /// <summary>
        /// Throws if the value is less than the specified minimum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="min">The minimum that <c>value</c> can be.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <c>value</c> is less than <c>min</c>.
        /// </exception>
        public static void ThrowIfLess<T>(this T value, T min, string paramName)
            where T: struct, IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                throw new ArgumentOutOfRangeException(
                    paramName,
                    string.Format("Value '{0}' cannot be less than '{1}' but is '{2}'.", paramName, min, value));
        }

        /// <summary>
        /// Throws if the specified value is greater than the specified maximum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="max">The maximum that <c>value</c> can be.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <c>value</c> is greater than <c>max</c>.
        /// </exception>
        public static void ThrowIfGreater<T>(this T value, T max, string paramName)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(
                    paramName,
                    string.Format("Value '{0}' cannot be greater than '{1}' but is '{2}'.", paramName, max, value));
        }

        /// <summary>
        /// Throws if the specified value is less than zero.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <c>value</c> is less than zero.
        /// </exception>
        public static void ThrowIfNegative<T>(this T value, string paramName)
            where T : struct, IComparable
        {
            if (value.CompareTo(0) < 0)
                throw new ArgumentOutOfRangeException(
                    paramName,
                    string.Format("Value '{0}' cannot be less than zero but is '{1}'.", paramName, value));
        }

        /// <summary>
        /// Throws if the specified value is less than or equal to zero.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <c>value</c> is less than or equal to zero.
        /// </exception>
        public static void ThrowIfZeroOrNegative<T>(this T value, string paramName)
            where T : struct, IComparable
        {
            if (value.CompareTo(0) <= 0)
                throw new ArgumentOutOfRangeException(
                    paramName,
                    string.Format("Value '{0}' cannot be less than or equal to zero but is '{1}'.", paramName, value));
        }

        /// <summary>
        /// Throws if the specified value is less than the specified minimum
        /// or greater than the specified maximum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="min">The minimum that <c>value</c> can be.</param>
        /// <param name="max">The maximum that <c>value</c> can be.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <c>value</c> is less than <c>min</c> or greater than <c>max</c>.
        /// </exception>
        public static void ThrowIfOutOfRange<T>(this T value, T min, T max, string paramName)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                string message = string.Format("Value '{0}' cannot be less than '{1}' or greater than '{2}' but is '{3}'.",
                    paramName, min, max, value);

                throw new ArgumentOutOfRangeException(paramName, message);
            }
        }
    }
}

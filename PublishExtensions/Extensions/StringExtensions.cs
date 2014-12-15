using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using PHZH;

namespace System
{
    /// <summary>
    /// Provides extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        private static readonly Dictionary<string, string> UmlautReplacements = CreateUmlautReplacements();
        private static readonly char[] DefaultDelimiters = { ',', ';', ' ' };

        /// <summary>
        /// Creates the umlaut replacements dictionary.
        /// </summary>
        /// <returns>The created umlaut replacements dictionary</returns>
        private static Dictionary<string, string> CreateUmlautReplacements()
        {
            var dict = new Dictionary<string, string>(StringComparer.CurrentCulture);
            dict.Add("ä", "ae");
            dict.Add("ö", "oe");
            dict.Add("ü", "ue");
            dict.Add("Ä", "Ae");
            dict.Add("Ö", "Oe");
            dict.Add("Ü", "Ue");
            dict.Add("ß", "ss");
            dict.Add("ı", "i");
            return dict;
        }

        /// <summary>
        /// Gets the number of characters in the string.
        /// If the string is a null reference 0 is returned.
        /// </summary>
        /// <param name="text">The text to get the length for.</param>
        /// <returns>The number of characters in the string</returns>
        public static int Length(this string text)
        {
            if (text != null)
                return text.Length;

            return 0;
        }

        /// <summary>
        /// Cuts the text that it has the specified length.
        /// If the text is shorter than the specified length it is returned without cutting it.
        /// If the text is a null reference a null reference is returned.
        /// </summary>
        /// <param name="text">The text to cut.</param>
        /// <param name="length">The new length of the text.</param>
        /// <returns>The cutted text with the specified length.</returns>
        public static string Cut(this string text, int length)
        {
            return text.Cut(length, false);
        }

        /// <summary>
        /// Cuts the text that it has the specified length and appends an ellipsis (…) to indicate it was truncated.
        /// If the text is shorter than the specified length it is returned without cutting it.
        /// If the text is a null reference a null reference is returned.
        /// </summary>
        /// <param name="text">The text to cut.</param>
        /// <param name="length">The new length of the text.</param>
        /// <param name="useEllipsis"><c>true</c> to append an ellipsis if the text was cut; otherwise, <c>false</c>.</param>
        /// <returns>The cutted text with the specified length.</returns>
        public static string Cut(this string text, int length, bool useEllipsis)
        {
            if (useEllipsis)
                return text.Cut(length, "…");
            else
                return text.Cut(length, null);
        }

        /// <summary>
        /// Cuts the text that it has the specified length and appends the specified indicator to indicate it was truncated.
        /// If the text is shorter than the specified length it is returned without cutting it.
        /// If the text is a null reference a null reference is returned.
        /// </summary>
        /// <param name="text">The text to cut.</param>
        /// <param name="length">The new length of the text.</param>
        /// <param name="cutIndicator">The indicator to append if the text was cut; otherwise, <c>false</c>.</param>
        /// <returns>The cutted text with the specified length.</returns>
        public static string Cut(this string text, int length, string cutIndicator)
        {
            // not null and range within value?
            if (!text.IsNullOrEmpty() && text.Length > length)
            {
                // length greater than zero?
                if (length > 0)
                {
                    if (cutIndicator.Length() < length)
                        return text.Remove(length - cutIndicator.Length()) + cutIndicator;
                    else
                        return text.Remove(length);
                }
                else
                    return string.Empty;
            }

            return text;
        }

        /// <summary>
        /// Concatenates the members of the enumerable collection, using the specified
        /// separator between each member.
        /// </summary>
        /// <param name="values">A collection that contains the strings to concatenate.</param>
        /// <param name="separator">The string to use as a separator.</param>
        /// <returns>
        /// A string that consists of the members of values delimited by the separator string.
        /// If the collection is a null reference or empty, an empty string is returned.
        /// </returns>
        public static string Join(this IEnumerable<string> values, string separator)
        {
            // not null and range within value?
            if (!values.IsNullOrEmpty())
            {
                return string.Join(separator, values);
            }

            return string.Empty;
        }
        
        /// <summary>
        /// Converts the text into a <see cref="HashSet{T}"/> that is case-insensitive.
        /// The text is splitted up at ',' ';' and ' '.
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <returns>A HashSet containing the splitted up text.</returns>
        public static HashSet<string> ToHashSet(this string text)
        {
            return text.ToHashSet(null, DefaultDelimiters);
        }

        /// <summary>
        /// Converts the text into a <see cref="HashSet{T}"/> that is case-insensitive.
        /// The text is splitted up at the specified separators.
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <param name="separators">The separators to split up the text.</param>
        /// <returns>A HashSet containing the splitted up text.</returns>
        public static HashSet<string> ToHashSet(this string text, params char[] separators)
        {
            return text.ToHashSet(null, separators);
        }

        /// <summary>
        /// Converts the text into a <see cref="HashSet{T}"/>.
        /// The text is splitted up at ',' ';' and ' '.
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A HashSet containing the splitted up text.</returns>
        public static HashSet<string> ToHashSet(
            this string text, IEqualityComparer<string> comparer)
        {
            return text.ToHashSet(comparer, DefaultDelimiters);
        }

        /// <summary>
        /// Converts the text into a <see cref="HashSet{T}"/>.
        /// The text is splitted up at the specified separators.
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <param name="comparer">The comparer.</param>
        /// <param name="separators">The separators to split up the text.</param>
        /// <returns>A HashSet containing the splitted up text.</returns>
        public static HashSet<string> ToHashSet(
            this string text, IEqualityComparer<string> comparer, params char[] separators)
        {
            var result = new HashSet<string>(comparer ?? StringComparer.CurrentCultureIgnoreCase);

            if (!text.IsNullOrWhiteSpace())
            {
                // any separators specified?
                if (!separators.IsNullOrEmpty())
                {
                    string[] tokens = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string token in tokens)
                        result.Add(token.Trim());
                }
                else
                {
                    result.Add(text);
                }
            }

            return result;
        }

        /// <summary>
        /// Replaces each format item in the string with the text equivalent of a corresponding object's value.
        /// This function does not throw a format exception if the provided format string does not match the specified
        /// arguments to format the text with.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects in args.</returns>
        public static string TryFormat(this string format, params object[] args)
        {
            // not empty?
            if (!format.IsNullOrWhiteSpace() && !args.IsNullOrEmpty())
            {
                try
                {
                    return string.Format(format, args);
                }
                catch (FormatException ex)
                {
                    return string.Format(
                        "{0} | Failed to format with arguments: {1}, Error: {2}", 
                        format, args.ToItemString(), ex.Message);
                }
            }

            return format;
        }

        /// <summary>
        /// Indicates whether the specified string is null or an empty string.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value parameter is null or an empty string (""); otherwise, <c>false</c>.</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Indicates whether the specified string is null, empty, or consists only of
        /// white-space characters.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value parameter is null, empty, or consists only of white-space characters; otherwise, <c>false</c>.</returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
#if NET35
            return value.IsNullOrEmpty() || value.Trim().Length == 0;
#else
            return string.IsNullOrWhiteSpace(value);
#endif
        }

        /// <summary>
        /// Returns the string or the default value if the string is null, empty, or consists only of
        /// white-space characters.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="defaultValue">The default value to return if the string is null or empty.</param>
        /// <returns>The string or the default value.</returns>
        public static string OrDefault(this string value, string defaultValue)
        {
            return value.OrDefault(defaultValue, null);
        }

        /// <summary>
        /// Returns the string or the default value if the string is null, empty, or consists only of
        /// white-space characters.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="defaultValue">The default value to return if the string is null or empty.</param>
        /// <param name="args">An object array that contains zero or more objects to format the default value.</param>
        /// <returns>The string or the default value.</returns>
        public static string OrDefault(this string value, string defaultValue, params object[] args)
        {
            return value.IsNullOrWhiteSpace() ? defaultValue.TryFormat(args) : value;
        }

        /// <summary>
        /// Returns the string or an empty string if the string is null, empty, or consists only of
        /// white-space characters.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>The string or an empty string.</returns>
        public static string OrEmpty(this string value)
        {
            return value.OrDefault(string.Empty);
        }

        /// <summary>
        /// Returns the string or a null reference if the string is null, empty, or consists only of
        /// white-space characters.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>The string or a null reference.</returns>
        public static string OrNull(this string value)
        {
            return value.OrDefault(null);
        }

        /// <summary>
        /// Replaces all occurrences of each old value with the corresponding new value from the specified collection.
        /// </summary>
        /// <param name="text">The text to replace the old values with the new ones.</param>
        /// <param name="oldNewValues">The old values with their corresponding new values.</param>
        /// <returns>The text with all replaced values.</returns>
        public static string ReplaceAll(this string text, IEnumerable<KeyValuePair<string, string>> oldNewValues)
        {
            foreach (KeyValuePair<string, string> pair in oldNewValues)
                text = text.Replace(pair.Key, pair.Value);

            return text;
        }

        /// <summary>
        /// Replaces the Umlauts in the text.
        /// </summary>
        /// <param name="value">The value to replace the Umlauts in.</param>
        /// <returns>The text without Umlauts.</returns>
        public static string ReplaceUmlauts(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            // remove umlauts
            value = value.ReplaceAll(UmlautReplacements);

            // remove diacritics
            return value.RemoveDiacritics();
        }

        /// <summary>
        /// Removes the diacritics from the text.
        /// </summary>
        /// <param name="value">The text to remove the diacritics from.</param>
        /// <returns>The text with removed diacritics.</returns>
        private static string RemoveDiacritics(this string value)
        {
            string formD = value.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder(value.Length);

            for (int i = 0; i < formD.Length; i++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(formD[i]);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(formD[i]);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Removes all occurences of the specified characters within the specified string.
        /// </summary>
        /// <param name="s">The string to remove the characters from.</param>
        /// <param name="chars">The characters to remove.</param>
        /// <returns>A string with all specified characters removed.</returns>
        public static string RemoveAll(this string s, params char[] chars)
        {
            if (s.IsNullOrEmpty() || chars.IsNullOrEmpty())
                return s;

            // remove each char
            string result = string.Empty;
            for (int i = 0; i < s.Length; ++i)
            {
                if (!chars.Contains(s[i]))
                    result += s[i];
            }
            return result;
        }

        /// <summary>
        /// Removes all occurences of the specified values within the specified string.
        /// </summary>
        /// <param name="s">The string to remove the characters from.</param>
        /// <param name="values">The values to remove.</param>
        /// <returns>A string with all specified values removed.</returns>
        public static string RemoveAll(this string s, params string[] values)
        {
            if (s.IsNullOrEmpty() || values.IsNullOrEmpty())
                return s;

            // remove each char
            foreach (string value in values)
                s = s.Replace(value, string.Empty);

            return s;
        }

        /// <summary>
        /// Gets the substring before the first occurence of the specified value.
        /// </summary>
        /// <param name="s">The string to get the substring from.</param>
        /// <param name="value">The value to find.</param>
        /// <returns>
        /// The substring before the first occurrence of the specified value.
        /// If value is a null reference or an empty string the string is returned unmodified.
        /// If value was not found an empty string is returned.
        /// </returns>
        public static string SubstringBefore(this string s, string value)
        {
            if (s.IsNullOrEmpty() || value.IsNullOrEmpty())
                return s;

            int index = s.IndexOf(value);
            return index >= 0 ? s.Substring(0, index) : string.Empty;
        }

        /// <summary>
        /// Gets the substring before the last occurence of the specified value.
        /// </summary>
        /// <param name="s">The string to get the substring from.</param>
        /// <param name="value">The value to find.</param>
        /// <returns>
        /// The substring before the last occurrence of the specified value.
        /// If value is a null reference or an empty string the string is returned unmodified.
        /// If value was not found an empty string is returned.
        /// </returns>
        public static string SubstringBeforeLast(this string s, string value)
        {
            if (s.IsNullOrEmpty() || value.IsNullOrEmpty())
                return s;

            int index = s.LastIndexOf(value);
            return index >= 0 ? s.Substring(0, index) : string.Empty;
        }

        /// <summary>
        /// Gets the substring after the first occurence of the specified value.
        /// </summary>
        /// <param name="s">The string to get the substring from.</param>
        /// <param name="value">The value to find.</param>
        /// <returns>
        /// The substring after the first occurrence of the specified value.
        /// If value is a null reference or an empty string the string is returned unmodified.
        /// If value was not found an empty string is returned.
        /// </returns>
        public static string SubstringAfter(this string s, string value)
        {
            if (s.IsNullOrEmpty() || value.IsNullOrEmpty())
                return s;

            int index = s.IndexOf(value);
            return index >= 0 ? s.Substring(index + value.Length) : string.Empty;
        }

        /// <summary>
        /// Gets the substring after the last occurence of the specified value.
        /// </summary>
        /// <param name="s">The string to get the substring from.</param>
        /// <param name="value">The value to find.</param>
        /// <returns>
        /// The substring after the last occurrence of the specified value.
        /// If value is a null reference or an empty string the string is returned unmodified.
        /// If value was not found an empty string is returned.
        /// </returns>
        public static string SubstringAfterLast(this string s, string value)
        {
            if (s.IsNullOrEmpty() || value.IsNullOrEmpty())
                return s;

            int index = s.LastIndexOf(value);
            return index >= 0 ? s.Substring(index + value.Length) : string.Empty;
        }

        /// <summary>
        /// Determines whether this string is equal to the specified string.
        /// </summary>
        /// <param name="s">The first string.</param>
        /// <param name="value">The string to compare.</param>
        /// <returns>true, if the strings are equal; otherwise, false.</returns>
        public static bool EqualsIgnoreCase(this string s, string value)
        {
            return string.Equals(s, value, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Converts a string to a valid enumeration. If the conversion fails this function throws an exception.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>The enumeration representation of the specified string.</returns>
        public static T ToEnum<T>(this string value)
            where T : struct
        {
            return value.ToEnumInternal((T?)null);
        }

        /// <summary>
        /// Converts a string to a valid enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The default value to use if the conversion fails.</param>
        /// <returns>The enumeration representation of the specified string or the default value.</returns>
        public static T ToEnum<T>(this string value, T defaultValue)
            where T : struct
        {
            return value.ToEnumInternal((T?)defaultValue);
        }

        /// <summary>
        /// Converts a string to a valid enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The default value to use if the conversion fails.</param>
        /// <returns>The enumeration representation of the specified string or the default value.</returns>
        private static T ToEnumInternal<T>(this string value, T? defaultValue)
            where T : struct
        {
            // string not empty?
            if (!value.IsNullOrWhiteSpace())
            {
                T result;
                if (Enum.TryParse(value, true, out result))
                {
                    // make sure that the value exists in the enum
                    if (Enum.IsDefined(typeof(T), result) | result.ToString().Contains(","))
                        return result;
                }
            }

            // no default value supplied?
            if (defaultValue != null)
                return defaultValue.Value;
            else
                throw new Exception(String.Format("String '{0}' could not be converted to enum '{1}'.", value, typeof(T).Name));
        }

        /// <summary>
        /// Indents each line of the string with the specified indent value.
        /// </summary>
        /// <param name="value">The value to indent.</param>
        /// <param name="indent">The value to indent each line with.</param>
        /// <returns>The indented string.</returns>
        public static string Indent(this string value, string indent)
        {
            return value.Indent(indent, false);
        }

        /// <summary>
        /// Indents each line of the string with the specified indent value.
        /// </summary>
        /// <param name="value">The value to indent.</param>
        /// <param name="indent">The value to indent each line with.</param>
        /// <param name="ignoreFirstLine"><c>true</c> to not apply the indent on the first line; otherwise, <c>false</c>.</param>
        /// <returns>The indented string.</returns>
        public static string Indent(this string value, string indent, bool ignoreFirstLine)
        {
            if (value == null)
                return ignoreFirstLine ? string.Empty : indent;

            if (indent.IsNullOrEmpty())
                return value;
            
            bool isFirst = true;
            using (StringWriter writer = new StringWriter())
            {
                using (StringReader reader = new StringReader(value))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!isFirst)
                            writer.WriteLine();

                        if (isFirst && ignoreFirstLine)
                            writer.Write(line);
                        else
                            writer.Write(indent + line);

                        isFirst = false;
                    }

                    return writer.ToString();
                }
            }
        }

        /// <summary>
        /// Repeats the character the specified times.
        /// </summary>
        /// <param name="c">The character to repeat.</param>
        /// <param name="count">The number of times to repeat the character.</param>
        /// <returns>A string containing the repeated character.</returns>
        public static string Repeat(this char c, int count)
        {
            return new string(c, count);
        }

        /// <summary>
        /// Repeats the string the specified times.
        /// </summary>
        /// <param name="s">The string to repeat.</param>
        /// <param name="count">The number of times to repeat the character.</param>
        /// <returns>A string containing the repeated string.</returns>
        public static string Repeat(this string s, int count)
        {
            // special cases
            if (s == null)
                return null;

            if (s.Length == 0)
                return s;
            
            if (count < 1)
                return string.Empty;
            
            // combine the string
            var sb = new StringBuilder(count * s.Length); 
            for (int i = 0; i < count; i++)
                sb.Append(s);
            return sb.ToString();
        }

        public static string EnsureEndingDirectorySeparator(this string s)
        {
            if (!s.IsNullOrWhiteSpace())
            {
                if (s[s.Length - 1] != Path.DirectorySeparatorChar)
                    return s += Path.DirectorySeparatorChar;
            }
            return s;
        }

        public static string RemoveEndingDirectorySeparator(this string s)
        {
            if (s.IsNullOrWhiteSpace())
                return s;
            
            return s.TrimEnd(Path.DirectorySeparatorChar);
        }
    }
}

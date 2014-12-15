using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using PHZH;

namespace System.Collections.Generic
{
    /// <summary>
    /// Provides extension methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Merges the two sequences by using the default equality comparer.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the input sequences.</typeparam>
        /// <param name="first">The collection whose distinct elements form the first set for the merge.</param>
        /// <param name="second">The collection whose distinct elements form the second set for the merge.</param>
        /// <returns>A collection that contains the elements from both input sequences, excluding duplicates.</returns>
        public static IEnumerable<T> Merge<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            return first.Merge(second, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Merges the two sequences by using a specified comparer.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the input sequences.</typeparam>
        /// <param name="first">The collection whose distinct elements form the first set for the merge.</param>
        /// <param name="second">The collection whose distinct elements form the second set for the merge.</param>
        /// <param name="comparer">The comparer to compare values.</param>
        /// <returns>A collection that contains the elements from both input sequences, excluding duplicates.</returns>
        public static IEnumerable<T> Merge<T>(
            this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            if (first == null)
                return second ?? new List<T>();
            else if (second == null)
                return first;

            return first.Union(second, comparer);
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="source">The collection to add the items to.</param>
        /// <param name="collection">The collection whose elements should be added to the end of the collection.</param>
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> collection)
        {
            foreach (T item in collection)
                source.Add(item);
        }

        /// <summary>
        /// Complements the collection with the specified items if they aren't already part of the collection.
        /// </summary>
        /// <param name="source">The collection to complement.</param>
        /// <param name="collection">The items to complement the collection with.</param>
        public static void ComplementWith<T>(this ICollection<T> source, IEnumerable<T> collection)
        {
            source.ComplementWith(collection, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Complements the collection with the specified items if they aren't already part of the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to complement.</param>
        /// <param name="collection">The items to complement the collection with.</param>
        /// <param name="comparer">The comparer to compare values.</param>
        public static void ComplementWith<T>(
            this ICollection<T> source, IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            source.ThrowIfNull("source");

            if (!collection.IsNullOrEmpty())
            {
                foreach (T item in collection)
                {
                    if (!source.Contains(item, comparer))
                        source.Add(item);
                }
            }
        }

        /// <summary>
        /// Sorts the elements of a collection in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">The collection to sort.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        public static void SortBy<TSource, TKey>(this ICollection<TSource> source, Func<TSource, TKey> keySelector)
        {
            IEnumerable<TSource> sortedItems = source.OrderBy(keySelector);
            source.Clear();
            source.AddRange(sortedItems);
        }

        /// <summary>
        /// Sorts the elements of a collection in descending order according to a key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">The collection to sort.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        public static void SortByDescending<TSource, TKey>(this ICollection<TSource> source, Func<TSource, TKey> keySelector)
        {
            IEnumerable<TSource> sortedItems = source.OrderByDescending(keySelector);
            source.Clear();
            source.AddRange(sortedItems);
        }

        /// <summary>
        /// Sorts the elements in the entire collection using the default comparer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to sort.</param>
        public static void Sort<T>(this ICollection<T> source)
        {
            source.Sort(Comparer<T>.Default);
        }

        /// <summary>
        /// Sorts the elements in the entire collection using the specified comparer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to sort.</param>
        /// <param name="comparer">The comparer to use.</param>
        public static void Sort<T>(this ICollection<T> source, IComparer<T> comparer)
        {
            List<T> list = new List<T>(source);
            list.Sort(comparer);
            source.Clear();
            source.AddRange(list);
        }

        /// <summary>
        /// Performs the specified action on each element.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The source enumeration.</param>
        /// <param name="action">The delegate to perform on each element.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            // use indexer if possible
            if (source is IList<T>)
            {
                IList<T> list = (IList<T>)source;
                for (int i = 0; i < list.Count; ++i)
                    action(list[i]);
            }
            else
            {
                for (int i = 0; i < source.Count(); ++i)
                    action(source.ElementAt(i));
            }
        }

        /// <summary>
        /// Determines whether the collection is a null reference or does not contain any items.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the collection.</typeparam>
        /// <param name="collection">The collection to test.</param>
        /// <returns>
        /// <c>true</c> if the collection is a null reference or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection == null || collection.Count() == 0)
                return true;

            return false;
        }

        /// <summary>
        /// Gets a string that represents the items in the collection that are separated by ','.
        /// If the collection is empty or a null reference an empty string is returned.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the collection.</typeparam>
        /// <param name="collection">The collection to get the string for.</param>
        /// <returns>A string that represents the items in the collection.</returns>
        public static string ToItemString<T>(this IEnumerable<T> collection)
        {
            return collection.ToItemString(",", null);
        }

        /// <summary>
        /// Gets a string that represents the items in the collection.
        /// If the collection is empty or a null reference an empty string is returned.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the collection.</typeparam>
        /// <param name="collection">The collection to get the string for.</param>
        /// <param name="separator">The separator to use.</param>
        /// <returns>A string that represents the items in the collection.</returns>
        public static string ToItemString<T>(this IEnumerable<T> collection, string separator)
        {
            return collection.ToItemString(separator, null);
        }

        /// <summary>
        /// Gets a string that represents the items in the collection.
        /// If the collection is empty or a null reference an empty string is returned.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the collection.</typeparam>
        /// <param name="collection">The collection to get the string for.</param>
        /// <param name="separator">The separator to use.</param>
        /// <param name="format">The format for formatting each value with.</param>
        /// <returns>A string that represents the items in the collection.</returns>
        public static string ToItemString<T>(this IEnumerable<T> collection, string separator, string format)
        {
            return collection.ToItemString(separator, format, false);
        }

        /// <summary>
        /// Gets a string for debugging that represents the items in the collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the collection.</typeparam>
        /// <param name="collection">The collection to get the string for.</param>
        /// <returns>A string that represents the items in the collection.</returns>
        public static string ToDebugString<T>(this IEnumerable<T> collection)
        {
            return collection.ToDebugString(", ", null);
        }

        /// <summary>
        /// Gets a string for debugging that represents the items in the collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the collection.</typeparam>
        /// <param name="collection">The collection to get the string for.</param>
        /// <param name="separator">The separator to use.</param>
        /// <returns>A string that represents the items in the collection.</returns>
        public static string ToDebugString<T>(this IEnumerable<T> collection, string separator)
        {
            return collection.ToDebugString(separator, null);
        }

        /// <summary>
        /// Gets a string for debugging that represents the items in the collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the collection.</typeparam>
        /// <param name="collection">The collection to get the string for.</param>
        /// <param name="separator">The separator to use.</param>
        /// <param name="format">The format for formatting each value with.</param>
        /// <returns>A string that represents the items in the collection.</returns>
        public static string ToDebugString<T>(this IEnumerable<T> collection, string separator, string format)
        {
            return collection.ToItemString(separator, format, true);
        }

        /// <summary>
        /// Gets a string that represents the items in the collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the collection.</typeparam>
        /// <param name="collection">The collection to get the string for.</param>
        /// <param name="separator">The separator to use.</param>
        /// <param name="format">The format for formatting each value with.</param>
        /// <param name="forDebug">true, if the returned value is used for debugging; otherwise, false.</param>
        /// <returns>
        /// A string that represents the items in the collection.
        /// </returns>
        private static string ToItemString<T>(this IEnumerable<T> collection, string separator, string format, bool forDebug)
        {
            // collection empty
            if (collection == null)
                return forDebug ? "<null>" : string.Empty;

            if (collection.Count() < 1)
                return forDebug ? "<empty>" : string.Empty;

            // adjust format
            format = format.OrNull();

            // get string representation of each item
            List<string> itemTexts = collection.ToList().ConvertAll<string>(item =>
            {
                if (item == null)
                    return forDebug ? "<null>" : null;

                if (format != null && item is IFormattable)
                    return ((IFormattable)item).ToString(format, null);
                else
                    return item.ToString();
            });

            // remove null and empty values if not for debugging
            if (!forDebug)
                itemTexts.RemoveAll(item => item.IsNullOrWhiteSpace());

            // join the texts
            return itemTexts.Join(separator);
        }

        /// <summary>
        /// Gets the value for the specified key or a null reference if no such key exists.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary to get the value from.</param>
        /// <param name="key">The key to get the value for.</param>
        /// <returns>The value for the specified key or a null reference if no such key exists.</returns>
        public static TValue ValueOrNull<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
            where TValue : class
        {
            return dict.ValueOrDefault(key, (TValue)null);
        }

        /// <summary>
        /// Gets the value for the specified key or the specified default values if no such key exists.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary to get the value from.</param>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="defaultValue">The default value to use if the key does not exist.</param>
        /// <returns>The value for the specified key or the specified default values if no such key exists.</returns>
        public static TValue ValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            TValue result;
            if (dict.TryGetValue(key, out result))
                return result;

            return defaultValue;
        }


        /// <summary>
        /// Gets the value for the specified key or a null reference if no such key exists.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="coll">The dictionary to get the value from.</param>
        /// <param name="key">The key to get the value for.</param>
        /// <returns>The value for the specified key or a null reference if no such key exists.</returns>
        public static TItem ValueOrNull<TKey, TItem>(this KeyedCollection<TKey, TItem> coll, TKey key)
            where TItem : class
        {
            return coll.ValueOrDefault(key, (TItem)null);
        }

        /// <summary>
        /// Gets the value for the specified key or the specified default values if no such key exists.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="coll">The dictionary to get the value from.</param>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="defaultValue">The default value to use if the key does not exist.</param>
        /// <returns>The value for the specified key or the specified default values if no such key exists.</returns>
        public static TItem ValueOrDefault<TKey, TItem>(this KeyedCollection<TKey, TItem> coll, TKey key, TItem defaultValue)
        {
            if (coll.Contains(key))
                return coll[key];

            return defaultValue;
        }

        /// <summary>
        /// Removes an element at the specified index from the array.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="source">The array to remove the element from.</param>
        /// <param name="index">The index of the element to remove.</param>
        /// <returns>A new array with the element at the specified index removed.</returns>
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            // null or out of range?
            if (source == null || index < 0 || index >= source.Length)
                return source;
            
            // copy the items
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }
    }
}

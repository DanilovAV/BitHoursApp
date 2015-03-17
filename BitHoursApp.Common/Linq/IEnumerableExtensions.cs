using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitHoursApp.Common.Linq
{
    public static class CollectionExtensions
    {      
        public static T SingleOrDefaultNoThrow<T>(this IEnumerable<T> source)
            where T : class
        {
            return source == null || source.Count() != 1 ? default(T) : source.First();
        }

        public static T SingleOrDefaultNoThrow<T>(this IEnumerable<T> source, Func<T, bool> predicate)
            where T : class
        {
            return source == null ? default(T) : source.Where(predicate).SingleOrDefaultNoThrow();
        }
    
        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> roots, Func<T, IEnumerable<T>> childrenSelector)
        {
            return roots.SelectMany(e => Traverse(e, childrenSelector));
        }
    
        public static IEnumerable<T> Traverse<T>(T root, Func<T, IEnumerable<T>> childrenSelector)
        {
            yield return root;
            foreach (var subItem in childrenSelector(root).Traverse(childrenSelector))
            {
                yield return subItem;
            }
        }

        public static bool EqualsToArray<T>(this T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2)) return true;
            if (a1 == null || a2 == null) return false;
            if (a1.Length != a2.Length) return false;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }

        public static void AddRange(this IList list, IEnumerable listToAdd)
        {
            foreach (object item in listToAdd)
            {
                list.Add(item);
            }
        }

        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> listToAdd)
        {
            foreach (T item in listToAdd)
            {
                list.Add(item);
            }
        }

        public static void RemoveRange<T>(this ICollection<T> list, IEnumerable<T> items)
        {
            items.ForEach(item => list.Remove(item));
        }
   
        public static void RemoveRange(this IList list, IEnumerable items)
        {
            items.ForEach(list.Remove);
        }
       
        public static void ForEach(this IEnumerable enumerable, Action<object> action)
        {
            foreach (var item in enumerable)
                action(item);
        }
        
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T t in enumerable)
                action(t);
        }

        public static bool Replace<T>(this IList<T> list, T oldItem, T newItem, IEqualityComparer<T> comparer = null)
        {
            if (comparer == null)
                comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < list.Count; i++)
            {
                if (!comparer.Equals(oldItem, list[i])) continue;
                list[i] = newItem;
                return true;
            }
            return false;
        }
        
        public static void Replace(this IList list, IEnumerable items)
        {
            list.Clear();
            list.AddRange(items);
        }
        
        public static void Replace<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            collection.AddRange(items);
        }

        /// <summary>
        /// Create a single string from a sequenc of items, separated by the provided <paramref name="separator"/>,
        /// and with the conversion to string done by the given <paramref name="converter"/>.
        /// </summary>
        /// <remarks>This method does basically the same thing as <see cref="string.Join(string,string[])"/>,
        /// but will work on any sequence of items, not just arrays.</remarks>
        /// <typeparam name="T">Type of items in the sequence.</typeparam>
        /// <param name="sequence">Sequence of items to convert.</param>
        /// <param name="separator">Separator to place between the items in the string.</param>
        /// <param name="converter">The conversion function to change T -&gt; string.</param>
        /// <returns>The resulting string.</returns>
        public static string Concatenate<T>(this IEnumerable<T> sequence, string separator, Func<T, string> converter)
        {
            var sb = new StringBuilder();
            sequence.Aggregate(sb, (builder, item) =>
            {
                if (builder.Length > 0)
                    builder.Append(separator);
                builder.Append(converter(item));
                return builder;
            });
            return sb.ToString();
        }

        /// <summary>
        /// Create a single string from a sequenc of items, separated by the provided <paramref name="separator"/>,
        /// and with the conversion to string done by the item's <see cref='object.ToString'/> method.
        /// </summary>
        /// <remarks>This method does basically the same thing as <see cref="string.Join(string,string[])"/>,
        /// but will work on any sequence of items, not just arrays.</remarks>
        /// <typeparam name="T">Type of items in the sequence.</typeparam>
        /// <param name="sequence">Sequence of items to convert.</param>
        /// <param name="separator">Separator to place between the items in the string.</param>
        /// <returns>The resulting string.</returns>
        public static string Concatenate<T>(this IEnumerable<T> sequence, string separator)
        {
            return sequence.Concatenate(separator, item => item.ToString());
        }
      
        public static int RemoveByCondition<T>(this ICollection<T> collection, Func<T, bool> condition)
        {
            var itemsToRemove = collection.Where(condition).ToList();
            int count = 0;
            foreach (var itemToRemove in itemsToRemove)
            {
                collection.Remove(itemToRemove);
                count++;
            }
            return count;
        }

        public static T MinOrDefault<T>(this IEnumerable<T> sequence)
        {
            return sequence.Any() ? sequence.Min() : default(T);
        }
   
        public static T MinOrDefault<T>(this IEnumerable<T> sequence, T defaultValue)
        {
            return sequence.Any() ? sequence.Min() : defaultValue;
        }
    
        public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> sequence, Func<TSource, TResult> selector)
        {
            var selectorResult = sequence.Select(selector);
            return MinOrDefault(selectorResult);
        }
     
        public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> sequence, Func<TSource, TResult> selector, TResult defaultValue)
        {
            var selectorResult = sequence.Select(selector);
            return MinOrDefault(selectorResult, defaultValue);
        }
    
        public static T MaxOrDefault<T>(this IEnumerable<T> sequence)
        {
            return sequence.Any() ? sequence.Max() : default(T);
        }
       
        public static T MaxOrDefault<T>(this IEnumerable<T> sequence, T defaultValue)
        {
            return sequence.Any() ? sequence.Max() : defaultValue;
        }

        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> sequence, Func<TSource, TResult> selector)
        {
            var selectorResult = sequence.Select(selector);
            return MaxOrDefault(selectorResult);
        }

        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> sequence, Func<TSource, TResult> selector, TResult defaultValue)
        {
            var selectorResult = sequence.Select(selector);
            return MaxOrDefault(selectorResult, defaultValue);
        }
    }
}
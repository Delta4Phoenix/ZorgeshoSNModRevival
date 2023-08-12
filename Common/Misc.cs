using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

// fix for C# 9.0 (pre-5.0 .NET) for init-only properties
namespace System.Runtime.CompilerServices { static class IsExternalInit {} }

namespace Common
{
	static class MiscExtensions
	{
		public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source) => source.ForEach(e => target.Add(e));

		public static void Add<T>(this List<T> target, T item, int count) where T: struct
		{
			for (int i = 0; i < count; i++)
				target.Add(item);
		}

		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if (sequence != null)
			{
				var enumerator = sequence.GetEnumerator();

				while (enumerator.MoveNext())
					action(enumerator.Current);
			}

			return sequence;
		}
	}


	static class ArrayExtensions
	{
		public static T[] Init<T>(this T[] array) where T: new()
		{
			for (int i = 0; i < array.Length; i++)
				array[i] = new T();
			return array;
		}

		public static bool IsNullOrEmpty(this Array array) => array == null || array.Length == 0;

		public static bool Contains<T>(this T[] array, T val) => Array.IndexOf(array, val) != -1;

		public static int FindIndex<T>(this T[] array, int beginIndex, int endIndex, Predicate<T> predicate) =>
			Array.FindIndex(array, beginIndex, endIndex - beginIndex, predicate);

		public static int FindIndex<T>(this T[] array, Predicate<T> predicate) =>
			Array.FindIndex(array, predicate);

		public static T[] Append<T>(this T[] array1, T[] array2)
		{
			if (array1 != null && array2.IsNullOrEmpty()) 
				return array1;

			if (array1 == null)
				return array2 ?? new T[0];

			T[] newArray = new T[array1.Length + array2.Length];

			array1.CopyTo(newArray, 0);
			array2.CopyTo(newArray, array1.Length);

			return newArray;
		}

		public static T[] SubArray<T>(this T[] array, int indexBegin, int indexEnd = -1)
		{
			try
			{
				int length = (indexEnd == -1? array.Length - 1: indexEnd) - indexBegin + 1;
				T[] newArray = new T[length];

				Array.Copy(array, indexBegin, newArray, 0, length);
				return newArray;
			}
			catch (Exception e) { Log.msg(e); return null; }
		}
	}


	static partial class StringExtensions
	{
		public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

		public static string Format(this string s, object arg0) => string.Format(s, arg0);
		public static string Format(this string s, object arg0, object arg1) => string.Format(s, arg0, arg1);
		public static string Format(this string s, object arg0, object arg1, object arg2) => string.Format(s, arg0, arg1, arg2);
		public static string Format(this string s, params object[] args) => string.Format(s, args);

		public static bool StartsWith(this string s, string str) => s.StartsWith(str, StringComparison.Ordinal);

		public static string RemoveFromEnd(this string s, int count) => s.Length > count? s.Remove(s.Length - count): s;
		public static StringBuilder RemoveFromEnd(this StringBuilder sb, int count) => sb.Length > count? sb.Remove(sb.Length - count, count): sb;

		public static string ClampLength(this string s, int length)
		{
			if (length < 5 || s.Length <= length)
				return s;

			return s.Remove(length / 2, s.Length - length + 3).Insert(length / 2, "...");
		}

		public static void SaveToFile(this string s, string localPath)
		{
			try { File.WriteAllText(FormatFileName(localPath), s); }
			catch (Exception e) { Log.msg(e); }
		}

		public static void AppendToFile(this string s, string localPath)
		{
			try { File.AppendAllText(FormatFileName(localPath), s + Environment.NewLine); }
			catch (Exception e) { Log.msg(e); }
		}

		static string FormatFileName(string filename) => Paths.FormatFileName(filename, "txt");
	}

	static class MathUtils
	{
		// allows to cycle in [0, n - 1] in both directions
		public static int Mod(int x, int n) => (x % n + n) % n;

		public static bool IsInRange(int x, int max) => IsInRange(x, 0, max);
		public static bool IsInRange(int x, int min, int max) => x >= min && x <= max;
	}

	class UniqueIDs
	{
		readonly HashSet<string> allIDs = new();
		readonly Dictionary<string, int> nonUniqueIDs = new();

		public bool EnsureUniqueID(ref string id, bool nonUniqueIDsWarning = true)
		{
			if (allIDs.Add(id)) // if this is new id, do nothing
				return true;

			nonUniqueIDs.TryGetValue(id, out int counter);
			nonUniqueIDs[id] = ++counter;

			id += "." + counter;
#if DEBUG
			if (nonUniqueIDsWarning)
				$"UniqueIDs: fixed ID: {id}".logWarning();

			Debug.assert(allIDs.Add(id)); // checking updated id just in case
#endif
			return false;
		}

		public bool FreeID(string id) => allIDs.Remove(id); // non-unique IDs can't be freed (? for now)
	}
}
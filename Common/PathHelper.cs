using System.IO;
using System.Reflection;

namespace Common
{
	static partial class Paths
	{
		public static readonly string modRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;

		// completes path with modRootPath if needed
		public static string MakeRootPath(string filename)
		{
			return filename.IsNullOrEmpty()? filename: ((Path.IsPathRooted(filename)? "": modRootPath) + filename);
		}

		// adds extension if it's absent
		public static string EnsureExtension(string filename, string ext)
		{
			return filename.IsNullOrEmpty()? filename: (Path.HasExtension(filename)? filename: filename + (ext.StartsWith(".")? "": ".") + ext);
		}

		// checks path for filename and creates intermediate directories if needed
		public static void EnsurePath(string filename)
		{
			if (filename.IsNullOrEmpty())
				return;

			string path = MakeRootPath(Path.GetDirectoryName(filename));

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
		}

		// completes path with modRootPath and adds extension if needed
		public static string FormatFileName(string filename, string extension, bool nullIfEmpty = false) =>
			filename.IsNullOrEmpty()? (nullIfEmpty? null: filename): MakeRootPath(EnsureExtension(filename, extension));
	}
}
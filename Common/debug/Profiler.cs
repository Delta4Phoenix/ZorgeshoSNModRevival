using System;
using System.Diagnostics;

namespace Common
{
	static partial class Debug
	{
		public static Profiler DProfiler(string message = null, string filename = null, bool allowNested = true) =>
#if DEBUG
			allowNested || Profiler.ProfilerCount == 0? new Profiler(message, filename): null;
#else
			null;
#endif

		public class Profiler: IDisposable
		{
			public static double LastResult { get; private set; }
#if DEBUG
			public static int ProfilerCount { get; private set; }

			readonly string message = null;
			readonly string filename = null;
			readonly Stopwatch stopwatch = null;
			readonly long mem = GC.GetTotalMemory(false);

			static string UformatFileName(string filename) => Paths.FormatFileName(filename, "prf");

			public Profiler(string message, string filename)
			{
				ProfilerCount++;

				this.message = message;
				this.filename = UformatFileName(filename);

				stopwatch = Stopwatch.StartNew();
			}

			public void Dispose()
			{
				stopwatch.Stop();

				ProfilerCount--;
				assert(ProfilerCount >= 0);

				LastResult = stopwatch.Elapsed.TotalMilliseconds;

				if (message == null)
					return;

				long m = GC.GetTotalMemory(false) - mem;
				string memChange = $"{(m > 0? "+": "")}{(Math.Abs(m) > 1024L * 1024L? (m / 1024L / 1024L + "MB"): (m / 1024L + "KB"))} ({m})";

				string result = $"{message}: {LastResult} ms; mem alloc:{memChange}";
				$"PROFILER: {result}".log();

				if (filename != null)
					result.AppendToFile(filename);
			}

			public static void UlogCompare(double prevResult, string filename = null)
			{
				string res = $"PROFILER: DIFF {prevResult} ms -> {LastResult} ms, delta: {(LastResult - prevResult) / prevResult * 100f:F2} %";
				res.log();

				if (filename != null)
					res.AppendToFile(UformatFileName(filename));
			}
#else
			public Profiler(string _0, string _1) {}
			public void Dispose() {}
#endif
		}
	}
}
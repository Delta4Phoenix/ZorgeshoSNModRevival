using System.Collections;

using Nautilus;
using UnityEngine;

namespace Common.Stasis
{
	static class Utils
	{
		public static EventDescription GetDescription(this EventInstance eventInstance)
		{
			eventInstance.GetDescription(out EventDescription desc);
			return desc;
		}

		static int GetInstanceCount(this EventDescription desc)
		{
			desc.GetInstanceCount(out int count);
			return count;
		}

		public static IEnumerator ReleaseAllEventInstances(string eventPath, int waitFramesMax = 10)
		{																									$"Utils.releaseAllEventInstances: {eventPath}".logDbg();
			FMODUWE.GetEventInstance(eventPath, out EventInstance eventInstance);
			var desc = eventInstance.GetDescription();
			eventInstance.release();

			int count = desc.getInstanceCount();															$"Utils.releaseAllEventInstances: instances count = {count}".logDbg();

			if (count == 0)
				yield break;

			desc.releaseAllInstances();
			yield return new WaitWhile(() => desc.getInstanceCount() > 0 && waitFramesMax-- > 0);
		}
	}
}
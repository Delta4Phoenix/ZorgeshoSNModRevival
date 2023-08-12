using UnityEngine;
using Common;

namespace MiscObjects
{
	static class Utils
	{
		public static void addStorageToPrefab(GameObject prefab, GameObject storagePrefab)
		{
			var storageRoot = prefab.CreateChild(storagePrefab.GetChild("StorageRoot"));

			var container = prefab.AddComponent<StorageContainer>();
			container.storageRoot = storageRoot.GetComponent<ChildObjectIdentifier>();
			container.prefabRoot = prefab;
		}
	}
}
using UnityEngine;

using Common;
using Common.Stasis;
using Common.Crafting;

namespace StasisTorpedo
{
	partial class StasisTorpedo
	{
		class StasisExplosion: MonoBehaviour
		{
			void Start()
			{
				StasisSphereCreator.Create(transform.position, Main.config.stasisTime, Main.config.stasisRadius);
				Destroy(gameObject);
			}
		}

		public static TorpedoType TorpedoType { get; private set; }

		public static void InitPrefab(GameObject gasTorpedoPrefab)
		{
			if (TorpedoType != null)
				return;

			if (!gasTorpedoPrefab)
			{
				"StasisTorpedo.initPrefab: invalid prefab for GasTorpedo!".logError();
				return;
			}

			var explosionPrefab = new GameObject("StasisExplosion", typeof(StasisExplosion));
			SMLHelper.V2.Assets.ModPrefabCache.AddPrefab(explosionPrefab, false);

			var torpedoPrefab = PrefabUtils.storePrefabCopy(gasTorpedoPrefab);
			torpedoPrefab.GetComponent<SeamothTorpedo>().explosionPrefab = explosionPrefab;

			TorpedoType = new() { techType = TechType, prefab = torpedoPrefab };
		}
	}
}
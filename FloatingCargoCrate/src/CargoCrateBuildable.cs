﻿using UnityEngine;

using Common;
using Common.Crafting;

namespace FloatingCargoCrate
{
	class FloatingCargoCrate: PoolCraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechInfo getTechInfo() => new
		(
			new (TechType.Titanium, Main.config.cheapBlueprint? 3: 6),
			new (TechType.Silicone, Main.config.cheapBlueprint? 1: 2),
			new (TechType.AirBladder, Main.config.cheapBlueprint? 1: 2)
		);

		public override void patch()
		{
			TechType = register(L10n.ids_crateName, L10n.ids_crateDesc);

			setTechTypeForUnlock(TechType.AirBladder);
#if GAME_SN
			addToGroup(TechGroup.ExteriorModules, TechCategory.ExteriorOther);
#elif GAME_BZ
			addToGroup(TechGroup.ExteriorModules, TechCategory.ExteriorModule);
#endif
		}

		protected override void initPrefabPool()
		{
			addPrefabToPool(TechType.SmallStorage);

			string prefabPath = $"WorldEntities/{(Mod.Consts.isGameSN? "Doodads/Debris/Wrecks/Decoration": "Alterra/Base")}/";
			addPrefabToPool(prefabPath + Main.config.crateModelName, false);
		}

		protected override GameObject getGameObject(GameObject[] prefabs)
		{
			var prefab = prefabs[0];
			var model = prefab.GetChild("3rd_person_model");

			var modelCargo = model.CreateChild(prefabs[1].GetChild(Main.config.crateModelName), localScale: Vector3.one * 2.1f);

			model.GetComponentInChildren<Animator>().enabled = false;

			var rigidbody = prefab.GetComponent<Rigidbody>();
			rigidbody.mass = Main.config.crateMass;
			rigidbody.angularDrag = 1f; //default 1f
			prefab.GetComponent<Stabilizer>().uprightAccelerationStiffness = 0.3f; //default 2.0f

			prefab.DestroyComponent<DeployableStorage>();
			prefab.DestroyComponentInChildren<PickupableStorage>();
			prefab.DestroyComponent<Pickupable>();

			prefab.DestroyComponent<FPModel>();
			prefab.DestroyComponent<FPModel>();

			prefab.DestroyComponent<LiveMixin>();
#if GAME_SN
			prefab.DestroyComponentInChildren<SmallStorage>();
#endif

			var storageContainer = PrefabUtils.initStorage(prefab, Main.config.storageWidth, Main.config.storageHeight, L10n.Str(L10n.ids_hoverText), L10n.Str(L10n.ids_storageLabel));
#if GAME_SN // TODO fix for BZ
			storageContainer.modelSizeRadius *= 3f;
#endif
			storageContainer.enabled = false; // disable until fully constructed


			prefab.DestroyChildren("LidLabel", "1st_person_model");

			var storagePillow = model.GetChild("floating_storage_cube_tp");
			storagePillow.DestroyChildren("Floating_storage_container_geo", "Floating_storage_lid_geo");
			storagePillow.SetTransform(localPos: new Vector3(0f, 1.155f, 0.18f), localScale: new Vector3(3.4f, 7.0f, 8.1f));

			var collider = prefab.GetChild("StorageContainer").GetComponent<BoxCollider>();
			collider.center = new Vector3(0.013f, 1.23f, 0.204f);
			collider.size = new Vector3(2.4f, 2.292f, 2.854f);

			collider = prefab.GetChild("collider_main").GetComponent<BoxCollider>();
			collider.center = new Vector3(0.014f, -0.415f, 0.173f);
			collider.size = new Vector3(2.47f, 0.89f, 3.0f);

			prefab.GetComponent<SkyApplier>().renderers = new[] { model.GetComponentInChildren<Renderer>(), modelCargo.GetComponent<Renderer>() };


			var constructable = PrefabUtils.initConstructable(prefab, model);
			constructable.allowedOutside = true;
			constructable.forceUpright = true;

			constructable.placeMaxDistance = 7f;
			constructable.placeMinDistance = 5f;
			constructable.placeDefaultDistance = 6f;

			prefab.AddComponent<FloatingCargoCrateControl>();

			return prefab;
		}
	}
}